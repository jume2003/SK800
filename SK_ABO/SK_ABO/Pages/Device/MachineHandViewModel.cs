using SKABO.Hardware.RunBJ;
using Stylet;
using System;
using SKABO.Common.Utils;
using SK_ABO.Views;
using System.Collections.Generic;
using SKABO.Common.Models.BJ;
using SKABO.Common;
using System.Linq;
using SKABO.ActionEngine;
using SKABO.ActionGeneraterEngine;
using SKABO.Common.Models.GEL;
using SKABO.ResourcesManager;
using System.Windows.Controls;

namespace SK_ABO.Pages.Device
{
    public class MachineHandViewModel:Screen
    {
        [StyletIoC.Inject]
        private IWindowManager windowManager;
        private bool loaded;
        [StyletIoC.Inject]
        private MachineHandDevice handDevice;
        [StyletIoC.Inject]
        private InjectorDevice injectorDevice;
        [StyletIoC.Inject]
        private OtherPartDevice opDevice;
        public bool AutoSetZeroForZ { get; set; }

        protected override void OnViewLoaded()
        {
            if (loaded)
            {
                return;
            }
            base.OnViewLoaded();
            this.AutoSetZeroForZ = true;
            loaded = true;
            this.StepXValue = this.StepYValue = this.StepZValue = 1;
        }
        public Stylet.BindableCollection<VBJ> _TargetBJList;
        public Stylet.BindableCollection<VBJ> TargetBJList
        {
            get
            {
                if (_TargetBJList == null) _TargetBJList = new Stylet.BindableCollection<VBJ>();
                UpdataBjList();
                return _TargetBJList;
            }
        }

        public void UpdataBjList()
        {
            _TargetBJList.Clear();
            foreach (var bj in Constants.BJDict[typeof(T_BJ_GelWarehouse).Name])
            {
                _TargetBJList.Add(bj);
            }
            foreach (var bj in Constants.BJDict[typeof(T_BJ_GelSeat).Name])
            {
                _TargetBJList.Add(bj);
            }
            foreach (var bj in Constants.BJDict[typeof(T_BJ_Scaner).Name].Where(item => (item as T_BJ_Scaner).Purpose == 1))
            {
                _TargetBJList.Add(bj);
            }
            foreach (var bj in Constants.BJDict[typeof(T_BJ_Camera).Name])
            {
                _TargetBJList.Add(bj);
            }
            foreach (var bj in Constants.BJDict[typeof(T_BJ_Centrifuge).Name].Where(item => (item as T_BJ_Centrifuge).Status == 1))
            {
                _TargetBJList.Add(bj);
            }
            foreach (var bj in Constants.BJDict[typeof(T_BJ_WastedSeat).Name].Where(item => (item as T_BJ_WastedSeat).HandZ > 0))
            {
                _TargetBJList.Add(bj);
            }
        }

