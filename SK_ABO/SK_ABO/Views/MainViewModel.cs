using SK_ABO.Pages;
using SKABO.BLL.IServices.ITrace;
using SKABO.Common;
using SKABO.Common.Enums;
using SKABO.Common.Models.BJ;
using Stylet;
using System;
using System.Reflection;
using SKABO.Hardware.Core;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Threading;
using SKABO.Common.Models.Communication;
using SKABO.Hardware.RunBJ;
using System.Linq;
using SKABO.ActionEngine;

namespace SK_ABO.Views
{
    public class MainViewModel:Screen
    {
        private IWindowManager windowManager;
        private IViewManager viewManager;
        [StyletIoC.Inject]
        private ITraceService TraceService;
        public delegate void SwitchPageHandle(object sender, EventArgs e);
        public event SwitchPageHandle SwitchPageEvent;
        [StyletIoC.Inject]
        private CouveuseMixerDevice cmixerDevice;
        [StyletIoC.Inject]
        private OtherPartDevice opDevice;
        public MainViewModel(IWindowManager windowManager,IViewManager viewManager)
        {
            this.windowManager = windowManager;
            this.viewManager = viewManager;
            this.DisplayName = "中山生科全自动血库检测系统 SK1200";
        }
        //动作引擎
        public Engine engine = Engine.getInstance();
        System.Windows.Controls.Frame frame0, frame1, frame2, frame3;
        public void SwitchPage(String PageIndex)
        {
            var f=this.GetType().GetField("frame" + PageIndex, BindingFlags.NonPublic | BindingFlags.Instance);
            if (f == null) return;
            var fVal = f.GetValue(this);
            Screen vm = null;
            if (fVal == null)
            {
                switch (PageIndex)
                {
                    case "0":
                        {
                            vm = IoC.Get<HomeViewModel>();
                            break;
                        }
                    case "1":
                        {
                            vm = IoC.Get<QueryViewModel>();
                            break;
                        }
                    case "2":
                        {
                            vm = IoC.Get<SystemViewModel>();
                            break;
                        }
                    case "3":
                        {
                            vm = IoC.Get<RunLogViewModel>();
                            break;
                        }
                }
                var pv = viewManager.CreateAndBindViewForModelIfNecessary(vm);
                
                pv.SetValue(System.Windows.Controls.Page.DataContextProperty, vm);
                fVal = new System.Windows.Controls.Frame();
                f.SetValue(this, fVal);
                (fVal as System.Windows.Controls.Frame).Content = pv;
            }

            if (SwitchPageEvent != null)
            {
                SwitchPageEvent(fVal, new EventArgs());
            }

        }
        public void Window_Loaded()
        {
            MainView v = this.View as MainView;
            this.SwitchPageEvent += v.Vm_SwitchPageEvent;
            SwitchPage("0");
            v.InitStatusBar();
            this.StatusLock = new object();
            ChangeDeviceStatus(null);
            Constants.MainWindow = v;
        }
        private bool? IsOnline;
        private Task ChangeColorTask;
        private Object StatusLock;
        private void ChangeDeviceStatus(bool? IsOnline)
        {
            lock (this.StatusLock)
            {
                if (this.IsOnline == IsOnline) return;
                this.IsOnline = IsOnline;
                if (IsOnline.HasValue)
                {
                    this.DeviceStatus = IsOnline.Value ? "设备在线" : "设备连线中...";
                    if (!IsOnline.Value )
                    {
                        if (ChangeColorTask == null)
                        {
                            ChangeColorTask = new Task(() =>
                            {
                                while (this.IsOnline.HasValue && !this.IsOnline.Value)
                                {
                                    Thread.Sleep(1000);
                                    this.DeviceColor = this.DeviceColor == Brushes.Red ? Brushes.Green : Brushes.Red;
                                }
                            });
                            ChangeColorTask.Start();
                        }
                    }
                    else
                    {
                        try
                        {
                            if (ChangeColorTask != null)
                            {
                                ChangeColorTask.Wait();
                                ChangeColorTask.Dispose();
                            }
                            this.DeviceColor = Brushes.Green;
                            
                        }
                        catch { }
                        ChangeColorTask = null;
                        
                        
                    }
                }
                else
                {
                    this.DeviceStatus = "设备离线";
                    this.DeviceColor = Brushes.Red;

                }
            }
        }
        protected override void OnClose()
        {
            //comm.StopScan();
            //pcbComm.StopScan();
            engine.exit();
            if(engine.work_thread!=null)
            engine.work_thread.Join();
            CanComm.OnError -= Comm_OnError;
            CanComm.Close();
            if (System.Windows.Application.Current.ShutdownMode == System.Windows.ShutdownMode.OnMainWindowClose)
            {

                TraceService.InsertT_Trace(TraceLevelEnum.LowImportant, "关闭主程序");
            }
            else
            {
                TraceService.InsertT_Trace(TraceLevelEnum.LowImportant, "注销");
            }

            foreach(var key in Constants.BJDict.Keys)
            {
                switch (key)
                {
                    case "T_BJ_DeepPlate":
                        {
                            VBJ.SaveConfig(Constants.BJDict[key]);
                            break;
                        }
                }
            }

            base.OnClose();
            System.Environment.Exit(0);
        }
        
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            Console.WriteLine("开启通讯线程");
            StartCommScan();
        }
        public Brush DeviceColor
        {
            get;set;
        }
        public String DeviceStatus { get; set; }
        [StyletIoC.Inject(Key ="PLC")]
        AbstractComm comm;
        [StyletIoC.Inject]
        AbstractCanComm CanComm;

