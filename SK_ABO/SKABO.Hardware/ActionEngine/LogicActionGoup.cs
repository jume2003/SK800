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
using SKABO.Common.Models.NotDuplex;

namespace SKABO.ActionEngine
{
    public enum LogicActionGoupEnum
    {
        /// <summary>
        /// 默认
        /// </summary>
        Define = 0,
        /// <summary>
        /// 试剂
        /// </summary>
        Agentia = 1,
        /// <summary>
        /// 生理盐水
        /// </summary>
        Slys = 2,
        /// <summary>
        /// 交叉配血红细胞
        /// </summary>
        CrossmatchingHxb = 3,
        /// <summary>
        /// 交叉配血血清
        /// </summary>
        CrossmatchingXueQing = 4,
        /// <summary>
        /// 交叉配血生理盐水
        /// </summary>
        CrossmatchingSlys = 5,

    }

    public class LogicActionUnity
    {
        public LogicActionGoup father { get; set; } = null;//上层组合
        public int State { get; set; } = 0;//0没执行1执行中2执行完
        public virtual List<LogicActionUnity> GetActions()
        {
            LogicActionUnity[] list_tem = { this };
            return list_tem.ToList();
        }
        public virtual List<LogicActionUnity> GetAllActions()
        {
            LogicActionUnity[] list_tem = { this };
            return list_tem.ToList();
        }
        public virtual int GetActionCount(){return 0;}
        public virtual LogicActionUnity GetLastAction(){return this;}
        public virtual void RemoveAction(){}
        public virtual T_GelStep GetGelStep(){return null;}
        public virtual void UpdataFather(){ } //父组合整理                  //
        public virtual void SetSkipPutTip(bool is_skip){}
        public virtual SampleInfo GetSampleInfo() { return null; }
        public virtual ExperimentPackage GetExperPackage() {return null; }
        public virtual string GetSampleCode() { return ""; }
        public virtual string GetAddvolunteerCode() { return ""; }
    }

    public class LogicAction: LogicActionUnity
    {
        int action_count = 1;
        private T_GelStep gel_step;
        public LogicAction(T_GelStep action)
        {
            gel_step = action;
        }
        public override T_GelStep GetGelStep()
        {
            return gel_step;
        }
        public override void RemoveAction()
        {
            if (father != null) father.RemoveAction(this);
            GetExperPackage().RemoveAct(this);
            action_count = 0;
        }
        public override ExperimentPackage GetExperPackage()
        {
            return (ExperimentPackage)gel_step.ExperPackage;
        }
        public override int GetActionCount()
        {
            return action_count;
        }
        public override LogicActionUnity GetLastAction()
        {
            return this;
        }
        public override void SetSkipPutTip(bool is_skip)
        {
            GetGelStep().is_skip_puttip = is_skip;
            GetGelStep().is_skip_spu_back = true;
        }
        public override SampleInfo GetSampleInfo()
        {
            var gelstep_tem = GetGelStep();
            var liquidtype = gelstep_tem.GetLiquidType();
            var experpackage = (ExperimentPackage)gel_step.ExperPackage;
            //病人（用sample）不是病人(用veri)
            if (liquidtype == TestStepEnum.FPBRXQ|| liquidtype== TestStepEnum.FPBRXSHXB || liquidtype == TestStepEnum.FPBRSLYS)
            {
                return ResManager.getInstance().GetSampleInfo(experpackage.GetSampleCode(gel_step.SampleIndex));
            }
            else if (liquidtype == TestStepEnum.FPXXYXQ || liquidtype == TestStepEnum.FPXXYXSHXB || liquidtype == TestStepEnum.FPXXYSLYS)
            {
                return ResManager.getInstance().GetSampleInfo(experpackage.GetAddvolunteerCode(gel_step.SampleIndex));
            }
            return null;
        }
        public override string GetSampleCode()
        {
            return GetExperPackage().GetSampleCode(GetGelStep().SampleIndex);
        }
        public override string GetAddvolunteerCode()
        {
            return GetExperPackage().GetAddvolunteerCode(GetGelStep().SampleIndex);
        }
    }