        public VBJ SelectedBJ { get; set; }
        public decimal DistanceX { get; set; }
        public decimal StepXValue { get; set; }
        public decimal DistanceY { get; set; }
        public decimal StepYValue { get; set; }
        public decimal DistanceZ { get; set; }
        public decimal StepZValue { get; set; }
        public decimal MaxSelect { get; set; }
        public byte SeatIndex { get; set; }
        public void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectedBJ == null) return;
            if (SelectedBJ is T_BJ_GelSeat gel_seat)
            {
                MaxSelect = gel_seat.Count-1;
            }
            else if (SelectedBJ is T_BJ_Centrifuge cen_seat)
            {
                MaxSelect = cen_seat.GetUnUsedCount()-1;
            }
            else if (SelectedBJ is T_BJ_GelWarehouse ware_seat)
            {
                MaxSelect = ware_seat.Count-1;
            }
            if (SeatIndex >= MaxSelect) SeatIndex =(byte)(MaxSelect);
        }
        public void InitX()
        {
            var dia = this.windowManager.ShowMessageBox("请再一次确认安全！", "安全警告", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Exclamation);
            if (dia == System.Windows.MessageBoxResult.OK)
            {
                var act = Sequence.create(MoveTo.create(30000, -1, -1, 0), InitXyz.create(30000, true, false, false));
                act.runAction(handDevice);
            }
        }
        public void InitY()
        {
            var dia = this.windowManager.ShowMessageBox("请再一次确认安全！", "安全警告", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Exclamation);
            if (dia == System.Windows.MessageBoxResult.OK)
            {
                var act = Sequence.create(MoveTo.create(30000, -1, -1, 0), InitXyz.create(30000, false, true, false));
                act.runAction(handDevice);
            }
        }
        public void InitZ()
        {
            var dia = this.windowManager.ShowMessageBox("请再一次确认安全！", "安全警告", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Exclamation);
            if (dia == System.Windows.MessageBoxResult.OK)
            {
                var act = Sequence.create(InitXyz.create(30000, false, false, true));
                act.runAction(handDevice);
            }
        }


        public void MoveX()
        {
            var act = Sequence.create(MoveTo.create(3000, -1, -1, 0), MoveTo.create(3000, (int)DistanceX,-1 ));
            act.runAction(handDevice);
        }

        public void MoveY()
        {
            var act = Sequence.create(MoveTo.create(3000, -1, -1, 0), MoveTo.create(3000,-1, (int)DistanceY));
            act.runAction(handDevice);
        }

        public void MoveZ()
        {
            var act = Sequence.create(MoveTo.create(3000, -1, -1, (int)DistanceZ));
            act.runAction(handDevice);
        }
        public void InitHand()
        {
            var res = this.handDevice.InitHandTongs();
            this.View.ShowHint(new MessageWin(res));
        }
        public void CheckGel()
        {
            var res = this.handDevice.CheckGel();

            String msg = "";
            if (res == null)
            {
                msg = "通讯失败";
            }
            else
            {
                msg = res ?  "有卡": "无卡";
            }
            this.View.ShowHint(new MessageWin(msg));
        }
        public void SwitchHand(String IsOpen)
        {
            var res = this.handDevice.SwitchHand(IsOpen == "1");
            this.View.ShowHint(new MessageWin(res));
        }

        public void TakeGel()
        {
            if (this.SelectedBJ == null) return;
            var resmanager = ResManager.getInstance();
            var generater = ActionGenerater.getInstance();
            var seque = Sequence.create();
            seque.AddAction(MoveTo.create(handDevice, 3000, -1, -1, 0));
            if (SelectedBJ is T_BJ_GelSeat gel_seat)
            {
                var take_seat = resmanager.SearchGelCard("T_BJ_GelSeat", gel_seat.Code,"",SeatIndex);
                generater.GenerateTakeGelFromNormal(take_seat,ref seque);
            }
            else if (SelectedBJ is T_BJ_Centrifuge cen_seat)
            {
                var take_seat = resmanager.SearchGelCard("T_BJ_Centrifuge", cen_seat.Code, "", SeatIndex);
                var act = generater.GenerateTakeGelFromCent(take_seat, cen_seat.Code, ref seque);
            }
            else if (SelectedBJ is T_BJ_GelWarehouse ware_seat)
            {
                var take_seat = resmanager.SearchGelCard("T_BJ_GelWarehouse", ware_seat._Code, "", SeatIndex);
                var act = generater.GenerateTakeGelFromWare(take_seat,ref seque);
            }
            seque.runAction(handDevice);
        }
        public void PutDownGel()
        {
            if (this.SelectedBJ == null) return;
            var resmanager = ResManager.getInstance();
            var generater = ActionGenerater.getInstance();
            var seque = Sequence.create();
            seque.AddAction(MoveTo.create(3000, -1, -1, 0));
            var put_gel = resmanager.handseat_resinfo;
            if (SelectedBJ is T_BJ_GelSeat gel_seat)
            {
                var put_seat = resmanager.SearchGelCard("T_BJ_GelSeat", gel_seat.Code, "", SeatIndex);
                generater.GeneratePutGelToNormal(put_seat, put_gel, ref seque);
            }
            else if (SelectedBJ is T_BJ_Centrifuge cen_seat)
            {
                var put_seat = resmanager.SearchGelCard("T_BJ_Centrifuge", cen_seat.Code, "", SeatIndex);
                generater.GeneratePutGelToCent(cen_seat.Code,put_seat, put_gel, ref seque);
            }
            else if (SelectedBJ is T_BJ_GelWarehouse ware_seat)
            {
                var put_seat = resmanager.SearchGelCard("T_BJ_GelWarehouse", ware_seat.Code, "", SeatIndex);
                generater.GeneratePutGelToWare(put_seat, put_gel, ref seque);
            }
            else if (SelectedBJ is T_BJ_WastedSeat wasted_seat)
            {
                var put_seat = resmanager.SearchGelCard("T_BJ_WastedSeat", "", "1", SeatIndex);
                generater.GeneratePutGelToNormal(put_seat, put_gel, ref seque);
            }
            seque.runAction(handDevice);
        }
        public void InitAll()
        {
            var dia = this.windowManager.ShowMessageBox("请再一次确认安全！", "安全警告", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Exclamation);
            if (dia == System.Windows.MessageBoxResult.OK)
            {
                var act = Sequence.create(InitXyz.create(30000, false, false, true));
                act.AddAction(InitXyz.create(30000, true, true, false));
                act.runAction(handDevice);
                InitHand();
            }
        }
        public void MoveXY()
        {
            if (this.SelectedBJ == null) return;
            var resmanager = ResManager.getInstance();
            var seque = Sequence.create();
            seque.AddAction(MoveTo.create(30000, -1, -1, 0));
            if (SelectedBJ is T_BJ_GelSeat gel_seat)
            {
                var move_seat = resmanager.SearchGelCard("T_BJ_GelSeat", gel_seat.Code, "", SeatIndex);
                var act = MoveTo.create(30000, (int)move_seat.X, (int)(move_seat.Y), -1);
                seque.AddAction(act);
            }
            else if (SelectedBJ is T_BJ_Centrifuge cen_seat)
            {
                var move_seat = resmanager.SearchGelCard("T_BJ_Centrifuge", cen_seat.Code, "", SeatIndex);
                var act = MoveTo.create(30000, (int)move_seat.X, (int)(move_seat.CenHandYP[move_seat.CountX]), -1);
                seque.AddAction(act);
            }
            else if (SelectedBJ is T_BJ_GelWarehouse ware_seat)
            {
                var move_seat = resmanager.SearchGelCard("T_BJ_GelWarehouse", ware_seat.Code, "", SeatIndex);
                var act = MoveTo.create(30000, (int)move_seat.X, (int)(move_seat.Y), -1);
                seque.AddAction(act);
            }
            else if (SelectedBJ is T_BJ_Scaner scaner_seat)
            {
                var move_seat = resmanager.SearchGelCard("T_BJ_Scaner", "", "1", SeatIndex);
                var act = MoveTo.create(30000, (int)move_seat.X, (int)(move_seat.Y), -1);
                seque.AddAction(act);
            }
            else if (SelectedBJ is T_BJ_Camera camera_seat)
            {
                var move_seat = resmanager.SearchGelCard("T_BJ_Camera", "", "1", SeatIndex);
                var act = MoveTo.create(30000, (int)move_seat.X, (int)(move_seat.Y), -1);
                seque.AddAction(act);
            }
            else if (SelectedBJ is T_BJ_WastedSeat wasted_seat)
            {
                var move_seat = resmanager.SearchGelCard("T_BJ_WastedSeat", "", "1", SeatIndex);
                var act = MoveTo.create(30000, (int)move_seat.X, (int)(move_seat.Y), -1);
                seque.AddAction(act);
            }
            seque.runAction(handDevice);

        }
    }
}
