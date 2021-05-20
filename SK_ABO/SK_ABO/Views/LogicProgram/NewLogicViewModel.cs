using SK_ABO.Views.LogicProgram.LogicStep;
using SKABO.Common.Models.Logic;
using Stylet;
using System;
using System.Linq;
using SKABO.Common;
using SKABO.Common.Utils;
using System.Windows;
using System.Windows.Controls;
using SKABO.BLL.IServices.ILogic;
using System.Windows.Input;
using SKABO.Common.Enums;
using System.Collections.Generic;

namespace SK_ABO.Views.LogicProgram
{
    public class NewLogicViewModel:Screen
    {
        [StyletIoC.Inject]
        private ILogicService logicService { get; set; }
        [StyletIoC.Inject]
        private IWindowManager windowManager { get; set; }
        private BindableCollection<T_LogicStep> _StepList { get; set; }
        public BindableCollection<T_LogicStep> StepList
        {
            get
            {
                if (_StepList == null)
                {
                    _StepList = new BindableCollection<T_LogicStep>();
                    if (Program.LogicSteps != null)
                    {
                        _StepList.AddRange(Program.LogicSteps);
                    }
                }
                return _StepList;
            }
        }
        public T_LogicTest Program { get; set; }
        public List<T_LogicStep> SelectedStep { get; set; } = new List<T_LogicStep>();
        public delegate void SaveEventHandler(T_LogicTest t_LogicTest);
        public event SaveEventHandler SaveEvent;
        /// <summary>
        /// 添加步骤
        /// </summary>
        public void Add(Button btn)
        {
            var menu = (this.View as Window).FindResource("AddMenu") as ContextMenu;
            menu.PlacementTarget = btn;
            menu.IsOpen = true;
        }
        public void Close()
        {
            this.RequestClose();
        }
        public void AddStep(MenuItem Item)
        {
            String TagStr = Item.Tag.ToString();
            String StepName = Item.Header.ToString();
            int StepID = 0;
            int.TryParse(TagStr, out StepID);
            LogicStepScreen vm = GetViewModel(TagStr);
            if (vm == null)
            {
                M_ConfirmEvent(null, new T_LogicStep() { Name = StepName ,StepID= StepID ,StepEnum= (LogicStepEnum)Convert.ToByte(TagStr) });
            }
            else if (vm is LogicStepScreen m)
            {
                m.ConfirmEvent += M_ConfirmEvent;
                m.Name = StepName;
                m.StepID = StepID;
                windowManager.ShowDialog(vm);
            }
            
        }
        #region Index得到ViewModel
        private LogicStepScreen GetViewModel(String IndexStr)
        {
            LogicStepScreen vm = null;
            LogicStepEnum stepEnum = (LogicStepEnum)Convert.ToByte(IndexStr);
            switch (stepEnum)
            {
                case LogicStepEnum.InitAll:
                case LogicStepEnum.InitZ:
                case LogicStepEnum.InitS:
                    {
                        break;
                    }
                case LogicStepEnum.Alert:
                    {
                        vm = IoC.Get<MessageViewModel>();
                        //
                        break;
                    }
                case LogicStepEnum.Delay:
                    {
                        vm = IoC.Get<Time_lapseViewModel>();
                        break;
                    }
                case LogicStepEnum.TimerStart:
                    {
                        vm = IoC.Get<TimingViewModel>();
                        break;
                    }
                case LogicStepEnum.WaitTimer:
                    {
                        vm = IoC.Get<WaitTimingViewModel>();
                        break;
                    }
                case LogicStepEnum.LoopStart:
                    {
                        vm = IoC.Get<LoopViewModel>();
                        break;
                    }
                case LogicStepEnum.LoopEnd:
                    {
                        vm = IoC.Get<LoopEndViewModel>();
                        break;
                    }
                case LogicStepEnum.SubFunc:
                    {
                        vm = IoC.Get<SubFuncViewModel>();
                        break;
                    }
                case LogicStepEnum.Hex:
                    {
                        vm = IoC.Get<HexViewModel>();
                        break;
                    }
                case LogicStepEnum.OutTip:
                    {
                        vm = IoC.Get<UploadTipViewModel>();
                        break;
                    }
                case LogicStepEnum.TakeTip:
                    {
                        vm = IoC.Get<TakeTipViewModel>();
                        break;
                    }
                case LogicStepEnum.MoveXY:
                    {
                        vm = IoC.Get<XYMoveViewModel>();
                        break;
                    }
                case LogicStepEnum.MoveZ:
                    {
                        vm = IoC.Get<ZMoveViewModel>();
                        break;
                    }
                case LogicStepEnum.SimpleAction:
                    {
                        vm = IoC.Get<ActionViewModel>();
                        break;
                    }
                case LogicStepEnum.DetectSquid:
                    {
                        vm = IoC.Get<LiquidLevelDetectViewModel>();
                        break;
                    }
                case LogicStepEnum.ZBySquid:
                    {
                        vm = IoC.Get<ZLiquidLevelMoveViewModel>();
                        break;
                    }
                case LogicStepEnum.FixedIn:
                    {
                        vm = IoC.Get<FixedAbsorbViewModel>();
                        break;
                    }
                case LogicStepEnum.FixedOut:
                    {
                        vm = IoC.Get<FixedDivideViewModel>();
                        break;
                    }
                case LogicStepEnum.TakeGel:
                case LogicStepEnum.PutDownGel:
                    {
                        vm = IoC.Get<TakeGELViewModel>();
                        break;
                    }
                //case "21":
                //    {
                //        vm = IoC.Get<PutGELViewModel>();
                //        break;
                //    }
                case LogicStepEnum.Piercer:
                    {
                        vm = IoC.Get<PierceViewModel>();
                        break;
                    }
                case LogicStepEnum.Centrifuge:
                    {
                        vm = IoC.Get<CentrifugeViewModel>();
                        break;
                    }
                case LogicStepEnum.GelWarehouse:
                    {
                        vm = IoC.Get<GelWarehouseViewModel>();
                        break;
                    }
                case LogicStepEnum.Couveuse:
                    {
                        vm = IoC.Get<HatcherViewModel>();
                        break;
                    }
                case LogicStepEnum.TakeAndPutDownGel:
                    {
                        vm = IoC.Get<TakeAndPutGelViewModel>();
                        break;
                    }
            }
            if(vm is LogicStepScreen lgvm)
            {
                lgvm.StepEnum = stepEnum;
            }
            return vm;
        }
        #endregion
        private void M_ConfirmEvent(object sender, T_LogicStep Step)
        {
            if (Step.OrderIndex == 0)
            {
                int orderIndex = 1;
                if (StepList.Count > 0)
                {
                    orderIndex = StepList[StepList.Count - 1].OrderIndex + 1;
                }
                Step.OrderIndex = orderIndex;
                StepList.Add(Step);
            }
        }
        public void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            foreach (var item in e.AddedItems)
            {
                SelectedStep.Add((T_LogicStep)item);

            }
            foreach (var item in e.RemovedItems)
            {
                SelectedStep.Remove((T_LogicStep)item);
            }
        }
        public void Edit()
        {
            if (SelectedStep == null&& SelectedStep.Count!=1) return;
            var vm = GetViewModel(SelectedStep[0].StepID.ToString());
            if (vm == null) return;
            vm.ParseLogicStep(SelectedStep[0]);
            windowManager.ShowDialog(vm);
        }
        public void Copy()
        {
            if (SelectedStep == null) return;
            List<T_LogicStep> selsteps = new List<T_LogicStep>(SelectedStep.ToArray());
            foreach (var selstep in selsteps)
            {
                var NewStep = TransExpUtil<T_LogicStep, T_LogicStep>.Trans(selstep);
                NewStep.ID = 0;
                int orderIndex = 1;
                if (StepList.Count > 0)
                {
                    orderIndex = StepList[StepList.Count - 1].OrderIndex + 1;
                }
                NewStep.OrderIndex = orderIndex;
                StepList.Add(NewStep);
            }
        }
        public void Delete()
        {
            if (SelectedStep == null) return;
            var diagle = windowManager.ShowMessageBox(String.Format("确定要删除"), "系统提示", MessageBoxButton.YesNo);
            if (diagle == MessageBoxResult.Yes)
            {
                List<T_LogicStep> selsteps = new List<T_LogicStep>(SelectedStep.ToArray());
                foreach (var selstep in selsteps)
                {
                    if (this.logicService.DeleteT_LogicStep(selstep.ID))
                    {
                        var CurrentIndex = StepList.IndexOf(selstep);
                        for (int i = CurrentIndex; i < StepList.Count; i++)
                        {
                            StepList[i].OrderIndex = StepList[i].OrderIndex - 1;
                        }
                        StepList.Remove(selstep);
                        Program.LogicSteps = StepList.ToList();
                        SaveEvent?.Invoke(Program);
                    }
                    else
                    {
                        this.View.ShowHint(new MessageWin(false));
                    }
                }
            }
        }
        public void MoveUp()
        {
            if (SelectedStep == null) return;
            List<T_LogicStep> selsteps = new List<T_LogicStep>(SelectedStep.ToArray());
            foreach (var selstep in selsteps)
            {
                var CurrentIndex = StepList.IndexOf(selstep);
                if (CurrentIndex == 0) return;
                var sec = StepList[CurrentIndex - 1];
                sec.OrderIndex = sec.OrderIndex + 1;
                selstep.OrderIndex = selstep.OrderIndex - 1;
                StepList.Remove(sec);
                StepList.Insert(CurrentIndex, sec);
            }
        }
        public void MoveDown()
        {
            if (SelectedStep == null) return;
            List<T_LogicStep> selsteps = new List<T_LogicStep>(SelectedStep.ToArray());
            selsteps.Reverse();
            foreach (var selstep in selsteps)
            {
                var CurrentIndex = StepList.IndexOf(selstep);
                if (CurrentIndex == StepList.Count - 1) return;
                var sec = StepList[CurrentIndex + 1];
                sec.OrderIndex = sec.OrderIndex - 1;
                selstep.OrderIndex = selstep.OrderIndex + 1;
                StepList.Remove(sec);
                StepList.Insert(CurrentIndex, sec);
            }

        }
        public void Confirm()
        {
            if (String.IsNullOrEmpty(Program.Name))
            {
                this.View.ShowHint(new MessageWin("程序名称不能为空"));
                return;
            }
            Program.LogicSteps = StepList.ToList();
            if (logicService.SaveT_LogicTest(Program))
            {
                SaveEvent?.Invoke(Program);
                this.View.ShowHint(new MessageWin());
            }
            else
            {
                this.View.ShowHint(new MessageWin("保存失败！"));
            }
        }
        public void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var Row = DataGridUtil.GetRowMouseDoubleClick(sender, e);
            if (Row == null) return;
            Edit();
        }
    }
}
