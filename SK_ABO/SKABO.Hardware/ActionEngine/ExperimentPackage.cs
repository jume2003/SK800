using SKABO.Hardware.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKABO.MAI.ErrorSystem;
using SKABO.ResourcesManager;
using SKABO.Hardware.RunBJ;
using SKABO.ActionGeneraterEngine;
using SKABO.Common.Models.GEL;
using SKABO.Common.Enums;
using SKABO.Common;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.TestStep;
using SKABO.Common.Utils;
using static SKABO.ResourcesManager.ActionPoint;
using SKABO.Common.Models.NotDuplex;

namespace SKABO.ActionEngine
{
    public class ExperimentPackage
    {
        public static int g_package_id { get; set; } = 0;
        public List<LogicActionUnity> action_list = new List<LogicActionUnity>();
        private int sort_value { get; set; }//排序变量
        public int lever { get; set; }//优先级
        public int gel_mask_id { get; set; }//gel卡一维码id
        public int gel_test_id { get; set; }//gel卡一维码（卡类型）
        public double hatch_cur_time { get; set; }//孵育时间
        public double hatch_time { get; set; }//孵育时间
        public int sort_index { get; set; } = 0;//排序
        public List<string> samples_barcode { get; set; } = new List<string>();//病人一维码
        public List<string> volunteers_barcode { get; set; } = new List<string>();//献血员一维码
        public List<int> samples_barcode_index { get; set; } = new List<int>();//病人所在载架号
        public bool is_jyjs { get; set; } = false;//是否加样结束
        public bool is_crossmatching { get; set; } = false;//是否交叉配血
        public int piercer_index { get; set; } = 0;//所在破孔位
        public string start_time;//开始时间
        public DateTime start_time_data { get; set; }
        public bool is_updataed { get; set; } = false;
        public int package_id { get; set; } = 0;
        public bool is_double { get; set; } = false;//是否双工
        //省卡模式
        public bool is_open { get; set; }//是否已经开孔了
        public int ren_fen { get; set; }//一共多小人份
        public double after_kktime { get; set; }//开孔后保留时间
        public bool is_used_gel { get; set; }//是否使用省卡模式
        public int gel_type { get; set; }//孔数
        public int batch_id { get; set; }//批次号(相同批次号时没开孔才可以合并)

