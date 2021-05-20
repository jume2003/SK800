using SKABO.BLL.IServices.ILogic;
using SKABO.Common.Models.Logic;
using SKABO.Common.Utils;
using Stylet;
using SKABO.Common;
using System.Windows.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System;
using SKABO.Hardware.RunBJ;
using System.Windows;
using SKABO.Common.Models.BJ;
using SKABO.Hardware.Core;
using SKABO.Hardware.Model;
using SKABO.MAI.ErrorSystem;
using SKABO.ActionEngine;
using SKABO.Common.Enums;
using SKABO.ResourcesManager;
using SKABO.Common.Models.Communication.Unit;
using SKABO.ActionGeneraterEngine;

namespace SK_ABO.Views.LogicProgram
{
    public class LogicManagerViewModel:Screen
    {
        [StyletIoC.Inject]
        private ILogicService logicService { get; set; }
        [StyletIoC.Inject]
        private IWindowManager windowManager { get; set; }
        [StyletIoC.Inject]
        private OtherPartDevice opDevice { get; set; }
        [StyletIoC.Inject]
        private MachineHandDevice handDevice { get; set; }
        [StyletIoC.Inject]
        private CentrifugeDevice centDevice { get; set; }
        [StyletIoC.Inject]
        private CentrifugeMrg cenMrg;
        [StyletIoC.Inject]
        private PiercerDevice pieDevice { get; set; }
        [StyletIoC.Inject]
        private GelWarehouseDevice gelwareDevice { get; set; }
        [StyletIoC.Inject]
        private InjectorDevice injDevice { get; set; }
        public LogicManagerViewModel()
        {
            this.DisplayName = "逻辑编程";
        }
        public Stylet.BindableCollection<T_LogicTest> LogicList{ get; set; }
        public T_LogicTest SelectedItem { get; set; }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            var list = logicService.QueryT_LogicTest();
            LogicList = new BindableCollection<T_LogicTest>();
            LogicList.AddRange(list);
            SetBtnCan(false);
        }
        public new bool CanClose { get; set; }
        public bool CanRun { get; set; }
        public bool CanDelete { get; set; }
        public bool CanCopy { get; set; }
        public bool CanEdit { get; set; }
        public bool CanNew { get; set; }
        private void SetBtnCan(bool running)
        {
            this.CanClose = !running;
            this.CanDelete = !running;
            this.CanEdit = !running;
            this.CanNew = !running;
            this.CanRun = !running;
            this.CanCopy = !running;
        }
        public void Close()
        {
            this.RequestClose();
        }
        public void Run()
        {
            if (SelectedItem == null) return;
            String msg = "";
            if (CheckSteps(SelectedItem.LogicSteps, out msg))
            {
                SetBtnCan(true);
                Task task = new Task(() =>
                  {
                      var flag=RunLogic(SelectedItem.LogicSteps,out String msg1);
                      if (_Timers != null)
                      {
                          foreach(var kv in _Timers)
                          {
                              kv.Value.Dispose();
                          }
                          _Timers.Clear();
                      }
                      SetBtnCan(false);
                  });
                task.Start();
            }
            else
            {
                this.View.ShowHint(new MessageWin(msg));
            }
        }
        private T Convert2T<T>(string param) where T: class
        {
            
            return (T)(object)param.ToInstance<T>();
        }
    private T Convert2TVal<T>(string param) where T : struct
    {
            if (typeof(T).Equals(typeof(int)))
            {
                return (T)(object)int.Parse(param);
            }
            return default(T);
        }
    private bool CheckSteps(IList<T_LogicStep> steps, out string msg)
        {
            msg = null;
            var result = true;
            var loops=steps.Where(s => s.StepEnum == SKABO.Common.Enums.LogicStepEnum.LoopStart);
            IList<int> indexList = new List<int>();
            foreach(var loop in loops)
            {
                LogicLoop logicLoop = Convert2T<LogicLoop>(loop.Parameters);
                if (indexList.Contains(logicLoop.Index))
                {
                    result = false;
                    msg = $"循环{logicLoop.Index}重复";
                    break;
                }
                else
                {
                    indexList.Add(logicLoop.Index);
                }
                var endLoop=steps.Where(s => s.StepEnum == SKABO.Common.Enums.LogicStepEnum.LoopEnd && s.Parameters== logicLoop.Index+"").FirstOrDefault();
                if (endLoop == null)
                {
                    result = false;
                    msg = $"循环{logicLoop.Index}没有结束";
                    break;
                }
            }
            var timers = steps.Where(s => s.StepEnum == SKABO.Common.Enums.LogicStepEnum.TimerStart);
            indexList.Clear();
            foreach (var t in timers)
            {
                LogicTimer lt = Convert2T<LogicTimer>(t.Parameters);
                if (indexList.Contains(lt.Index))
                {
                    result = false;
                    msg = $"计时器{lt.Index}重复";
                    break;
                }
                else
                {
                    indexList.Add(lt.Index);
                }

            }
                return result;
        }
        private IDictionary<int, Semaphore> _Timers;
        private IDictionary<int, Semaphore> Timers
        {
            get
            {
                if (_Timers == null)
                {
                    _Timers = new Dictionary<int, Semaphore>();
                }
                return _Timers;
            }
        }
        private bool RunLogic(IList<T_LogicStep> steps,out String msg)
        {
            msg = null;
            var seque_root = Sequence.create();
            var seque = Sequence.create();
            var resmanager = ResManager.getInstance();
            var generater = ActionGenerater.getInstance();
            int loop_times = 0;
            foreach (var step in steps)
            {
                switch (step.StepEnum)
                {
                    case LogicStepEnum.Alert:
                        seque.AddAction(SkCallBackFun.create((ActionBase act_tem) =>
                        {
                            ErrorSystem.WriteActError(step.Parameters, true, false, 9999);
                            return true;
                        }));
                        break;
                    case LogicStepEnum.InitAll:
                        var act_init = opDevice.InitAllDevice();
                        seque.AddAction(act_init);
                        break;
                    case LogicStepEnum.Centrifuge:
                        {
                            CommonAction action = Convert2T<CommonAction>(step.Parameters);
                            var cen = cenMrg.GetCentrifugeByCode(action.Code);
                            if(cen!=null)
                            {
                                if (action.DoAction == 0)//正常离心程序
                                {
                                    int hspeed = (int)cen.Centrifugem.HightSpeed.SetValue;
                                    int lspeed = (int)cen.Centrifugem.LowSpeed.SetValue;
                                    int htime = (int)cen.Centrifugem.HightSpeedTime.SetValue;
                                    int ltime = (int)cen.Centrifugem.LowSpeedTime.SetValue;
                                    int uphtime = (int)cen.Centrifugem.AddHSpeedTime.SetValue;
                                    int upltime = (int)cen.Centrifugem.AddLSpeedTime.SetValue;
                                    int stime = (int)cen.Centrifugem.StopSpeedTime.SetValue;
                                    var act = CentrifugeStart.create(cen, 3000000, hspeed, lspeed, htime, ltime, uphtime, upltime, stime);
                                    seque.AddAction(act);
                                }
                                else if (action.DoAction == 1)//移动指定卡位
                                {
                                    var resinfo = ResManager.getInstance().SearchGelCard("T_BJ_Centrifuge", action.Code, "", (int)action.SeatIndex);
                                    var act = MoveTo.create(cen, 30000, -1, -1, resinfo.CenGelP[resinfo.CountX], 5);
                                    seque.AddAction(act);
                                }
                                else if (action.DoAction == 2)//开舱门
                                {
                                    var act = HandOpenCloseDoor.create(handDevice, 3000, action.Code, true);
                                    seque.AddAction(act);
                                }
                                else if (action.DoAction == 3)//关舱门
                                {
                                    var act = HandOpenCloseDoor.create(handDevice, 3000, action.Code, false);
                                    seque.AddAction(act);
                                }
                                else if (action.DoAction == 4)//初始化
                                {
                                    var act = InitXyz.create(cen, 3000, false, false,true);
                                    seque.AddAction(act);
                                }
                            }
                            else
                            {
                                ErrorSystem.WriteActError("找不到指定离心机", true, false, 9999);
                            }
                        }
                        break;
                    case LogicStepEnum.Couveuse:
                        {
                            
                        }
                        break;
                    case LogicStepEnum.Delay:
                        {
                            int DelayTime = Convert.ToInt32(step.Parameters);
                            seque.AddAction(SKSleep.create(DelayTime));
                        }
                        break;
                    case LogicStepEnum.DetectSquid:
                        {
                            DetectAction action = Convert2T<DetectAction>(step.Parameters);
                            List<Enterclose> iinjects = new List<Enterclose>();
                            foreach (var entindex in action.Indexs)
                            {
                                iinjects.Add(injDevice.Injector.Entercloses[entindex]);
                            }
                            seque.AddAction(InjectDetector.create(3000, iinjects.ToArray(),IMask.Gen((int)action.Start),IMask.Gen((int)action.ZLimit),IMask.Gen(0)));
                        }
                        break;
                    case LogicStepEnum.SimpleAction:
                        {
                            SimpleAction action = Convert2T<SimpleAction>(step.Parameters);
                            AbstractCanDevice device = null;
                            switch (action.Device)
                            {
                                case "加样器":
                                    device = injDevice;
                                    break;
                                case "机械手":
                                    device = handDevice;
                                    break;
                                case "卡仓":
                                    device = gelwareDevice;
                                    break;
                                case "破孔器":
                                    device = gelwareDevice;
                                    break;
                            }
                            if (action.Action == "移动")
                            {
                                ActionBase act_move = null;
                                int x = action.Direction == "X" ? (int)action.Value : -1;
                                int y = action.Direction == "Y" ? (int)action.Value : -1;
                                int z = action.Direction == "Z"? (int)action.Value:-1;
                                if (action.Direction == "XYZ") x = y = z = (int)action.Value;
                                if (device is InjectorDevice)
                                {
                                    act_move = InjectMoveTo.create(device,3000, injDevice.Injector.Entercloses, x, IMask.Gen(y), IMask.Gen(z));
                                }
                                else
                                {
                                    act_move = MoveTo.create(device,3000,x, y, z);
                                }
                                seque.AddAction(act_move);
                            }
                            else if (action.Action == "初始化")
                            {
                                ActionBase act_move = null;
                                bool x = action.Direction == "X" ? true : false;
                                bool y = action.Direction == "Y" ? true : false;
                                bool z = action.Direction == "Z" ? true : false;
                                if (action.Direction == "XYZ") x = y = z = true;
                                if (device is InjectorDevice)
                                {
                                    act_move = InitXyz.create(device, 30000, injDevice.Injector.Entercloses, x, y, z);
                                }
                                else
                                {
                                    act_move = InitXyz.create(device, 30000, x, y, z);
                                }
                                seque.AddAction(act_move);
                            }
                        }
                        break;
                    case LogicStepEnum.TakeAndPutDownGel:
                        {
                            GelAction[] actions = Convert2T<GelAction[]>(step.Parameters);
                            seque.AddAction(MoveTo.create(handDevice, 3000, -1, -1, 0));
                            ResInfoData take_seat = null;
                            if (actions[0].BJType== "T_BJ_GelSeat")
                            {
                                take_seat = resmanager.SearchGelCard("T_BJ_GelSeat", "", "", actions[0].Index,0,true, actions[0].BJName);
                                generater.GenerateTakeGelFromNormal(take_seat, ref seque);
                            }
                            else if (actions[0].BJType == "T_BJ_Centrifuge")
                            {
                                take_seat = resmanager.SearchGelCard("T_BJ_Centrifuge", "", "", actions[0].Index, 0, true, actions[0].BJName);
                                var act = generater.GenerateTakeGelFromCent(take_seat, "", ref seque);
                            }
                            else if (actions[0].BJType == "T_BJ_GelWarehouse")
                            {
                                take_seat = resmanager.SearchGelCard("T_BJ_GelWarehouse", "", "", actions[0].Index, 0, true, actions[0].BJName);
                                var act = generater.GenerateTakeGelFromWare(take_seat, ref seque);
                            }

                            if (actions[1].BJType == "T_BJ_GelSeat")
                            {
                                var put_seat = resmanager.SearchGelCard("T_BJ_GelSeat", "", "", actions[1].Index, 0, true, actions[1].BJName);
                                generater.GeneratePutGelToNormal(put_seat, take_seat, ref seque);
                            }
                            else if (actions[1].BJType == "T_BJ_Centrifuge")
                            {
                                var put_seat = resmanager.SearchGelCard("T_BJ_Centrifuge", "", "", actions[1].Index, 0, true, actions[1].BJName);
                                generater.GeneratePutGelToCent(put_seat.CenCode, put_seat, take_seat, ref seque);
                            }
                            else if (actions[1].BJType == "T_BJ_GelWarehouse")
                            {
                                var put_seat = resmanager.SearchGelCard("T_BJ_GelWarehouse", "", "", actions[1].Index, 0, true, actions[1].BJName);
                                generater.GeneratePutGelToWare(put_seat, take_seat, ref seque);
                            }
                        }
                        break;
                    case LogicStepEnum.TakeGel:
                        {
                            GelAction action = Convert2T<GelAction>(step.Parameters);
                            ResInfoData take_seat = null;
                            if (action.BJType == "T_BJ_GelSeat")
                            {
                                take_seat = resmanager.SearchGelCard("T_BJ_GelSeat", "", "", action.Index, 0, true, action.BJName);
                                generater.GenerateTakeGelFromNormal(take_seat, ref seque);
                            }
                            else if (action.BJType == "T_BJ_Centrifuge")
                            {
                                take_seat = resmanager.SearchGelCard("T_BJ_Centrifuge", "", "", action.Index, 0, true, action.BJName);
                                var act = generater.GenerateTakeGelFromCent(take_seat, "", ref seque);
                            }
                            else if (action.BJType == "T_BJ_GelWarehouse")
                            {
                                take_seat = resmanager.SearchGelCard("T_BJ_GelWarehouse", "", "", action.Index, 0, true, action.BJName);
                                var act = generater.GenerateTakeGelFromWare(take_seat, ref seque);
                            }
                            break;
                        }
                    case LogicStepEnum.PutDownGel:
                        {
                            GelAction action = Convert2T<GelAction>(step.Parameters);
                            ResInfoData take_seat = resmanager.handseat_resinfo;
                            if (action.BJType == "T_BJ_GelSeat")
                            {
                                var put_seat = resmanager.SearchGelCard("T_BJ_GelSeat", "", "", action.Index, 0, true, action.BJName);
                                generater.GeneratePutGelToNormal(put_seat, take_seat, ref seque);
                            }
                            else if (action.BJType == "T_BJ_Centrifuge")
                            {
                                var put_seat = resmanager.SearchGelCard("T_BJ_Centrifuge", "", "", action.Index, 0, true, action.BJName);
                                generater.GeneratePutGelToCent(put_seat.CenCode, put_seat, take_seat, ref seque);
                            }
                            else if (action.BJType == "T_BJ_GelWarehouse")
                            {
                                var put_seat = resmanager.SearchGelCard("T_BJ_GelWarehouse", "", "", action.Index, 0, true, action.BJName);
                                generater.GeneratePutGelToWare(put_seat, take_seat, ref seque);
                            }
                            break;
                        }
                    case LogicStepEnum.TakeTip:
                        {
                            TakeTipAction action = Convert2T<TakeTipAction>(step.Parameters);
                            var ents = injDevice.Injector.Entercloses.Where(ent => ent.Valid && action.Indexs.Contains(ent.Index)).ToArray();
                            var tipseat = Constants.BJDict[typeof(T_BJ_Tip).Name].Where(item => item.Name == action.TipBoxName).FirstOrDefault() as T_BJ_Tip;
                            List<ActionPoint> tip_seat = new List<ActionPoint>();
                            foreach (var ent in ents)
                            {
                                int x = (int)(tipseat.X - action.C * tipseat.GapX);
                                int y = (int)(tipseat.Y + action.R * tipseat.GapY);
                                var point = new ActionPoint();
                                point.x = x;
                                point.y = y;
                                point.z = (int)tipseat.Limit;
                                point.type = TestStepEnum.JXZT;
                                point.index = ent.Index;
                                tip_seat.Add(point);
                            }
                            var sequ_taketip = Sequence.create();
                            var move_act = InjectMoveActs.create(injDevice,3000, tip_seat.ToArray(), false);
                            sequ_taketip.AddAction(InjectMoveTo.create(injDevice,3000, ents, -1, IMask.Gen(-1), IMask.Gen(0)));
                            sequ_taketip.AddAction(move_act);
                            seque.AddAction(sequ_taketip);
                        }
                        break;
                    case LogicStepEnum.OutTip:
                        {
                            TakeTipAction action = Convert2T<TakeTipAction>(step.Parameters);
                            var ents = injDevice.Injector.Entercloses.Where(ent => ent.Valid && action.Indexs.Contains(ent.Index)).ToArray();
                            var sequ_puttip = Sequence.create();
                            List<ActionPoint> unload_seat = new List<ActionPoint>();
                            var inject_unload = resmanager.unload_list;
                            if (inject_unload.Count() == 1)
                            {
                                var unloader = inject_unload[0];
                                for (int i = 0; i < 4; i++)
                                {
                                    var unload_point = new ActionPoint((int)unloader.X, (int)unloader.Y + i * (int)unloader.FZ, (int)unloader.Z, TestStepEnum.JXZT);
                                    unload_point.puttip_x = (int)unloader.FirstX;
                                    unload_seat.Add(unload_point);
                                }
                                sequ_puttip.AddAction(InjectMoveTo.create(injDevice,3000, ents, -1, IMask.Gen(-1), IMask.Gen(0)));
                                sequ_puttip.AddAction(InjectMoveActs.create(injDevice,3000, unload_seat.ToArray(), true));
                                seque.AddAction(sequ_puttip);
                            }
                        }
                        break;
                    case LogicStepEnum.Piercer:
                        {
                            var paper_seat = resmanager.GetResByCode("", "T_BJ_GelSeat", "", "4");
                            seque.AddAction(PaperCard.create(pieDevice,3000, paper_seat,0));
                        }
                        break;
                    case LogicStepEnum.LoopStart:
                        {
                            LogicLoop logicLoop = Convert2T<LogicLoop>(step.Parameters);
                            loop_times = logicLoop.Count;
                            seque_root.AddAction(seque);
                            seque = Sequence.create();
                        }
                        break;
                    case LogicStepEnum.LoopEnd:
                        {
                            var act_repat = Repeat.create(seque, loop_times);
                            seque_root.AddAction(act_repat);
                            seque = Sequence.create();
                            loop_times = 0;
                        }
                        break;
                }

            }
            seque_root.AddAction(seque);
            seque_root.runAction(opDevice);
            return true;
        }
        private void timerCallback(object obj)
        {
            if(obj is Semaphore sph)
            {
                sph.Release(1);
            }
        }
        public void Delete()
        {
            if (SelectedItem == null) return;
            var diagle = windowManager.ShowMessageBox(String.Format("确定要删除【{0}】", SelectedItem.Name), "系统提示", MessageBoxButton.YesNo);
            if (diagle == MessageBoxResult.Yes)
            {
                var res = this.logicService.DeleteT_LogicTest(SelectedItem.ID);
                if (res)
                {
                    LogicList.Remove(SelectedItem);
                }
                this.View.ShowHint(new MessageWin(res));
            }
        }
        public void Copy()
        {
            if (SelectedItem == null) return;
            var newP=this.logicService.QueryT_LogicTestById(SelectedItem.ID);
            newP.ID = 0;
            if (newP.LogicSteps != null)
            {
                foreach(var t in newP.LogicSteps)
                {
                    t.ID = 0;
                    t.ProgramID = 0;
                }
            }
            var res = this.logicService.SaveT_LogicTest(newP);
            if (res)
            {
                this.LogicList.Add(newP);
            }
            this.View.ShowHint(new MessageWin(res));
        }
        public void Edit()
        {
            if (SelectedItem == null) return;
            var vm = IoC.Get<NewLogicViewModel>();
            vm.Program = SelectedItem;
            windowManager.ShowDialog(vm);
        }
        public void New()
        {
            var vm = IoC.Get<NewLogicViewModel>();
            vm.Program = new T_LogicTest();
            vm.SaveEvent += Vm_SaveEvent;
            windowManager.ShowDialog(vm);
        }

        private void Vm_SaveEvent(T_LogicTest t_LogicTest)
        {
            LogicList.Add(t_LogicTest);
        }

        public void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var Row = DataGridUtil.GetRowMouseDoubleClick(sender, e);
            if (Row == null) return;
            Edit();
        }
    }
}
