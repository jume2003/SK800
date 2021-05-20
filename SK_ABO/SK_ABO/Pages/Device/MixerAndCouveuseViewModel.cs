using SKABO.Common;
using SKABO.Common.Enums;
using SKABO.Common.Models.BJ;
using SKABO.Hardware.RunBJ;
using Stylet;
using System;
using System.Collections.Generic;
using SKABO.Common.Utils;
using SK_ABO.Views;
using SKABO.Hardware.Core;
using SKABO.ActionEngine;

namespace SK_ABO.Pages.Device
{
    
    public class MixerAndCouveuseViewModel:Screen
    {
        [StyletIoC.Inject]
        private CouveuseMixerDevice couveuseMixerDevice;
        [StyletIoC.Inject]
        private OtherPartDevice otherPartDevice;

        public decimal DistanceZ { get; set; }
        private T_BJ_GelSeat __SelectedItem;
        public T_BJ_GelSeat SelectedItem
        {
            get { return __SelectedItem; }
            set
            {
                __SelectedItem = value;
                CanQueryTemp = CanStartHot = CanStopHot = value != null;
            }
        }
        private bool loaded;
        public int ledcolor = 0;
        public bool [] leds { get; set; } = new bool[7];
        public MixerAndCouveuseViewModel()
        {
            for(int i=0;i<7;i++)
            {
                leds[i] = new bool();
            }
        }
        protected override void OnViewLoaded()
        {
            if (loaded)
            {
                return;
            }
            loaded = true;
            base.OnViewLoaded();
            this.InitTemp = 37.0f;
        }
        public IList<VBJ> _HatchList;
        /// <summary>
        /// 孵育器列
        /// </summary>
        public IList<VBJ> HatchList
        {
            get
            {
                if(_HatchList == null && Constants.BJDict.ContainsKey("T_BJ_GelSeat"))
                {
                    _HatchList = new List<VBJ>();
                    foreach(var item in Constants.BJDict["T_BJ_GelSeat"])
                    {
                        if(item is T_BJ_GelSeat gelSeat && gelSeat.Purpose == (int)GelSeatPurposeEnum.孵育位)
                        {
                            _HatchList.Add(item);
                        }
                    }
                }
                return _HatchList;
            }

        }
        #region 混匀器
        public void InitMixer()
        {
            var act = InitXyz.create(10000, false, false, true);
            act.runAction(couveuseMixerDevice);
        }
        public void StopMixer()
        {
            var run_act = Sequence.create();
            var mixer_act = ActionManager.getInstance().getAllActions(couveuseMixerDevice);
            if (mixer_act.Count != 0)
            {
                run_act.AddAction(SkCallBackFun.create((ActionBase act_tem) => {
                    if (mixer_act.Count != 0)mixer_act[0].stop();
                    return true;
                }));
                run_act.AddAction(InitXyz.create(10000, false, false, true));
                run_act.runAction(couveuseMixerDevice);
            }
        }
        public void StartMixer()
        {
            var reseque = Sequence.create();
            reseque.AddAction(MoveTo.create(couveuseMixerDevice, 10000, -1, -1, 2000));
            reseque.AddAction(SKSleep.create(500));
            reseque.AddAction(MoveTo.create(couveuseMixerDevice, 10000, -1, -1, 0));
            reseque.AddAction(SKSleep.create(15000));
            var rep_act = Repeat.create(couveuseMixerDevice, reseque, 99999);
            rep_act.runAction(couveuseMixerDevice);
        }
        public void MoveZ()
        {
            var action = MoveTo.create(couveuseMixerDevice, 10000, -1, -1,(int)DistanceZ);
            action.runAction(couveuseMixerDevice);
        }
        #endregion 混匀器
        #region 孵育器
        public bool CanStartHot { get; set; }
        public bool CanStopHot { get; set; }
        public bool CanQueryTemp { get; set; }
        /// <summary>
        /// 初始化设定温度
        /// </summary>
        public float InitTemp { get; set; }
        /// <summary>
        /// 当前温度
        /// </summary>
        public String CurrentTemp { get; set; }
        public void StartHot()
        {
            if (SelectedItem != null)
            {
                byte index = (Byte)this.HatchList.IndexOf(SelectedItem);
                var res = this.couveuseMixerDevice.SetTemp(index, InitTemp);
                res= res && this.couveuseMixerDevice.StartHot(index);

                this.View.ShowHint(new MessageWin(res));
            }
        }
        public void StopHot()
        {
            if (SelectedItem != null)
            {
                byte index = (Byte)this.HatchList.IndexOf(SelectedItem);
                var res = this.couveuseMixerDevice.StopHot(index);
                this.View.ShowHint(new MessageWin(res));
            }
        }
        public void QueryTemp()
        {
            if (SelectedItem != null)
            {
                byte index = (Byte)this.HatchList.IndexOf(SelectedItem);
                CurrentTemp = this.couveuseMixerDevice.ReadTemp(index).ToString();
            }
        }
        #endregion 孵育器
        public void MakeColor(int color)
        {
            for (int i = 0; i < 7; i++)
            {
                if (leds[i])
                {
                    int mask1 = color << i * 2;
                    int mask2 = (0x03 << i * 2) ^ 0xffff;
                    int mask3 = mask1 | mask2;
                    int mask4 = (0x03 << i * 2);
                    ledcolor = ledcolor | mask4;
                    ledcolor = ledcolor & mask3;
                }
            }
        }
        //灯光光
        public void LedGreen()
        {
            var act_light = SKLight.create(3000, SKLight.LightEnum.Green);
            act_light.runAction(otherPartDevice);
        }
        public void LedBule()
        {
            var act_light = SKLight.create(3000, SKLight.LightEnum.Green);
            act_light.runAction(otherPartDevice);
        }
        public void LedRed()
        {
            var act_light = SKLight.create(3000, SKLight.LightEnum.Red);
            act_light.runAction(otherPartDevice);
        }
        public void LedRedBlink()
        {
            var act_light = SKLight.create(3000, SKLight.LightEnum.RedBlink);
            act_light.runAction(otherPartDevice);
        }
    }
}