    public class LogicActionGoup: LogicActionUnity
    {
        public List<LogicActionUnity> action_group = new List<LogicActionUnity>();
        public int ExeActionCount { get; set; } = 1;//一次取多小个动作
        public int GetExeActionCount()
        {
          int retcount = ExeActionCount;
          int maxinj =  Engine.getInstance().injectorDevice.GetMaxInjCount();
          if (retcount > maxinj) retcount = maxinj;
          return retcount;
        }
        public LogicActionGoupEnum GroupType = LogicActionGoupEnum.Define;
        public LogicActionGoup(LogicActionUnity action)
        {
            AddAction(action);
        }
        public LogicActionGoup() { }
        public void AddAction(LogicActionUnity action)
        {
            action.father = this;
            action_group.Add(action);
        }
        public void AddAction(List<LogicActionUnity> actions)
        {
            foreach(var act in actions)
            {
                AddAction(act);
            }
        }
        public override List<LogicActionUnity> GetActions()
        {
            int count = ExeActionCount > action_group.Count ? action_group.Count : ExeActionCount;
            var list_tem = new List<LogicActionUnity>();
            for (int i=0;i< count;i++)
            {
                list_tem = list_tem.Concat(action_group[i].GetActions()).ToList();
            }
            if(father==null&&list_tem.Count!=0&&list_tem.Where(item=>item.GetGelStep().StepClass!=TestStepEnum.FULLUP).Count()==0)
            {
                RemoveAction(list_tem);
                return GetActions();
            }
            return list_tem;
        }
        public override List<LogicActionUnity> GetAllActions()
        {
            var list_tem = new List<LogicActionUnity>();
            for (int i = 0; i < action_group.Count; i++)
            {
                list_tem = list_tem.Concat(action_group[i].GetActions()).ToList();
            }
            return list_tem;
        }
        public void RemoveAction(LogicActionUnity logicaction)
        {
            action_group.Remove(logicaction);
            if (action_group.Count == 0)
            {
                if (father != null) father.RemoveAction(this);
            }
        }
        public override void RemoveAction()
        {
            var list_tem = GetActions();
            foreach (var item in list_tem)
            {
                item.RemoveAction();
            }
        }
        public void RemoveAction(List<LogicActionUnity> list_tem)
        {
            foreach (var item in list_tem)
            {
                item.RemoveAction();
            }
        }
        public override ExperimentPackage GetExperPackage()
        {
            if (action_group.Count == 0) return null;
            return action_group[0].GetExperPackage();
        }
        public override T_GelStep GetGelStep()
        {
            if (action_group.Count == 0) return null;
            return action_group[0].GetGelStep();
        }
        public void SortActionByPiercerIndex()
        {
            //按ExeActionCount分组
            var group_tem = new List<List<LogicActionUnity>>();
            for(int i=0;i< action_group.Count;i++)
            {
                if(i%ExeActionCount==0)group_tem.Add(new List<LogicActionUnity>());
                group_tem[group_tem.Count - 1].Add(action_group[i]);
            }
            //按破孔位排序
            action_group.Clear();
            foreach (var group in group_tem)
            {
                var groupp = group.OrderBy(c => c.GetExperPackage().piercer_index).ToList();
                foreach (var grouppp in groupp)
                {
                    action_group.Add(grouppp);
                }
            }
        }
        public override int GetActionCount()
        {
            return action_group.Count;
        }
        public override LogicActionUnity GetLastAction()
        {
            if (action_group.Count == 0) return null;
            return action_group[action_group.Count - 1];
        }
        public LogicAction GetAction(int index)
        {
            if (index>action_group.Count) return null;
            if (action_group[index] is LogicAction == false) return null;
            return (LogicAction)action_group[index];
        }
        public override SampleInfo GetSampleInfo()
        {
            var action = GetAction(0);
            if(action!=null)
            {
                return action.GetSampleInfo();
            }
            return null;
        }
        public override string GetSampleCode()
        {
            return GetExperPackage().GetSampleCode(GetGelStep().SampleIndex);
        }
        public override string GetAddvolunteerCode()
        {
            return GetExperPackage().GetAddvolunteerCode(GetGelStep().SampleIndex);
        }
        //
        public override void SetSkipPutTip(bool is_skip)
        {
            foreach(var action in action_group)
            {
                action.GetGelStep().is_skip_puttip = is_skip;
                action.GetGelStep().is_skip_spu_back = true;
            }
        }
        //单管动作处理
        public void UpdataOneTube()
        {
            if(ExeActionCount == 1)
            {
                if (GroupType == LogicActionGoupEnum.CrossmatchingHxb || GroupType == LogicActionGoupEnum.CrossmatchingXueQing)
                {
                    for (int i = 0; i < action_group.Count; i++)
                    {
                        action_group[i].GetGelStep().is_skip_puttip = i < (action_group.Count - 1);
                        action_group[i].GetGelStep().is_skip_spu_back = i < (action_group.Count - 1);
                        action_group[i].GetGelStep().HitSort = 1;
                        action_group[i].GetGelStep().skip_spu_times = 1;
                        if (i >= 1)
                        {
                            action_group[i].GetGelStep().is_skip_zjt = true;
                            action_group[i].GetGelStep().is_skip_abs = true;
                            action_group[i].GetGelStep().is_skip_mix = true;
                        }
                    }
                    action_group[0].GetGelStep().skip_spu_times = (GroupType == LogicActionGoupEnum.CrossmatchingXueQing) ? action_group.Where(item=>item.GetGelStep().StepClass!=TestStepEnum.FULLUP).Count() : 1;
                    action_group[0].GetGelStep().after_mix_spucapacity = (GroupType == LogicActionGoupEnum.CrossmatchingHxb) ? action_group.Where(item => item.GetGelStep().StepClass != TestStepEnum.FULLUP).Count() : 1;
                }
            }
        }
        //融合更新处理
        public void UpdataCombination()
        {
            int maxinj_count = Engine.getInstance().injectorDevice.GetMaxInjCount();
            if (action_group.Count > 1)
            {
                if (GroupType == LogicActionGoupEnum.Agentia)
                {
                    if (action_group.Count > 2)
                    {
                        int vol_count = action_group.Count - 2;
                        for (int i = 0; i < action_group.Count; i++)
                        {
                            action_group[i].GetGelStep().is_skip_puttip = i < (action_group.Count - ((action_group.Count % 2 == 0) ? 2 : action_group.Count % 2));
                            action_group[i].GetGelStep().is_skip_spu_back = true;
                            action_group[i].GetGelStep().skip_spu_times = 1;
                            if (i >= 2)
                            {
                                action_group[i].GetGelStep().is_skip_zjt = true;
                                action_group[i].GetGelStep().is_skip_abs = true;
                                action_group[i].GetGelStep().is_skip_mix = true;
                            }
                        }
                        for (int i = 0; i < vol_count; i++)
                        {
                            action_group[i % 2].GetGelStep().skip_spu_times++;
                        }
                    }
                }
                else if (GroupType == LogicActionGoupEnum.Slys)
                {
                    if (action_group.Count > maxinj_count)
                    {
                        for (int i = 0; i < action_group.Count; i++)
                        {
                            action_group[i].GetGelStep().is_skip_puttip = i < (action_group.Count - ((action_group.Count % maxinj_count == 0) ? maxinj_count : action_group.Count % maxinj_count));
                            action_group[i].GetGelStep().is_skip_spu_back = true;
                            action_group[i].GetGelStep().skip_spu_times = 1;
                            if (i >= maxinj_count)
                            {
                                action_group[i].GetGelStep().is_skip_zjt = true;
                            }
                        }
                    }
                }
                else if (GroupType == LogicActionGoupEnum.CrossmatchingSlys)
                {
                    if (action_group.Count > maxinj_count)
                    {
                        for (int i = 0; i < action_group.Count; i++)
                        {
                            action_group[i].SetSkipPutTip(i < (action_group.Count - ((action_group.Count % maxinj_count == 0) ? maxinj_count : action_group.Count % maxinj_count)));
                            if (i >= maxinj_count)
                            {
                                action_group[i].GetGelStep().is_skip_zjt = true;
                            }
                        }
                    }
                }
            }

        }
        //是否融合
        public bool IsCombination(LogicActionUnity action)
        {
            var last_action = GetLastAction();
            int maxinj_count = Engine.getInstance().injectorDevice.GetMaxInjCount();
            //int group_max_count = action.GetExperPackage().is_crossmatching&& GroupType == LogicActionGoupEnum.CrossmatchingSlys? 64 : 12;
            int group_max_count = action.GetExperPackage().is_crossmatching? 64 : 12;
            if (action_group.Count < group_max_count && last_action != null && last_action.GetGelStep().StepClass == TestStepEnum.FPYT && action.GetGelStep().StepClass == TestStepEnum.FPYT)
            {
                var exp = action.GetExperPackage();
                var liquidinfo = action.GetGelStep().GetLiquidInfo();
                var fpytinfo = action.GetGelStep().GetFpytInfo();
                var samplecode = exp.GetSampleCode(action.GetGelStep().SampleIndex);

                var last_exp = last_action.GetExperPackage();
                var last_liquidinfo = last_action.GetGelStep().GetLiquidInfo();
                var last_fpytinfo = last_action.GetGelStep().GetFpytInfo();
                var last_samplecode = last_exp.GetSampleCode(last_action.GetGelStep().SampleIndex);
                string liquidtype = liquidinfo.LiquidType;
                string last_liquidtype = last_liquidinfo.LiquidType;
                if(exp.is_crossmatching && last_exp.is_crossmatching)
                {
                    liquidtype = liquidtype.Replace("病人", "");
                    liquidtype = liquidtype.Replace("献血员", "");
                    last_liquidtype = last_liquidtype.Replace("病人", "");
                    last_liquidtype = last_liquidtype.Replace("献血员", "");
                }
                if (liquidtype == last_liquidtype && exp.gel_test_id == last_exp.gel_test_id)
                {
                    //试剂
                    if (liquidinfo.IsAgentia && last_liquidinfo.IsAgentia)
                    {
                        ExeActionCount = 2;
                        GroupType = LogicActionGoupEnum.Agentia;
                        return true;
                    }
                    //交叉配血
                    else if (exp.is_crossmatching && last_exp.is_crossmatching && liquidinfo.IsSlys == last_liquidinfo.IsSlys && liquidinfo.IsAgentia == last_liquidinfo.IsAgentia)
                    {
                        ExeActionCount = maxinj_count;
                        bool is_bingren = fpytinfo.FindLiquidType("病人*");
                        bool is_xueqing = liquidinfo.LiquidType.IndexOf("清") != -1;
                        bool is_hongxibao = liquidinfo.LiquidType.IndexOf("红") != -1;
                        if (liquidinfo.IsSlys) GroupType = LogicActionGoupEnum.CrossmatchingSlys;
                        else if (is_xueqing) GroupType = LogicActionGoupEnum.CrossmatchingXueQing;
                        else if (is_hongxibao) GroupType = LogicActionGoupEnum.CrossmatchingHxb;
                        return true;
                    }
                    //生理盐水
                    else if (liquidinfo.IsSlys && last_liquidinfo.IsSlys)
                    {
                        ExeActionCount = maxinj_count;
                        GroupType = LogicActionGoupEnum.Slys;
                        return true;
                    }
                    //默认
                    else if (samplecode != last_samplecode)
                    {
                        ExeActionCount = maxinj_count;
                        GroupType = LogicActionGoupEnum.Define;
                        return true;
                    }
                }

            }
            return false;
        }
        //组合整理
        public void UpdataGroup()
        {
            LogicActionUnity max_group = null;
            int max_count = 0;
            for (int i=0;i< action_group.Count;i++)
            {
                if(i%ExeActionCount==0)
                {
                    int range_count = ExeActionCount < (action_group.Count - i) ? ExeActionCount : (action_group.Count - i);
                    var group_list = action_group.GetRange(i, range_count).OrderByDescending(item => item.GetActionCount()).ToList();
                    max_group = group_list[0];
                    int group_count = max_group.GetActionCount();
                    foreach (var group in group_list)
                    {
                        //把最多的那个多加一个动作用来先完成多动作那个针头
                        if (group is LogicActionGoup&& group.GetActionCount()== group_count)
                        {
                            var max_last_act = group.GetLastAction();
                            var action_tem = max_last_act.GetGelStep().clone();
                            action_tem.StepClass = TestStepEnum.FULLUP;
                            action_tem.is_skip_zjt = true;//是否跳过装针
                            action_tem.is_skip_abs = true;//是否跳过加样
                            action_tem.is_skip_spu = true;//是否跳过分配
                            action_tem.is_skip_puttip = true;//是否跳过脱针
                            action_tem.is_skip_mix = true;//是否跳过混合
                            action_tem.is_skip_spu_back = true;//是否跳过分配回零
                            var logaction = new LogicAction(action_tem);
                            ((LogicActionGoup)group).AddAction(logaction);
                            ((LogicActionGoup)group).UpdataOneTube();
                            max_last_act.GetGelStep().is_skip_spu_back = false;
                        }
                    }
                   
                   

                    //int range_count = ExeActionCount<(action_group.Count-i)? ExeActionCount: (action_group.Count-i);
                    //max_group = action_group.GetRange(i, range_count).OrderByDescending(item => item.GetActionCount()).ToList()[0];
                    ////把最多的那个多加一个动作用来先完成多动作那个针头
                    //if (max_group is LogicActionGoup)
                    //{
                    //    var max_last_act = max_group.GetLastAction();
                    //    var action_tem = max_last_act.GetGelStep().clone();
                    //    action_tem.StepClass = TestStepEnum.FULLUP;
                    //    action_tem.is_skip_zjt = true;//是否跳过装针
                    //    action_tem.is_skip_abs = true;//是否跳过加样
                    //    action_tem.is_skip_spu = true;//是否跳过分配
                    //    action_tem.is_skip_puttip = true;//是否跳过脱针
                    //    action_tem.is_skip_mix = true;//是否跳过混合
                    //    action_tem.is_skip_spu_back = true;//是否跳过分配回零
                    //    var logaction = new LogicAction(action_tem);
                    //    ((LogicActionGoup)max_group).AddAction(logaction);
                    //    ((LogicActionGoup)max_group).UpdataOneTube();
                    //}
                }

                max_count = max_group.GetActionCount();

                int count = max_count - action_group[i].GetActionCount();
                var last_act = action_group[i].GetLastAction();
                if (count != 0&& action_group[i] is LogicAction)
                {
                    var group_tem = action_group[i];
                    action_group[i] = new LogicActionGoup(group_tem);
                    action_group[i].father = group_tem.father;
                }
                if (max_group is LogicActionGoup)
                {
                    for (int j = 0; j < count; j++)
                    {
                        var action_tem = last_act.GetGelStep().clone();
                        action_tem.StepClass = TestStepEnum.FULLUP;
                        action_tem.is_skip_zjt = true;//是否跳过装针
                        action_tem.is_skip_abs = true;//是否跳过加样
                        action_tem.is_skip_spu = true;//是否跳过分配
                        action_tem.is_skip_puttip = true;//是否跳过脱针
                        action_tem.is_skip_mix = true;//是否跳过混合
                        action_tem.is_skip_spu_back = true;//是否跳过分配回零
                        var logaction = new LogicAction(action_tem);
                        ((LogicActionGoup)action_group[i]).AddAction(logaction);
                    }
                    if(count!=0)
                    {
                        for (int j = 0; j < action_group[i].GetActionCount(); j++)
                        {
                            var action_tem = ((LogicActionGoup)action_group[i]).action_group[j].GetGelStep();
                            action_tem.is_skip_spu = j!= (action_group[i].GetActionCount() - 1);//分配
                            action_tem.is_skip_puttip = j != (action_group[i].GetActionCount() - 1);//脱针
                            action_tem.is_skip_spu_back = j != (action_group[i].GetActionCount() - 1);//脱针
                            if (j == action_group[i].GetActionCount() - 1)
                            {
                                action_tem.StepClass = action_group[i].GetGelStep().StepClass;
                            }
                        }
                    }
                }
            }
        }
       
