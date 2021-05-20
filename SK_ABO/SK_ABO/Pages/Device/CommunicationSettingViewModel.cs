using SKABO.BLL.IServices.IDevice;
using SKABO.Common.Models.Communication;
using SKABO.Hardware.Core;
using SKABO.Hardware.RunBJ;
using Stylet;
using System;
using System.Windows.Controls;
using SKABO.Common;
using System.Windows.Input;
using System.Windows;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Common.Utils;
using SK_ABO.Views;
using System.Collections.Generic;

namespace SK_ABO.Pages.Device
{
    /// <summary>
    /// 通讯设置
    /// </summary>
    public class CommunicationSettingViewModel:Screen
    {
        [StyletIoC.Inject]
        private IPlcBjParamService bjParamService;
        [StyletIoC.Inject]
        private AbstractCanComm CanComm;
        private OtherPartDevice OPDevice;
        private CouveuseMixerDevice CouMixerDevice;
        private PiercerDevice PieDevice;
        private CentrifugeDevice CentDevice;
        private MachineHandDevice HandDevice;
        private CentrifugeMrg CentMrg;


        private bool loaded = false;
        public CommunicationSettingViewModel():base()
        {
            
        }
        protected override void OnViewLoaded()
        {
            if (loaded)
            {
                return;
            }
            base.OnViewLoaded();
            this.Piercer = bjParamService.LoadFromJson< Piercer>();
            this.Piercer = this.Piercer ?? new Piercer();
            this.PieDevice = new PiercerDevice(CanComm, this.Piercer);

            this.CentSys = bjParamService.LoadFromJson<CentrifugeSystem>();
            this.CentSys = this.CentSys ?? new CentrifugeSystem();
            this.CentDevice = new CentrifugeDevice(CanComm, CentSys);

            this.GelWare = bjParamService.LoadFromJson<GelWarehouse>();
            this.GelWare = this.GelWare ?? new GelWarehouse();
            this.GelWareDevice = new GelWarehouseDevice(CanComm,GelWare);

            this.Hand= bjParamService.LoadFromJson<MachineHand>();
            this.Hand = this.Hand ?? new MachineHand();
            this.Hand.CheckNull();
            this.HandDevice = new MachineHandDevice(CanComm, Hand);

            this.CouMixer = bjParamService.LoadFromJson<CouveuseMixer>();
            this.CouMixer = this.CouMixer ?? new CouveuseMixer();
            this.CouMixer.checkNull();
            this.CouMixerDevice = new CouveuseMixerDevice(CanComm, CouMixer);

            this.Param = bjParamService.LoadFromJson<OtherPart>();
            this.Param = this.Param ?? new OtherPart();
            this.OPDevice = new OtherPartDevice(CanComm,Param);

            this.Centrifuge = bjParamService.LoadFromJson<CentrifugeData>();
            this.Centrifuge = this.Centrifuge ?? CentrifugeData.Create();
            this.CentMrg = new CentrifugeMrg(CanComm,Centrifuge);
        }
        public GelWarehouseDevice GelWareDevice { get; set; }
        public GelWarehouse GelWare { get; set; }
        public CentrifugeSystem CentSys { get; set; }
        public OtherPart Param { get; set; }
        public Piercer Piercer { get; set; }
        public MachineHand Hand { get; set; }
        public CouveuseMixer CouMixer { get; set; }
        public CentrifugeData Centrifuge { get; set; }
        /// <summary>
        /// 加载PLC数据
        /// </summary>
        public void LoadParam()
        {
            this.PieDevice.LoadPLCValue();
            this.CentDevice.LoadPLCValue();
            this.HandDevice.LoadPLCValue();
            this.CouMixerDevice.LoadPLCValue();
            this.OPDevice.LoadPLCValue();
            this.GelWareDevice.LoadPLCValue();
            if (this.GelWareDevice.GelWare.TestResults != null)
            {
                var Children = (this.View as CommunicationSettingView).checkBoxPanel.Children;
                for(byte i=0;i< this.GelWareDevice.GelWare.TestResults.Length; i++)
                {
                    (Children[i] as CheckBox).IsChecked = this.GelWareDevice.GelWare[i];
                }
            }
        }
        /// <summary>
        /// 更新设定值到PLC
        /// </summary>
        public void UpdateParam2PLC()
        {
            this.PieDevice.Update2Plc();
            this.CentDevice.Update2Plc();
            this.GelWareDevice.Update2Plc();
            this.HandDevice.Update2Plc();
            this.CouMixerDevice.Update2Plc();
            this.OPDevice.Update2Plc();
            this.CentMrg.Update2Plc();
            this.View.ShowHint(new MessageWin("更新完毕"));
        }
        public void SaveParam()
        {
            if (Keyboard.FocusedElement is UIElement ele)
            {
                ele.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                ele.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
            }
            if (this.Piercer != null)
            {
                bjParamService.SaveAsJson<Piercer>(this.Piercer);
                var newPie = bjParamService.LoadFromJson<Piercer>();
                CopyStatus(IoC.Get<PiercerDevice>().Pie.YMotor, newPie.YMotor);
                CopyStatus(IoC.Get<PiercerDevice>().Pie.ZMotor, newPie.ZMotor);
                IoC.Get<PiercerDevice>().Pie = newPie;
            }
            if (CentSys != null)
            {
                bjParamService.SaveAsJson<CentrifugeSystem>(this.CentSys);
                IoC.Get<CentrifugeDevice>().Centrifuges = bjParamService.LoadFromJson<CentrifugeSystem>().Centrifuges;
                //this.CentSys = bjParamService.LoadFromJson<CentrifugeSystem>();
            }
            if (GelWare != null)
            {
                bjParamService.SaveAsJson<GelWarehouse>(this.GelWare);
                var newGw = bjParamService.LoadFromJson<GelWarehouse>();
                CopyStatus(IoC.Get<GelWarehouseDevice>().GelWare.XMotor, newGw.XMotor);
                IoC.Get<GelWarehouseDevice>().GelWare = newGw;
                //this.GelWare = bjParamService.LoadFromJson<GelWarehouse>();
            }
            if (Hand != null)
            {
                bjParamService.SaveAsJson<MachineHand>(this.Hand);
                var newHand= bjParamService.LoadFromJson<MachineHand>();
                var oldHand = IoC.Get<MachineHandDevice>().Hand;
                newHand.IsOpen = oldHand.IsOpen;
                
                CopyStatus(oldHand.XMotor, newHand.XMotor);
                CopyStatus(oldHand.YMotor, newHand.YMotor);
                CopyStatus(oldHand.ZMotor, newHand.ZMotor);
                IoC.Get<MachineHandDevice>().Hand = newHand;
                oldHand = null;
            }
            if (this.CouMixer != null)
            {
                bjParamService.SaveAsJson<CouveuseMixer>(this.CouMixer);
                IoC.Get<CouveuseMixerDevice>().CouMixer = bjParamService.LoadFromJson<CouveuseMixer>();
            }
            if (this.Param != null)
            {
                bjParamService.SaveAsJson<OtherPart>(this.Param);
                IoC.Get<OtherPartDevice>().OP = bjParamService.LoadFromJson<OtherPart>();
                //this.Param = bjParamService.LoadFromJson<OtherPart>();
            }
            if(this.CentMrg!=null)
            {
                bjParamService.SaveAsJson<CentrifugeData>(this.CentMrg.GetCentrifugeDatas());
                IoC.Get<CentrifugeMrg>().SetCentrifugeDatas(bjParamService.LoadFromJson<CentrifugeData>());
            }
            this.View.ShowHint(new MessageWin("保存成功"));
        }
        private void CopyStatus(Electromotor sorce, Electromotor target)
        {
            target.CurrentDistance = sorce.CurrentDistance;
            target.IsBack = sorce.IsBack;
            target.IsStarted = sorce.IsStarted;
        }
        public void GuangSan_ValueChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var num = (Int32)(sender as SKABO.Common.UserCtrls.NumericUpDown_Control).Value;
            var panel = (this.View as CommunicationSettingView).checkBoxPanel;
            var list=panel.Children;
            if (list.Count < num)
            {
                for(int i= list.Count; i < num; i++)
                {
                    CheckBox cb = new CheckBox() {
                        Content = (i + 1) + "#",
                        IsEnabled = false,
                        Height = 25,
                        Width=50,
                        Margin = new System.Windows.Thickness(2, 5, 5, 5)
                    };
                    list.Add(cb);
                }
            }else if (list.Count > num)
            {
                for (int i = num; i < list.Count; i++)
                {
                    list.RemoveAt(i);
                }
            }
        }
    }
}
