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

namespace SKABO.ActionEngine
{
    public class ExperimentLogic
    {
        private Object mylock = new Object();
        public double lasttime = 0;
        public double inj_wait_time = 0;
        public double hand_wait_time = 0;
        public MachineHandDevice handDevice = null;
        public PiercerDevice piercerDevice = null;
        public GelWarehouseDevice gelwareDevice = null;
        public InjectorDevice injectorDevice = null;
        public CentrifugeMrg cenMrg = null;
        public CameraDevice cameraDevice = null;
        public OtherPartDevice opDevice = null;
        public ResManager resmanager = ResManager.getInstance();
        public ActionManager actionmanager = ActionManager.getInstance();
        public ActionGenerater generater = ActionGenerater.getInstance();
        public List<ExperimentPackage> experiment_package_list = new List<ExperimentPackage>();
        public List<ExperimentPackage> experiment_package_del_list = new List<ExperimentPackage>();
        public List<T_GelStep> inj_action_list = new List<T_GelStep>();
        public static ExperimentLogic experimentlogic = null;
        public static ExperimentLogic getInstance()
        {
            if (experimentlogic == null) experimentlogic = new ExperimentLogic();
            return experimentlogic;
        }
        public void DelPackage(ExperimentPackage experiment_package)
        {
            lock (mylock)
            {
                if(experiment_package!=null)
                experiment_package_del_list.Add(experiment_package);
            }
        }

        public void DelAllPackage()
        {
            lock (mylock)
            {
                foreach(var item in experiment_package_list)
                {
                    experiment_package_del_list.Add(item);
                }
            }
        }