        public static ExperimentPackage Create(List<T_GelStep> aaction_list, string gelmask,string samplebar,string volunteerbar, int lever,int geltype,int renfen,int afterkktime, bool isusedgel,int gel_test_id,bool is_crossmatching,int batch_id,bool is_double)
        {
            var exp = new ExperimentPackage();
            exp.lever = lever;
            exp.AddSampleCode(samplebar);
            exp.AddvolunteerCode(volunteerbar);
            exp.gel_mask_id = ResManager.getInstance().AddGelMaskByID(gelmask);
            exp.start_time = DateTime.Now.ToString("yyyyMMddHHmmss");
            exp.start_time_data = DateTime.Now;
            exp.gel_test_id = gel_test_id;
            exp.batch_id = batch_id;
            exp.is_crossmatching = is_crossmatching;
            exp.is_open = false;
            exp.package_id = g_package_id;
            exp.is_double = is_double;
            g_package_id++;
            foreach (var act in aaction_list)
            {
                act.ExperPackage = exp;
                exp.action_list.Add(new LogicAction(act));
            }
            //省卡模式
            exp.gel_type = geltype;
            exp.ren_fen = renfen;
            exp.after_kktime = afterkktime*60*1000;
            exp.is_used_gel = isusedgel;
            var economize = new T_GelStep();
            economize.ExperPackage = exp;
            economize.StepClass = TestStepEnum.ECONOMIZECOUNTTIME;
            if (isusedgel) exp.action_list.Add(new LogicAction(economize));
            int index = 0;
            bool is_sort_index_ok = false;
            foreach(var act in exp.action_list)
            {
                act.GetGelStep().StepIndex = index;
                act.GetGelStep().SampleIndex = 0;
                if(is_sort_index_ok==false)
                exp.sort_index = exp.gel_test_id;
                if (act.GetGelStep().StepClass == TestStepEnum.ZKDFY)
                {
                    ZKDFYStepParameter parameter = act.GetGelStep().StepParamters.ToInstance<ZKDFYStepParameter>();
                    exp.hatch_time = parameter.YsTime;
                    if (is_sort_index_ok == false)
                    {
                        exp.sort_index += 100000;
                        is_sort_index_ok = true;
                    }
                }
                index++;
            }
            exp.UpdataSampleIndex();
            if (is_crossmatching) exp.SortCrossMatching();
            return exp;
        }
        //省卡合并
        public void Combination(ExperimentPackage expack)
        {
            is_jyjs = false;
            //添加病人code
            AddSampleCode(expack.GetSampleCode(0));
            AddvolunteerCode(expack.GetAddvolunteerCode(0));
            //更新gel_mask_id
            if (expack.gel_mask_id < gel_mask_id)
            {
                gel_mask_id = expack.gel_mask_id;
                is_open = expack.is_open;
            }
            //更新所有动作的指向病人code
            foreach (var act in expack.action_list)
            {
                act.GetGelStep().SampleIndex = samples_barcode.Count() - 1;
            }
            //合并在加样结束前
            int jyjs_index = 0;
            for (int i=0;i<action_list.Count();i++)
            {
                if (action_list[i].GetGelStep().StepClass == TestStepEnum.JYJS)
                {
                    jyjs_index = i;
                    break;
                }
            }
            //插入所有分配液体
            //if (jyjs_index != 0) expack.action_list.Reverse();
            foreach (var act in expack.action_list)
            {
                if(jyjs_index==0)
                {
                    action_list.Add(act);
                }
                else if (act.GetGelStep().StepClass == TestStepEnum.FPYT)
                {
                    action_list.Insert(jyjs_index, act);
                    jyjs_index++;
                }

            }
            //人份满了就把省卡去掉
            if(ren_fen==samples_barcode.Count())
            {
                for (int i = action_list.Count - 1; i >= 0; i--)
                {
                    if (action_list[i].GetGelStep().StepClass == TestStepEnum.ECONOMIZECOUNTTIME)
                    {
                        action_list.Remove(action_list[i]);
                    }
                }
            }
            //只留一个省卡
            bool is_find_econtime = false;
            for (int i = action_list.Count - 1; i >= 0; i--)
            {
                if (action_list[i].GetGelStep().StepClass == TestStepEnum.ECONOMIZECOUNTTIME)
                {
                    if(is_find_econtime)
                    {
                        action_list.Remove(action_list[i]);
                    }
                    is_find_econtime = true;
                }
            }
            //如果已开孔把开孔去掉
            if (is_open)
            {
                for (int i = action_list.Count - 1; i >= 0; i--)
                {
                    if (action_list[i].GetGelStep().StepClass == TestStepEnum.KaiKongGel)
                    {
                        action_list.Remove(action_list[i]);
                    }
                }
            }
            //排序
            for (int i = 0; i < action_list.Count(); i++)
            {
                foreach(var actt in action_list[i].GetAllActions())
                {
                    actt.GetGelStep().ExperPackage = this;
                }
                if (is_crossmatching == false)
                action_list[i].GetGelStep().StepIndex = i;
            }
            action_list = action_list.OrderBy(c => c.GetGelStep().SampleIndex).ToList();
            action_list = action_list.OrderBy(c => c.GetGelStep().StepIndex).ToList();
            //交叉配血
            if (is_crossmatching) SortCrossMatching();
            UpdataSampleIndex();
            expack.action_list.Clear();
            expack.samples_barcode.Clear();
            expack.volunteers_barcode.Clear();
        }
        //排序交叉配血
        public void SortCrossMatching()
        {
            //把生理盐水同一排序
            var slys_tem_list = action_list.Where(item => item.GetGelStep().GetLiquidInfo() != null && item.GetGelStep().GetLiquidInfo().IsSlys).ToList();
            for (int i = 1; i < slys_tem_list.Count; i++)
            {
                slys_tem_list[i].GetGelStep().StepIndex = slys_tem_list[0].GetGelStep().StepIndex;
            }
            //交叉配血合并(按献血员排序)
            List <List<SampleInfo >> sort_list = new List<List<SampleInfo>>();
            for (int i = 0; i < samples_barcode.Count; i++)
            {
                var samplev_info = ResManager.getInstance().GetSampleInfo(samples_barcode[i]);
                var samplek_info = ResManager.getInstance().GetSampleInfo(volunteers_barcode[i]);
                if (samplev_info == null) samplev_info = new SampleInfo(samples_barcode[i], -1, 0);
                if (samplek_info == null) samplek_info = new SampleInfo(volunteers_barcode[i], -1, 0);
                sort_list.Add(new List<SampleInfo>());
                sort_list[sort_list.Count - 1].Add(samplek_info);
                sort_list[sort_list.Count - 1].Add(samplev_info);
            }
            sort_list = sort_list.OrderBy(p => p[0].Index).ToList();
            samples_barcode.Clear();
            volunteers_barcode.Clear();
            foreach (var item in sort_list)
            {
                volunteers_barcode.Add(item[0].Barcode);
                samples_barcode.Add(item[1].Barcode);
            }
        }
        //更新载架号
        public void UpdataSampleIndex()
        {
            samples_barcode_index.Clear();
            foreach (var item in samples_barcode)
            {
                var sampleinfo = ResManager.getInstance().GetSampleInfo(item);
                if (sampleinfo != null)
                    samples_barcode_index.Add(sampleinfo.Index);
                else
                    samples_barcode_index.Add(0);
            }
        }
        //当前人份
        public int GetCurRenFen()
        {
            return samples_barcode.Count();
        }
        public void AddSampleCode(string sample_code)
        {
            samples_barcode.Add(sample_code);
        }