        //单卡优化
        public void OptimizeAction()
        {
            //单管加血清
            if (GetExperPackage().is_crossmatching)
            {
                var sample_codes = new List<string>();
                foreach (var act in action_group)
                {
                    string code_tem = act.GetSampleCode();
                    sample_codes.Add(code_tem);
                }
                sample_codes = sample_codes.Where((x, i) => sample_codes.FindIndex(z => z == x) == i).ToList();
                foreach (var code in sample_codes)
                {
                    var xueqing_list = action_group.Where(item => item.GetExperPackage().GetSampleCode(item.GetGelStep().SampleIndex) == code && item.GetGelStep().GetLiquidType() == TestStepEnum.FPBRXQ).ToList();
                    if (xueqing_list.Count > 1)
                        MakeGroup(xueqing_list, LogicActionGoupEnum.CrossmatchingXueQing);
                    var hxb_list = action_group.Where(item => item.GetExperPackage().GetSampleCode(item.GetGelStep().SampleIndex) == code && item.GetGelStep().GetLiquidType() == TestStepEnum.FPBRXSHXB).ToList();
                    if (hxb_list.Count > 1)
                        MakeGroup(hxb_list, LogicActionGoupEnum.CrossmatchingHxb);
                    //把病人生理盐水删除多余
                    var slys_list = action_group.Where(item => item.GetExperPackage().GetSampleCode(item.GetGelStep().SampleIndex) == code && item.GetGelStep().GetLiquidType() == TestStepEnum.FPBRSLYS).ToList();
                    if (slys_list.Count > 1)
                    {
                        for (int j = 1; j < slys_list.Count; j++)
                        {
                            slys_list[0].GetGelStep().MixCode += slys_list[j].GetGelStep().MixCode;
                            action_group.Remove(slys_list[j]);
                            slys_list[j].GetExperPackage().RemoveAct(slys_list[j]);
                        }
                    }
                }
            }
        }
        //生成组合
        public void MakeGroup(List<LogicActionUnity> act_list, LogicActionGoupEnum group_type)
        {
            LogicActionGoup group = null;
            if (act_list[0] is LogicActionGoup)
            {
                group = (LogicActionGoup)act_list[0];
                group.GroupType = group_type;
                for (int i = 1; i < act_list.Count; i++)
                {
                    group.AddAction(act_list[i]);
                    action_group.Remove(act_list[i]);
                }
            }
            else
            {
                int index = action_group.FindIndex(item => item == act_list[0]);
                group = new LogicActionGoup();
                group.GroupType = group_type;
                action_group.Insert(index, group);
                for (int i = 0; i < act_list.Count; i++)
                {
                    group.AddAction(act_list[i]);
                    action_group.Remove(act_list[i]);
                }
            }
            group.UpdataOneTube();
        }
        //交叉配血样本排序
        public void SortSampleCrossMatching()
        {
           if(GroupType== LogicActionGoupEnum.CrossmatchingHxb|| GroupType == LogicActionGoupEnum.CrossmatchingXueQing|| GroupType == LogicActionGoupEnum.CrossmatchingSlys)
           {
                action_group =  action_group.OrderBy(item => item.GetSampleInfo()!=null?item.GetSampleInfo().Index:0).ToList();
           }
           //foreach(var group in action_group)
           // {
           //     if(group.GetGelStep().GetLiquidType()==TestStepEnum.FPBRXQ)
           //         Console.WriteLine(group.GetSampleCode());
           //     else
           //         Console.WriteLine(group.GetAddvolunteerCode());
           // }
        }
        //父组合整理
        public override void UpdataFather()
        {
            foreach (var item in action_group)
            {
                item.father = this;
                item.UpdataFather();
            }
        }
    }
}
