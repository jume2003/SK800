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
    public class InjLogicManager
    {
        public MachineHandDevice handDevice = null;
        public PiercerDevice piercerDevice = null;
        public GelWarehouseDevice gelwareDevice = null;
        public InjectorDevice injectorDevice = null;
        public ResManager resmanager = ResManager.getInstance();
        public ActionManager actionmanager = ActionManager.getInstance();
        public ActionGenerater generater = ActionGenerater.getInstance();
        public List<List<T_GelStep>> action_tree = new List<List<T_GelStep>>();
        private Object mylock = new Object();
        public double lasttime = 0;
        public double inj_wait_time = 0;
        public static InjLogicManager injlogicmanager = null;
        public int setp = 0;
        public static InjLogicManager getInstance()
        {
            if (injlogicmanager == null) injlogicmanager = new InjLogicManager();
            return injlogicmanager;
        }
        public void AddAction(List<List<T_GelStep>> acttree)
        {
            lock (mylock)
            {
                action_tree = action_tree.Concat(acttree).ToList();
            }
        }
        public void ClsAction()
        {
            lock(mylock)
            {
                //清理完成动作
                bool iscls = false;
                for (int i = action_tree.Count - 1; i >= 0; i--)
                {
                    if (action_tree[i][0].State == 2)
                    {
                        action_tree.Remove(action_tree[i]);
                        iscls = true;
                    }
                }
                if(iscls&& action_tree.Count == 0)
                {
                    var act = Sequence.create(
                             InjectMoveTo.create(injectorDevice, 3000, injectorDevice.GetSeleteced(), -1, IMask.Gen(-1), IMask.Gen(0)),
                             InjectMoveTo.create(injectorDevice, 3000, injectorDevice.GetSeleteced(), 0, IMask.Gen(0), IMask.Gen(-1)),
                             InitXyz.create(injectorDevice, 5000, injectorDevice.GetSeleteced(), true, true, true));
                    act.runAction(injectorDevice);
                }
            }
        }
        public void Logic(double dt)
        {
            lock (mylock)
            {
                if (action_tree.Count == 0) return;
                //机器手空闲回零
                if (actionmanager.getAllActionsCount(injectorDevice) == 0 && injectorDevice.Injector.XMotor.CurrentDistance != 0)
                {
                    inj_wait_time += dt;
                    if (inj_wait_time > 1000)
                    {
                        var act = Sequence.create(
                            InjectMoveTo.create(injectorDevice,3000,injectorDevice.GetSeleteced(), -1,IMask.Gen(-1), IMask.Gen(0)),
                            InjectMoveTo.create(injectorDevice, 3000, injectorDevice.GetSeleteced(), 0, IMask.Gen(0), IMask.Gen(-1)),
                            InitXyz.create(injectorDevice, 10000,injectorDevice.GetSeleteced(),true,true,true));
                        act.runAction(injectorDevice);
                        inj_wait_time = 0;
                    }
                }
                var act_group = action_tree[0];
                if (act_group.Count != 0 && act_group[0].State == 0)
                {
                    var seque_act = Sequence.create();
                    AbstractCanDevice device = null;
                    if (act_group[0].InjectCount != 0)
                    {
                        foreach (var act_tem in act_group)
                        {
                            resmanager.tip_count += act_tem.InjectCount;
                        }
                        device = injectorDevice;
                        foreach (var act in act_group)
                            act.UpdataPoints();
                        seque_act = generater.GenerateAction(act_group);
                    }
                    else
                    {
                        foreach (var act in act_group)
                        {
                            act.UpdataPoints();
                            switch (act.StepClass)
                            {
                                case TestStepEnum.LoadGel:
                                    {
                                        device = handDevice;
                                        var paper_seat = resmanager.GetResByCode("null", "T_BJ_GelSeat", "", "4");
                                        //var ware_seat = resmanager.GetResByCode(act.GetGelMask(), "T_BJ_GelWarehouse");
                                        seque_act.AddAction(HandTakeGelFromWare.create(handDevice, 3001, gelwareDevice, act.GelMaskID, act.GetGelMask(), act.SampleBarCode));
                                        seque_act.AddAction(HandPutGelToNormal.create(handDevice,3001,gelwareDevice, paper_seat));
                                    }
                                    break;
                                case TestStepEnum.KaiKongGel:
                                    {
                                        int handrx = 0;
                                        var device_hand = new ActionDevice(handDevice);
                                        var paper_seat = resmanager.GetResByCode(act.GetGelMask(), "T_BJ_GelSeat", "", "4");
                                        if (paper_seat != null&&device_hand.GetRealX(ref handrx)&&
                                            (handrx+1000)< paper_seat.X&& ActionDevice.hand_tx < paper_seat.X)
                                        {
                                            device = piercerDevice;
                                            seque_act.AddAction(PaperCard.create(piercerDevice, 3000, paper_seat));
                                            seque_act.AddAction(SkCallBackFun.create((ActionBase acttem) => {
                                                resmanager.gel_count++;
                                                return true;
                                            }));
                                        }
                                        else
                                        {
                                            return;
                                        }
                                    }
                                    break;
                                case TestStepEnum.JYJS:
                                    {
                                        device = injectorDevice;
                                        //得到剩下动作
                                        List<T_GelStep> act_list = new List<T_GelStep>();
                                        foreach (var act_tem in action_tree)
                                        {
                                            if (act_tem.Count == 1 && act_tem[0].StepClass != TestStepEnum.GELEND)
                                            {
                                                act_tem[0].State = 2;
                                                act_list.Add(act_tem[0]);
                                            }
                                            else break;
                                        }
                                        act_list.Remove(act);
                                        var actplist = ActionPoint.GenActList(act_list);
                                        var paper_seat = resmanager.GetResByCode(act.GetGelMask(), "T_BJ_GelSeat", "", "4");
                                        foreach (var actp in actplist)
                                        {
                                            paper_seat.ActionList.Add(actp.type);
                                            if (actp.hatchtime != 0)
                                                paper_seat.HatchTime = actp.hatchtime;
                                        }
                                        seque_act.AddAction(SkCallBackFun.create((ActionBase acttem) =>
                                        {
                                            paper_seat.InjectFinish = true;
                                            return true;
                                        }));
                                    }
                                    break;
                            }
                        }
                    }

                    if (device != null)
                    {
                        seque_act.AddAction(SkCallBackFun.create((ActionBase act) =>
                        {
                            act_group[0].State = 2;
                            return true;
                        }));
                        act_group[0].State = 1;
                        seque_act.runAction(device);
                    }
                    else
                    {
                        act_group[0].State = 2;
                    }
                }
            }
        }
        public void runLoop(double time)
        {
            double dt = time - lasttime;
            if (dt < 50) return;
            lasttime = time;
            ClsAction();
            Logic(dt);
        }
    }
}
