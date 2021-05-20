using SKABO.Common;
using SKABO.Common.Models.BJ;
using SKABO.Hardware.RunBJ;
using Stylet;
using System;
using System.Collections.Generic;
using SKABO.Common.Utils;
using SK_ABO.Views;
using System.Linq;
using System.Threading.Tasks;
using SKABO.ActionEngine;
using SKABO.ResourcesManager;

namespace SK_ABO.Pages.Device
{
    public class CentrifugeViewModel:Screen
    {
        public CentrifugeViewModel()
        {
            /*
            this.BtnText[0] = "初始化";
            this.BtnText[1] = "全部初始化";
            this.BtnText[2] = "低速";
            this.BtnText[3] = "高速";
            this.BtnText[4] = "停止";
            */
            this.BtnText = new Stylet.BindableCollection<String>(new String[] {"初始化","全部初始化","低速","高速","停止" });
        }
        [StyletIoC.Inject]
        private CentrifugeDevice centDevice;
        [StyletIoC.Inject]
        private CentrifugeMrg cenMrg;
        [StyletIoC.Inject]
        private MachineHandDevice handDevice;
        [StyletIoC.Inject]
        private OtherPartDevice opDevice;

        private T_BJ_Centrifuge _SelectedItem;
        public T_BJ_Centrifuge SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
                _SelectedItem = value;
                CanCloseDoor = CanInitCentrifuge = CanOpenDoor = CanMoveToIndex=CanMoveAngel= CanExecuteMove= 
                    CanStartLow=CanStartWork=value != null;
                CanStartHigh = CanStartStop = false;
            }
        }
        public IList<VBJ> CentrifugeList
        {
            get
            {
                if (Constants.BJDict.ContainsKey("T_BJ_Centrifuge"))
                {
                    var res = Constants.BJDict["T_BJ_Centrifuge"].Where(item => (item as T_BJ_Centrifuge).Status == 1);
                    if (res == null) return null;
                    return res.ToList();
                }
                else
                {
                    return null;
                }
            }
        }
        public IObservableCollection<String> BtnText { get; set; } 
        public bool CanOpenDoor { get; set; }
        public bool CanCloseDoor { get; set; }
        public bool CanInitCentrifuge { get; set; }
        public bool CanMoveToIndex { get; set; }
        public bool CanMoveAngel { get; set; }
        public bool CanExecuteMove { get; set; }
        public bool CanStartLow { get; set; }

        public bool CanStartHigh { get; set; }
        
        public bool CanStartStop { get; set; }
        public bool CanStartWork { get; set; }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            
        }
        /// <summary>
        /// 初始化选中离心机
        /// </summary>
        public void InitCentrifuge()
        {
            var cen = cenMrg.GetCentrifugeByCode(SelectedItem.Code);
            var act = InitXyz.create(cen,30000, false, false, true);
            act.runAction(cen);
        }
        /// <summary>
        /// 初始化所有离心机
        /// </summary>
        public void InitCentrifugeAll()
        {
            foreach (var cent in cenMrg.CentrifugeMDevices)
            {
                var act = InitXyz.create(30000, false, false, true);
                act.runAction(cent);
            }
        }
        public void STAShowHint(bool res)
        {
            this.View.Dispatcher.Invoke(() =>
            {
                this.View.ShowHint(new MessageWin(res));
            });
        }
        /// <summary>
        /// 按角度旋转
        /// </summary>
        public void MoveAngel()
        {
            var act = MoveTo.create(30000,-1,-1,(int)AngleValue,5);
            act.runAction(cenMrg.CentrifugeMDevices[0]);
        }
        private void StartWithAngel(float angel)
        {
            //if (SelectedItem == null) return;
            //var res = centDevice.MoveAngel(SelectedItem.Code, angel);
            //this.View.ShowHint(new MessageWin(res ? "Successfully" : "Failed"));
        }
        public void CameraLight()
        {
            var is_light = opDevice.GetCameraLight();
            opDevice.CameraLight(!is_light);
        }
        /// <summary>
        /// 移动卡位
        /// </summary>
        /// <param name="index"></param>
        public void MoveToIndex(Double seatIndex)
        {
            if (SelectedItem == null) return;
            var cen = cenMrg.GetCentrifugeByCode(SelectedItem.Code);
            var resinfo = ResManager.getInstance().SearchGelCard("T_BJ_Centrifuge", SelectedItem.Code,"", (int)seatIndex);
            var act = MoveTo.create(cen,30000, -1, -1, resinfo.CenGelP[resinfo.CountX],5);
            act.runAction(cen);
        }
        
        public void StartLow()
        {

        }
        public void StartHigh()
        {

        }
        public void StartStop()
        {

        }
        public void StartWork()
        {
            var cen = cenMrg.GetCentrifugeByCode(SelectedItem.Code);
            if (cen!=null)
            {
                int hspeed = (int)cen.Centrifugem.HightSpeed.SetValue;
                int lspeed = (int)cen.Centrifugem.LowSpeed.SetValue;
                int htime = (int)cen.Centrifugem.HightSpeedTime.SetValue;
                int ltime = (int)cen.Centrifugem.LowSpeedTime.SetValue;
                int uphtime = (int)cen.Centrifugem.AddHSpeedTime.SetValue;
                int upltime = (int)cen.Centrifugem.AddLSpeedTime.SetValue;
                int stime = (int)cen.Centrifugem.StopSpeedTime.SetValue;
                var act = CentrifugeStart.create(cen,30000000, hspeed, lspeed, htime, ltime, uphtime, upltime, stime);
                act.runAction(cen);

            }
        }

        private void Cent_ChangeStatusEvent(SKABO.Common.Models.Communication.Unit.Centrifuge centrifuge, SKABO.Common.Models.CentrifugeStatusChangeEventArg e)
        {
            this.RunMsg = $"{e.StatusEnum.GetDescription()} {e.Time} 秒";
        }

        /// <summary>
        /// 点动步长
        /// </summary>
        public double StepValue { get; set; }
        public double AngleValue { get; set; }
        public void ExecuteMove(String flag)
        {
            if (flag == "+")
            {
                AngleValue += StepValue;
            }else if (flag == "-")
            {
                AngleValue -= StepValue;
                AngleValue = Math.Max(AngleValue, 0);
            }
            MoveAngel();
        }
        /// <summary>
        /// 开舱门
        /// </summary>
        public void OpenDoor()
        {
            if (SelectedItem == null) return;
            var act = HandOpenCloseDoor.create(handDevice, 3000, SelectedItem.Code, true);
            act.runAction(handDevice);
        }
        /// <summary>
        /// 关舱门
        /// </summary>
        public void CloseDoor()
        {
            if (SelectedItem == null) return;
            var act = HandOpenCloseDoor.create(handDevice,3000,SelectedItem.Code,false);
            act.runAction(handDevice);
        }
        public String RunMsg { get; set; }
    }
}