        private void StartCommScan()
        {
            /*
            comm.OnError += Comm_OnError;
            comm.WatchEventCoil.Add(cmixerDevice.CouMixer.Mixer.EixstCoil.Addr, true);
            comm.OnCoilSwitchEvent += Comm_OnCoilSwitchEvent;

            if (pcbComm is PcbComm pcb)
            {
                pcb.OnChangeSampleRackStatus += Pcb_OnChangeSampleRackStatus;
            }
            comm.StartScan();
            pcbComm.StartScan();*/
            CanComm.OnError+= Comm_OnError;
            CanComm.Connect();
            //opDevice.StartScan();

        }
        /// <summary>
        /// 用于记录样本载架脱离事件，最多记录50次
        /// </summary>
        /// <param name="indexs"></param>
        /// <param name="eventType"></param>
        private void Pcb_OnChangeSampleRackStatus(byte[] indexs, byte eventType)
        {
            Byte index = indexs[0];
            if (index > 7)//6路感应
            {
            }
            else//5路感应 实际顺序与index顺序相反 index==0时，实际对应的是第5个
            {
                if (eventType == 0)
                {
                    var now = DateTime.Now;
                    foreach (var b in indexs)
                    {
                        Constants.AddTakeOutRackRecord(((byte)(Constants.SampleRackCount-b), now));
                    }
                }
            }
        }

        private void Comm_OnError(bool IsOnline, string errorMsg)
        {
                ChangeDeviceStatus(IsOnline);
                Console.WriteLine("更新通讯信息" + errorMsg);
        }

        public void StopApp()
        {
            CanComm.OnError -= Comm_OnError;
            CanComm.Close();
            comm.WatchEventCoil.Clear();
            comm.OnCoilSwitchEvent -= Comm_OnCoilSwitchEvent;
            opDevice.OnChangeSampleRackStatus -= Pcb_OnChangeSampleRackStatus;
            comm.StopScan();
            opDevice.StopScan();
        }
        private void Comm_OnCoilSwitchEvent(System.Collections.Generic.KeyValuePair<string, bool>[] kvs)
        {
            //return;
            //foreach(var kv in kvs)
            //{
            //    if (kv.Key== cmixerDevice.CouMixer.Mixer.EixstCoil.Addr)
            //    {
            //        if (!kv.Value)
            //            opDevice.SetColor_G(5);
            //        else
            //            opDevice.SetColor_R(5);
            //    }
            //}
            
        }
    }
}