        public string GetSampleCode(int index)
        {
            //System.Diagnostics.Debug.Assert(index < samples_barcode.Count);
            if (index<samples_barcode.Count)
            {
                return samples_barcode[index];
            }
            return "";
        }

        public string GetAddvolunteerCode(int index)
        {
            if(volunteers_barcode.Count != 0)
            {
                //System.Diagnostics.Debug.Assert(index < volunteers_barcode.Count);
                if (index < volunteers_barcode.Count)
                {
                    return volunteers_barcode[index];
                }
            }
            return "";
        }

        public void AddvolunteerCode(string barcode)
        {
            if (barcode != "") 
            volunteers_barcode.Add(barcode);
        }

        public TestStepEnum GetActionTypeAt(int index)
        {
            if (index >= action_list.Count) return TestStepEnum.Define;
            var gelstep = action_list[index].GetGelStep();
            System.Diagnostics.Debug.Assert(gelstep != null);
            if(gelstep==null) return TestStepEnum.Define;
            return gelstep.StepClass;
        }

        public LogicActionUnity GetActionAt(int index)
        {
            if (index >= action_list.Count) return null;
            return action_list[index];
        }

        public bool FindAct(TestStepEnum act_type)
        {
            foreach(var act_tem in action_list)
            {
                if (act_tem.GetGelStep().StepClass == act_type)
                    return true;
            }
            return false;
        }
        
        public string GetGelMask()
        {
            return ResManager.getInstance().GetGelMaskByID(gel_mask_id);
        }

        public bool RemoveAct(LogicActionUnity act)
        {
            var ret = action_list.Remove(act);
            return ret;
        }

        public SampleInfo GetSampleInfo(int index)
        {
            var code = samples_barcode.Where(item => item.IndexOf("used") == -1).ToList();
            if (index>code.Count) return null;
            return ResManager.getInstance().GetSampleInfo(code[index]);
        }

       
    }
}