        public void AddPackage(ExperimentPackage experiment_package)
        {
            lock (mylock)
            {
                experiment_package_list.Add(experiment_package);
            }
        }
        public ExperimentPackage GetCombinationExpPackage(ExperimentPackage expack)
        {
            foreach (var exp in experiment_package_list)
            {
                if (exp!= expack && exp.GetGelMask().IndexOf(expack.GetGelMask()) != -1)
                {
                    //没有开孔,已开孔加样还没结束,等待使用
                    var pier_seat = resmanager.GetResByCode(exp.GetGelMask(), "T_BJ_GelSeat","1");
                    if(pier_seat==null) pier_seat = resmanager.GetResByCode(exp.GetGelMask(), "T_BJ_GelSeat", "3");
                    if (exp.is_used_gel&& exp.GetCurRenFen()+ 1<=exp.ren_fen&&exp.action_list.Count != 0&& exp.after_kktime > 0&&
                        (!exp.is_open|| exp.is_open&&pier_seat!=null&&exp.FindAct(TestStepEnum.JYJS)|| exp.action_list[0].StepClass==TestStepEnum.ECONOMIZECOUNTTIME))
                    {
                        return exp;
                    }
                }
            }
            return null;
        }
        //得到加样中的试验
        public List<ExperimentPackage> GetInjExpPackageList()
        {
            List<ExperimentPackage> exp_list = new List<ExperimentPackage>();
            foreach (var exp in experiment_package_list)
            {
                var jyjsact_list = exp.action_list.Where(item => item.StepClass == TestStepEnum.JYJS).ToList();
                var jyjs_index = jyjsact_list.Count != 0 ? jyjsact_list[0].StepIndex : 0;
                var nomoact_list = exp.action_list.Where(item => item.StepIndex < jyjs_index).ToList();
                if (exp.is_open && nomoact_list.Count!=0)
                {
                    exp_list.Add(exp);
                }
            }
            return exp_list;
        }
        //
        public ExperimentPackage GetExpPackageByMask(string mask)
        {
            foreach (var exp in experiment_package_list)
            {
                if(exp.GetGelMask() == mask)
                {
                    return exp;
                }
            }
            return null;
        }
        public void ClsAction()
        {
            //清理完成动作
            foreach (var package in experiment_package_list)
            {
                for (int i = package.action_list.Count - 1; i >= 0; i--)
                {
                    if (package.action_list[i].State == 2|| package.action_list[i].is_skip)
                    {
                        package.action_list.Remove(package.action_list[i]);
                    }
                       
                }
            }
            for (int i = inj_action_list.Count - 1; i >= 0; i--)
            {
                if (inj_action_list[i].State == 2 || inj_action_list[i].is_skip)
                {
                    inj_action_list.Remove(inj_action_list[i]);
                }
            }
            //清理测试包
            for (int i = experiment_package_list.Count - 1; i >= 0; i--)
            {
                if (experiment_package_list[i].action_list.Count == 0)
                    experiment_package_list.Remove(experiment_package_list[i]);
            }
            //清离用户删除
            foreach (var exppack in experiment_package_del_list)
            {
                experiment_package_list.Remove(exppack);
            }
            experiment_package_del_list.Clear();
        }
        //排序
        public void UpDataAction()
        {
            lock (mylock)
            {
                //融合两个试验
                foreach (var package in experiment_package_list)
                {
                    if (package.is_used_gel&& package.action_list.Count != 0 && package.ren_fen != package.GetCurRenFen())
                    {
                        var exp_pack_tem = GetCombinationExpPackage(package);
                        if (exp_pack_tem != null)
                        {
                            if(exp_pack_tem.samples_barcode.Count==1)
                            package.Combination(exp_pack_tem);
                            else
                            exp_pack_tem.Combination(package);
                        }
                    }
                }
                ClsAction();
                //把交叉配血卡的动作ID更新
                //foreach (var package in experiment_package_list)
                //{
                //    if (package.is_crossmatching) package.UpDataCrossMatchingSetpIndex();
                //}
                inj_action_list = new List<T_GelStep>();
                List<List<T_GelStep>> action_group = new List<List<T_GelStep>>();
                int index = 0;
                int max_goup_size = 12;
                //按有孵育的就组
                experiment_package_list = experiment_package_list.OrderByDescending(a=> a.sort_index).ToList();
                //12个一组
                int group_last_gel_test_id = -1;
                for (int i = 0; i < experiment_package_list.Count; i++)
                {
                    //不同卡为一组
                    if (experiment_package_list[i].is_jyjs == false)
                    {
                        if (index % max_goup_size == 0||(group_last_gel_test_id!= experiment_package_list[i].gel_test_id))
                        {
                            action_group.Add(new List<T_GelStep>());
                            index = 1;
                        }
                        else index++;
                        action_group[action_group.Count - 1] = action_group[action_group.Count - 1].Concat(experiment_package_list[i].action_list).ToList();
                        group_last_gel_test_id = experiment_package_list[i].gel_test_id;
                    }
                }
              
                for (int i = 0; i < action_group.Count; i++)
                {
                    action_group[i] = action_group[i].OrderBy(u => u.StepIndex).ToList();
                    inj_action_list = inj_action_list.Concat(action_group[i]).ToList();
                }
                //试剂(最多两个针头加样，分配，把脱针去掉，把加样去掉,把容量合并)
                //查找连续试剂(如果超过三个就合并)
                AgentiaCombination(inj_action_list,0);
                SlysCombination(inj_action_list, 0);
                //交叉配血合并
                CrossMatchingCombination(inj_action_list, 0);
                //破空器返回
                int pier_y = 0;
                var pier_device = new ActionDevice(piercerDevice);
                pier_device.GetRealY(ref pier_y);
                if(pier_y != 0&& inj_action_list.Count!=0&& inj_action_list[0].StepClass != TestStepEnum.KaiKongGel)
                {
                    var pier_back = MoveTo.create(piercerDevice, 3000, -1, 0, -1);
                    pier_back.runAction();
                }
            }
        }
        //加样试剂融合
        public void AgentiaCombination(List<T_GelStep> action_list,int index)
        {
            List<T_GelStep> agentia_list = new List<T_GelStep>();
            string liquidtype = "";
            int gel_test_id = -1;
            while(index < inj_action_list.Count)
            {
                var act = inj_action_list[index];
                var exp_tem = (ExperimentPackage)act.ExperPackage;
                var liquidinfo = act.GetLiquidInfo();
                if (liquidinfo!=null&&liquidinfo.IsAgentia&& (liquidinfo.LiquidType== liquidtype|| liquidtype=="")&&(exp_tem.gel_test_id==gel_test_id||gel_test_id==-1))
                {
                    liquidtype = act.GetLiquidInfo().LiquidType;
                    gel_test_id = exp_tem.gel_test_id;
                    agentia_list.Add(act);
                    if (agentia_list.Count == 12) break;
                }
                else if(agentia_list.Count!=0)
                {
                    break;
                }
                index++;
            }
            if(agentia_list.Count>2)
            {
                int vol_count = agentia_list.Count-2;
                for (int i = 0; i < agentia_list.Count; i++)
                {
                    agentia_list[i].is_skip_puttip = i< (agentia_list.Count-2);
                    agentia_list[i].is_skip_spu_back = true;
                    agentia_list[i].skip_spu_times = 1;
                    if (i >= 2)
                    {
                        
                        agentia_list[i].is_skip_zjt = true;
                        agentia_list[i].is_skip_abs = true;
                        agentia_list[i].is_skip_mix = true;
                    }
                }
                for (int i = 0; i < vol_count; i++)
                {
                    agentia_list[i%2].skip_spu_times++;
                }
            }
            if(index< inj_action_list.Count)
            AgentiaCombination(action_list, index);
        }
        //生理盐水剂融合
        public void SlysCombination(List<T_GelStep> action_list, int index)
        {
            List<T_GelStep> agentia_list = new List<T_GelStep>();
            string liquidtype = "";
            while (index < inj_action_list.Count)
            {
                var act = inj_action_list[index];
                var liquidinfo = act.GetLiquidInfo();
                if (liquidinfo != null && liquidinfo.IsSlys && (liquidinfo.LiquidType == liquidtype || liquidtype == ""))
                {
                    liquidtype = act.GetLiquidInfo().LiquidType;
                    agentia_list.Add(act);
                    if (agentia_list.Count == 12) break;
                }
                else if (agentia_list.Count != 0)
                {
                    break;
                }
                index++;
            }
            if (agentia_list.Count > 4)
            {
                for (int i = 0; i < agentia_list.Count; i++)
                {
                    agentia_list[i].is_skip_puttip = i < (agentia_list.Count - 4);
                    agentia_list[i].is_skip_spu_back = true;
                    agentia_list[i].skip_spu_times = 1;
                    if (i >= 4)
                    {
                        agentia_list[i].is_skip_zjt = true;
                    }
                }
            }
            if (index < inj_action_list.Count)
                SlysCombination(action_list, index);
        }
        //交叉配血病人红细胞与病人血清融合
        public void CrossMatchingCombination(List<T_GelStep> action_list, int index)
        {
            
        }
        //是否在离心机内的所有卡当前动作是离心
        public bool IsCanPutCen(CentrifugeMDevice cendev)
        {
            if (actionmanager.getAllActionsCount(cendev) != 0) return false;
            foreach (var seat in resmanager.centrifuge_list)
            {
                if (seat.Code == cendev.Centrifugem.Code.SetValue)
                {
                    for (int i = 0; i < seat.Values.Length; i++)
                    {
                        ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                        if (resinfo != null&& resinfo.PutOk)
                        {
                            var exp_package = GetExpPackageByMask(resinfo.GetGelMask());
                            if (exp_package.GetActionTypeAt(0) != TestStepEnum.ZKDLXJ)
                                return false;
                        }
                    }
                    break;
                }
            }
            return true;
        }
        //离心开始逻辑
        public bool CenRunLogic(CentrifugeMDevice cendev)
        {

            bool is_allcenact = true;//是否全是离心动作
            bool is_allputok = true;//是否全是已放好
            bool is_empty = true;//是否为空
            bool is_gelfull = false;//是否已满
            bool is_gelsoon = false;//是否有卡快要离心
            int gel_count = 0;
            List<ResInfoData> resinfo_list = new List<ResInfoData>();
            if (experiment_package_list.Count == 0) return false;
            foreach (var seat in resmanager.centrifuge_list)
            {
                if (seat.Code == cendev.Centrifugem.Code.SetValue)
                {
                    for (int i = 0; i < seat.Values.Length; i++)
                    {
                        ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                        if (resinfo != null &&resinfo.PutOk)
                        {
                            is_empty = false;
                            var exp_pack = GetExpPackageByMask(resinfo.GetGelMask());
                            if(exp_pack==null) exp_pack = GetExpPackageByMask(resinfo.GetCodeAt(0));
                            System.Diagnostics.Debug.Assert(exp_pack!=null);
                            if (exp_pack == null) return false;
                            is_allcenact = is_allcenact && exp_pack.GetActionTypeAt(0) ==TestStepEnum.ZKDLXJ;
                            is_allputok = is_allputok && resinfo.PutOk;
                            resinfo_list.Add(resinfo);
                            gel_count++;
                        }
                    }
                    is_gelfull = gel_count >= seat.Values.Length;
                }
            }
            if ((is_gelfull == false && actionmanager.getAllActionsCount(handDevice) != 0) || actionmanager.getAllActionsCount(cendev) != 0) return false;
            //普通卡位是否有卡快要离心
            foreach (var seat in resmanager.gelseat_list)
            {
                for (int i = 0; i < seat.Values.Length; i++)
                {
                    ResInfoData resinfo = (ResInfoData)seat.Values[i, 0];
                    if (resinfo != null && resinfo.PutOk&& resinfo.FindCode("pei*",false)==false)
                    {
                        var exp_pack = GetExpPackageByMask(resinfo.GetGelMask());
                        if (exp_pack == null) continue;
                        is_gelsoon = (exp_pack.GetActionTypeAt(0) == TestStepEnum.ZKDLXJ) ||
                        (exp_pack.GetActionTypeAt(0) == TestStepEnum.ZKDFY &&
                        exp_pack.GetActionTypeAt(1) == TestStepEnum.ZKDLXJ &&
                        exp_pack.hatch_time - exp_pack.hatch_cur_time <= 20000);
                        if (seat.Purpose == 4 && exp_pack.FindAct(TestStepEnum.ZKDLXJ) && exp_pack.FindAct(TestStepEnum.ZKDFY) == false)
                            is_gelsoon = true;
                        if (is_gelsoon) break;
                    }
                }
                if (is_gelsoon) break;
            }
            //检测是否满足运行条件
            if (is_allcenact && is_allputok && !is_empty && (!is_gelsoon || is_gelfull))
            {
                int hspeed = (int)cendev.Centrifugem.HightSpeed.SetValue;
                int lspeed = (int)cendev.Centrifugem.LowSpeed.SetValue;
                int htime = (int)cendev.Centrifugem.HightSpeedTime.SetValue;
                int ltime = (int)cendev.Centrifugem.LowSpeedTime.SetValue;
                int uphtime = (int)cendev.Centrifugem.AddHSpeedTime.SetValue;
                int upltime = (int)cendev.Centrifugem.AddLSpeedTime.SetValue;
                int stime = (int)cendev.Centrifugem.StopSpeedTime.SetValue;
                //配平卡动作(如果离心机内卡是单数就再放一张配平卡)
                var seque_pei = Sequence.create();
                var seque = Sequence.create();
                if (resinfo_list.Count() % 2 != 0)
                {
                    var spaw = Spawn.create();
                    var put_seque = Sequence.create();
                    //得到配平卡
                    var pei_gel = resmanager.GetResByCode("pei" + cendev.Centrifugem.Code.SetValue, "T_BJ_GelSeat");
                    var put_gel = generater.GenerateTakeGelFromNormal(pei_gel, ref put_seque);
                    T_GelStep[] pei_step = { new T_GelStep(), new T_GelStep() };
                    pei_step[0].StepClass = TestStepEnum.ZKDLXJ;
                    pei_step[1].StepClass = TestStepEnum.PutPeiGelBack;
                    var exp_pack = ExperimentPackage.Create(pei_step.ToList(), pei_gel.GetCodeAt(0), "123",8, 0,1,0,false,0,0,false);
                    exp_pack.is_jyjs = true;
                    experiment_package_list.Add(exp_pack);
                    resinfo_list.Add(put_gel);
                    var put_seat = resmanager.GetResByCode("null", "T_BJ_Centrifuge", cendev.Centrifugem.Code.SetValue);
                    spaw.AddAction(put_seque);
                    spaw.AddAction(MoveTo.create(cendev, 30001, -1, -1, put_seat.CenGelP[put_seat.CountX]));
                    seque_pei.AddAction(spaw);
                    generater.GeneratePutGelToCent(cendev.Centrifugem.Code.SetValue, put_seat, put_gel, ref seque_pei);
                }
                seque_pei.AddAction(HandOpenCloseDoor.create(handDevice, 5000, cendev.Centrifugem.Code.SetValue, false));
                seque.AddAction(SkWaitForAction.create(handDevice, seque_pei));
                seque.AddAction(CentrifugeStart.create(cendev, 6000000, hspeed, lspeed, htime, ltime, uphtime, upltime, stime));
                seque.AddAction(SkCallBackFun.create((ActionBase acttem) =>
                {
                    foreach (var resinfo in resinfo_list)
                    {
                        var exp_pack = GetExpPackageByMask(resinfo.GetGelMask());
                        if (exp_pack == null) exp_pack = GetExpPackageByMask(resinfo.GetCodeAt(0));
                        System.Diagnostics.Debug.Assert(exp_pack!=null);
                        var act = exp_pack.GetActionAt(0);
                        act.State = 2;
                    }
                    return true;
                }));
                seque_pei.runAction(handDevice);
                seque.runAction(cendev);
                return true;
            }
            return false;
        }
        public void ActLogic(List<T_GelStep> act_group, List<T_GelStep> act_group_next,double dt)
        {
            if (act_group!=null&&act_group.Count != 0 && act_group[0].State == 0)
            {
                int next_state = 2;
                var seque_act = Sequence.create();
                var exper_package = ((ExperimentPackage)act_group[0].ExperPackage);
                seque_act.exp_pack = exper_package;
                AbstractCanDevice device = null;
                foreach (var act in act_group)
                {
                    resmanager.tip_count += act.InjectCount;
                }
                if (act_group[0].InjectCount != 0)
                {
                    var act_next = act_group_next == null ? null : act_group_next[0];
                    device = injectorDevice;
                    act_group.Sort((a, b) => { return ((ExperimentPackage)a.ExperPackage).piercer_index > ((ExperimentPackage)b.ExperPackage).piercer_index ? 1 : -1; });
                    seque_act = generater.GenerateAction(act_group);
                    if(act_next!=null&& act_next.InjectCount==0)
                    {
                        seque_act.AddAction(SkCallBackFun.create((ActionBase act_tem) =>
                        {
                            inj_wait_time = 9999;
                            return true;
                        }));
                    }
                }
                else
                {
                    var act = act_group[0];
                    var act_next = act_group_next == null ? null : act_group_next[0];
                    var resinfo = resmanager.GetResByCode(exper_package.GetGelMask(), "T_BJ_GelSeat");
                    if (resinfo == null) resinfo = resmanager.GetResByCode(exper_package.GetGelMask(), "T_BJ_Centrifuge");
                    if ((resinfo == null || !resinfo.PutOk)&& act.StepClass!=TestStepEnum.LoadGel) return;
                    switch (act.StepClass)
                    {
                        case TestStepEnum.LoadGel:
                            {
                                if (actionmanager.getAllActionsCount(handDevice) == 0)
                                {
                                    var take_seat = resmanager.GetResByCode(exper_package.GetGelMask(), "T_BJ_GelSeat", "", "3");
                                    var paper_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "4",null, exper_package.is_crossmatching);
                                    if (paper_seat != null)
                                    {
                                        device = handDevice;
                                        if (take_seat != null)
                                            generater.GenerateTakeGelFromNormal(take_seat, ref seque_act);
                                        else
                                            seque_act.AddAction(HandTakeGelFromWare.create(handDevice, 3001, gelwareDevice, exper_package.gel_mask_id, exper_package.GetGelMask(), exper_package.GetSampleCode(act.SampleIndex)));
                                        seque_act.AddAction(HandPutGelToNormal.create(handDevice, 3001, gelwareDevice, paper_seat));
                                        if (act_next != null && act_next.StepClass != TestStepEnum.LoadGel)
                                        {
                                            seque_act.AddAction(SkCallBackFun.create((ActionBase act_tem) => {
                                                hand_wait_time = 999999;
                                                var ware_act = MoveTo.create(gelwareDevice, 3000, 0);
                                                ware_act.runAction(gelwareDevice);
                                                return true;
                                            }));
                                        }
                                        exper_package.piercer_index = paper_seat.CountX;
                                        paper_seat.Values[paper_seat.CountX, 0] = new ResInfoData();
                                        seque_act.destroyfun = (ActionBase act_tem) =>
                                        {
                                            paper_seat.Values[paper_seat.CountX, 0] = null;
                                            return true;
                                        };
                                        next_state = 2;
                                    }
                                    else
                                    {
                                        next_state = 0;
                                    }
                                }
                                else
                                {
                                    next_state = 0;
                                }
                            }
                            break;
                        case TestStepEnum.KaiKongGel:
                            {
                                int handrx = 0;
                                int injrx = 0;
                                var device_hand = new ActionDevice(handDevice);
                                var device_inj = new ActionDevice(injectorDevice);
                                next_state = 0;
                                if (resinfo.Purpose!="4")
                                {
                                    var put_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "4");
                                    if (put_seat == null && resinfo.Purpose != "3") put_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "3");
                                    if (put_seat != null)
                                    {
                                        device = handDevice;
                                        ResInfoData put_gel = null;
                                        if (resinfo.Purpose == "lxj")
                                        {
                                            put_gel = generater.GenerateTakeGelFromCent(resinfo, resinfo.CenCode, ref seque_act);
                                        }
                                        else 
                                            put_gel = generater.GenerateTakeGelFromNormal(resinfo, ref seque_act);
                                        generater.GeneratePutGelToNormal(put_seat, put_gel, ref seque_act);
                                        next_state = 0;
                                    }
                                }
                                else
                                {
                                    bool is_next_kaikong = act_next != null ? act_next.StepClass == TestStepEnum.KaiKongGel : false;
                                    var paper_seat = resmanager.GetResByCode(exper_package.GetGelMask(), "T_BJ_GelSeat", "", "4");
                                    int yb = is_next_kaikong ? -1 : 0;
                                    if (paper_seat != null &&
                                        device_hand.GetRealX(ref handrx) &&
                                        (handrx + 1000) < paper_seat.X && ActionDevice.hand_tx < paper_seat.X &&
                                        device_inj.GetRealX(ref injrx) &&
                                        (injrx + 1000) < paper_seat.InjectorX && ActionDevice.inj_tx < paper_seat.InjectorX+1000)
                                    {
                                        device = piercerDevice;
                                        seque_act.AddAction(PaperCard.create(piercerDevice, 3000, paper_seat, yb));
                                        seque_act.AddAction(SkCallBackFun.create((ActionBase acttem) =>
                                        {
                                            exper_package.is_open = true;
                                            resmanager.gel_count++;
                                            return true;
                                        }));
                                        next_state = 2;
                                    }
                                }
                            }
                            break;
                        case TestStepEnum.JYJS:
                            {
                                exper_package.is_jyjs = true;
                                next_state = 2;
                            }
                            break;
                        case TestStepEnum.ZKDFY:
                            {
                                next_state = 0;
                                if (resinfo.Purpose == "1")
                                {
                                    next_state = 0;
                                    exper_package.hatch_cur_time += dt;
                                    if (exper_package.hatch_cur_time >= exper_package.hatch_time)
                                    {
                                        next_state = 2;
                                    }
                                }
                                else if (!(resinfo.Purpose == "4" && actionmanager.getAllActionsCount(piercerDevice) != 0))
                                {
                                    var put_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "1");
                                    if (put_seat == null && resinfo.Purpose != "3") put_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "3");
                                    if (put_seat != null)
                                    {
                                        device = handDevice;
                                        ResInfoData put_gel = null;
                                        if (resinfo.Purpose == "lxj")
                                        {
                                            put_gel = generater.GenerateTakeGelFromCent(resinfo, resinfo.CenCode, ref seque_act);
                                        }
                                        else
                                            put_gel = generater.GenerateTakeGelFromNormal(resinfo, ref seque_act);
                                        exper_package.hatch_cur_time = 0;
                                        generater.GeneratePutGelToNormal(put_seat, put_gel, ref seque_act);
                                        next_state = 0;
                                    }
                                    else
                                    {
                                        next_state = 0;
                                    }
                                }
                            }
                            break;
                        case TestStepEnum.ZKDLXJ:
                            {
                                if (actionmanager.getAllActionsCount(handDevice) == 0)
                                {
                                    next_state = 0;
                                    if (resinfo.Purpose != "lxj" && !(resinfo.Purpose == "4" && actionmanager.getAllActionsCount(piercerDevice) != 0))
                                    {
                                        //查看可用离心机
                                        var centrifuge_list = resmanager.centrifuge_list.Where(item => item.Status == 1).ToList();
                                        centrifuge_list.Sort((a, b) => { return a.LastUseTime < b.LastUseTime ? 1 : -1; });
                                        foreach (var seat in centrifuge_list)
                                        {
                                            CentrifugeMDevice cendev = cenMrg.GetCentrifugeByCode(seat.Code);
                                            if (seat.Status == 1 && cendev != null)
                                            {
                                                device = handDevice;
                                                bool is_last_cent = seat == centrifuge_list[centrifuge_list.Count - 1];
                                                bool iscanputcen = IsCanPutCen(cendev);
                                                var put_seat = resmanager.GetResByCode("null", "T_BJ_Centrifuge", cendev.Centrifugem.Code.SetValue);
                                                if (iscanputcen == false) put_seat = null;
                                                //放进待定位
                                                if (put_seat == null && resinfo.Purpose != "3" && is_last_cent) put_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "3");
                                                if (put_seat != null)
                                                {
                                                    var spaw = Spawn.create();
                                                    var put_seque = Sequence.create();
                                                    if (put_seat.Purpose == "lxj")
                                                    {
                                                        //打开离心机门
                                                        var opendoor_act = HandOpenCloseDoor.create(handDevice, 5000, cendev.Centrifugem.Code.SetValue, true);
                                                        put_seque.AddAction(opendoor_act);
                                                    }
                                                    var put_gel = generater.GenerateTakeGelFromNormal(resinfo, ref put_seque);
                                                    spaw.AddAction(put_seque);
                                                    seque_act.AddAction(spaw);
                                                    if (put_seat.Purpose == "lxj")
                                                    {
                                                        spaw.AddAction(MoveTo.create(cendev, 30001, -1, -1, put_seat.CenGelP[put_seat.CountX]));
                                                        generater.GeneratePutGelToCent(cendev.Centrifugem.Code.SetValue, put_seat, put_gel, ref seque_act);
                                                    }
                                                    else
                                                        generater.GeneratePutGelToNormal(put_seat, put_gel, ref seque_act);
                                                    seat.LastUseTime = Engine.getInstance().getSystemMs();
                                                    break;
                                                }
                                                else
                                                {
                                                    next_state = 0;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    next_state = 0;
                                }
                            }
                            break;
                        case TestStepEnum.PutPeiGelBack:
                            {
                                var put_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "2");
                                if (put_seat != null && resinfo.Purpose == "lxj")
                                {
                                    device = handDevice;
                                    var put_gel = generater.GenerateTakeGelFromCent(resinfo, resinfo.CenCode, ref seque_act);
                                    generater.GeneratePutGelToNormal(put_seat, put_gel, ref seque_act);
                                    next_state = 2;
                                }
                            }
                            break;
                        case TestStepEnum.XJPD:
                            {
                                var put_seat = resmanager.GetResByCode("", "T_BJ_Camera");//相机位
                                var wasted_seat = resmanager.GetResByCode("", "T_BJ_WastedSeat","","1");//垃圾位
                                var daiding_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "3");//待定位
                                if (put_seat != null && wasted_seat != null && !(resinfo.Purpose == "4" && actionmanager.getAllActionsCount(piercerDevice) != 0))
                                {
                                    device = handDevice;
                                    ResInfoData put_gel = null;
                                    if (resinfo.Purpose == "lxj")
                                    {
                                        put_gel = generater.GenerateTakeGelFromCent(resinfo, resinfo.CenCode, ref seque_act);
                                    }
                                    else
                                        put_gel = generater.GenerateTakeGelFromNormal(resinfo, ref seque_act);

                                    seque_act.AddAction(MoveTo.create(handDevice, 3000, -1, -1, 0));
                                    seque_act.AddAction(MoveTo.create(handDevice, 3000, (int)put_seat.X, (int)(put_seat.Y), -1));
                                    seque_act.AddAction(MoveTo.create(handDevice, 3000, -1, -1, (int)(put_seat.Z)));
                                    //拍照分析
                                    seque_act.AddAction(SkCallBackFun.create((ActionBase acttem) =>
                                    {
                                        opDevice.CameraLight(true);
                                        bool result = true;
                                        if (cameraDevice.IsOpen == false) result = cameraDevice.Open();
                                        var bm = cameraDevice.CaptureImage();
                                        cameraDevice.Save(bm,exper_package);
                                        opDevice.CameraLight(false);
                                        return true;
                                    }));
                                    //是否还有动作
                                    if(exper_package.action_list.Count>1&& daiding_seat!=null)
                                    {
                                        seque_act.AddAction(MoveTo.create(handDevice, 3000, -1, -1, 0));
                                        generater.GeneratePutGelToNormal(daiding_seat, put_gel, ref seque_act);
                                    }
                                    else
                                    {
                                        seque_act.AddAction(MoveTo.create(handDevice, 3000, -1, -1, 0));
                                        seque_act.AddAction(MoveTo.create(handDevice, 3000, (int)wasted_seat.X, (int)(wasted_seat.Y)));
                                        seque_act.AddAction(HandPutCard.create(handDevice, 3000, (int)wasted_seat.ZPut, 0));
                                        seque_act.AddAction(SkCallBackFun.create((ActionBase act_tem)=>{
                                            DelPackage(exper_package);
                                            return true;
                                        }));
                                    }
                                    next_state = 2;
                                }
                            }
                            break;
                        case TestStepEnum.ECONOMIZECOUNTTIME:
                            {
                                next_state = 0;
                                if (exper_package.is_open)
                                {
                                    exper_package.after_kktime -= dt;
                                    if (exper_package.after_kktime <= 0)
                                    {
                                        device = handDevice;
                                        next_state = 2;
                                        exper_package.after_kktime = 0;
                                        ResInfoData put_gel = null;
                                        if (resinfo.Purpose == "lxj")
                                        {
                                            put_gel = generater.GenerateTakeGelFromCent(resinfo, resinfo.CenCode, ref seque_act);
                                        }
                                        else
                                            put_gel = generater.GenerateTakeGelFromNormal(resinfo, ref seque_act);

                                        var wasted_seat = resmanager.GetResByCode("", "T_BJ_WastedSeat");//垃圾位
                                        seque_act.AddAction(MoveTo.create(handDevice, 3000, -1, -1, 0));
                                        seque_act.AddAction(MoveTo.create(handDevice, 3000, (int)wasted_seat.X, (int)(wasted_seat.Y)));
                                        seque_act.AddAction(HandPutCard.create(handDevice, 3000, (int)wasted_seat.ZPut, 0));
                                    }
                                }
                            }
                            break;
                    }
                }
                if (device != null)
                {
                    seque_act.AddAction(SkCallBackFun.create((ActionBase act) =>
                    {
                        foreach(var act_tem in act_group)
                        {
                            act_tem.State = next_state;
                        }
                        return true;
                    }));
                    foreach (var act_tem in act_group)
                    {
                        act_tem.State = 1;
                    }
                    seque_act.runAction(device);
                }
                else
                {
                    foreach (var act_tem in act_group)
                    {
                        act_tem.State = next_state;
                    }
                }
            }
        }
        //得到初始最大取针
        public int GetInjectCount()
        {
            //连续动作ID相同且不同卡
            var inj_action_list_tem = inj_action_list.Where(item => ((ExperimentPackage)item.ExperPackage).is_jyjs == false).ToList();
            inj_action_list_tem = inj_action_list_tem.Where(item => item.State == 0).ToList();
            if (inj_action_list_tem.Count == 0) return 4;
            var action_tem = inj_action_list_tem[0];
            var exp_pack = (ExperimentPackage)action_tem.ExperPackage;
            int count = 0;
            int action_count = inj_action_list_tem.Count > 4 ? 4 : inj_action_list_tem.Count;
            //如果是交叉配血最小两个针
            if (exp_pack.is_crossmatching) action_count = exp_pack.samples_barcode.Count;
            if (exp_pack.is_crossmatching&& action_count<2) action_count = 2;
            for (int i=0;i< action_count; i++)
            {
                var exp_pack_tem = (ExperimentPackage)action_tem.ExperPackage;
                var exp_pack_cur = (ExperimentPackage)inj_action_list_tem[i].ExperPackage;
                //第4个针头不能是第一个样本架吸液动作
                bool is_frist_sample = false;
                if (i == 3 && inj_action_list_tem[i].StepClass == TestStepEnum.FPYT && exp_pack_cur.sample_index == 0)
                {
                    var liquidinfo = inj_action_list_tem[i].GetLiquidInfo();
                    if (liquidinfo != null)
                    {
                        string liquidtype = liquidinfo.LiquidType;
                        is_frist_sample = (liquidtype.IndexOf("血清") != -1 || liquidtype.IndexOf("红细胞") != -1);
                    }
                }
                if (is_frist_sample==false&&inj_action_list_tem[i].StepIndex== action_tem.StepIndex&& exp_pack_tem.gel_test_id == exp_pack_cur.gel_test_id)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            if (count <= 0) count = 1;
            return count;
        }
        //执行
        public void Logic(double dt)
        {
            ClsAction();
            //并行执行
            foreach (var pack in experiment_package_list)
            {
                if (pack.action_list.Count != 0)
                {
                    if (pack.is_jyjs)
                    {
                        var cen_tree = generater.DivideIntoGroups(pack.action_list, 1);
                        ActLogic(cen_tree[0], null, dt);
                    }
                }
            }
            //离心逻辑
            bool is_use_cent = false;
            foreach (var cent in cenMrg.CentrifugeMDevices)
            {
                if(resmanager.GetCenStatus(cent.Centrifugem.Code.SetValue))
                {
                    //如果当前离心机在跑或已满卡就添加到下一个离心机中
                    is_use_cent = CenRunLogic(cent);
                    if (is_use_cent) break;// 离心启动逻辑
                }
            }
            if(is_use_cent==false)
            {
                //顺序执行
                int inject_count = 1;
                var inj_action_list_tem = inj_action_list.Where(item => ((ExperimentPackage)item.ExperPackage).is_jyjs == false).ToList();
                if (inj_action_list_tem.Count != 0)
                {
                    var action_tem = inj_action_list_tem[0];
                    var exp_pack = (ExperimentPackage)action_tem.ExperPackage;
                    var paper_seat = resmanager.GetResByCode(exp_pack.GetGelMask(), "T_BJ_GelSeat", "", "4");
                    var liquidinfo = action_tem.GetLiquidInfo();
                    inject_count = GetInjectCount();
                    //如果是加样试剂最多两个针头
                    if (liquidinfo != null && liquidinfo.IsAgentia && inject_count > 2)
                    {
                        inject_count = 2;
                    }
                    var inj_tree = generater.DivideIntoGroups(inj_action_list_tem, inject_count);
                    var act_group = inj_tree[0];
                    var act_group_next = inj_tree.Count > 1 ? inj_tree[1] : null;
                    ActLogic(act_group, act_group_next, dt);
                }
            }
            //机器手空闲回零
            if (actionmanager.getAllActionsCount(handDevice) == 0 && handDevice.Hand.XMotor.CurrentDistance != 0)
            {
                hand_wait_time += dt;
                if (hand_wait_time > 1000)
                {
                    var act = Sequence.create(MoveTo.create(handDevice, 10000, -1, -1, 0),
                        MoveTo.create(handDevice, 10000, 0, 0, 0), 
                        InitXyz.create(10000, true, true, true));
                    act.runAction(handDevice);
                    hand_wait_time = 0;
                }
            }
            //加样器空闲回零
            if (actionmanager.getAllActionsCount(injectorDevice) == 0 && injectorDevice.Injector.XMotor.CurrentDistance != 0)
            {
                inj_wait_time += dt;
                if (inj_wait_time > 1000)
                {
                    var by = IMask.Gen(0);
                    foreach (var ent in injectorDevice.Injector.Entercloses)
                    {
                        by[ent.Index] = (int)ent.YZero;
                    }
                    var act = Sequence.create(
                        InjectMoveTo.create(injectorDevice, 3000, injectorDevice.Injector.Entercloses, -1, IMask.Gen(-1), IMask.Gen(0)),
                        InjectMoveTo.create(injectorDevice, 3000, injectorDevice.Injector.Entercloses, 0, IMask.Gen(0), IMask.Gen(-1)),
                        InitXyz.create(injectorDevice, 20000, injectorDevice.Injector.Entercloses, true, true, true, by));
                    act.runAction(injectorDevice);
                    inj_wait_time = 0;
                }
            }
            else
            {
                inj_wait_time = 0;
            }

        }

        //节省卡摸试
        public void EconomizeGelLogic(double dt)
        {
            //计算开孔时间
            foreach (var pack in experiment_package_list)
            {
                if (pack.is_used_gel && pack.is_open)
                {
                    pack.after_kktime -= dt;
                    if (pack.after_kktime <= 0)
                    pack.after_kktime = 0;
                }
            }
        }

        public void runLoop(double time)
        {
            lock (mylock)
            {
                double dt = time - lasttime;
                if (dt < 50 || (experiment_package_list.Count == 0)) return;
                lasttime = time;
                Logic(dt);
            }
        }
    }
}
