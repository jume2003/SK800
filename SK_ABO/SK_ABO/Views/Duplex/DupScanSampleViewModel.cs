using SK_ABO.MAI.HisSystem;
using SKABO.BLL.IServices.IUser;
using SKABO.Common;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.Duplex;
using SKABO.Hardware.Core;
using SKABO.Hardware.RunBJ;
using SKABO.Hardware.Scaner;
using SKABO.Ihardware.Core;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using SKABO.Common.Utils;
using SKABO.Common.Models.GEL;
using SKABO.BLL.IServices.IGel;
using SKABO.Hardware.Model;
using SK_ABO.robot;
using System.Threading;
using SKABO.Common.Views;

namespace SK_ABO.Views.Duplex
{
    public class DupScanSampleViewModel : Screen
    {
        [StyletIoC.Inject]
        private OtherPartDevice op;
        [StyletIoC.Inject]
        private ScanDevice scanDevice;
        public Stylet.BindableCollection<SampleInfo> Samples { get; set; }
        [StyletIoC.Inject]
        private IUserService userService;
        public static bool is_starttimer= false;

        private static System.Windows.Threading.DispatcherTimer readDataTimer = new System.Windows.Threading.DispatcherTimer();

 
        public void timeCycle(object sender, EventArgs e)
        {
            HisSystemWork();
        }
        protected override void OnViewLoaded()
        {
            Samples = new BindableCollection<SampleInfo>();

            OpenReaderRack();

            if(is_starttimer==false)
            {
                is_starttimer = true;
                readDataTimer.Tick += new EventHandler(timeCycle);
                readDataTimer.Interval = new TimeSpan(0, 0, 0, 1);
                readDataTimer.Start();
            }

            base.OnViewLoaded();
        }
        protected override void OnClose()
        {
            this.CloseReaderRack();
            base.OnClose();
        }

        private AbstractScaner _Scaner;
        private AbstractScaner Scaner
        {
            get
            {
                if (_Scaner == null) _Scaner = scanDevice.SampleScaner;
                return _Scaner;
            }
        }
        private bool OpenedRack = false;
        public void OpenReaderRack()
        {
            if (OpenedRack) return;
            OpenedRack = true;
            op.OnChangeSampleRackStatus += Pcb_OnChangeSampleRackStatus;
            //if (pcbComm is PcbComm pcb)
            //{
            //    pcb.OnChangeSampleRackStatus += Pcb_OnChangeSampleRackStatus;
            //}
        }
        private T_BJ_SampleRack[] _SampleRacks;
        private T_BJ_SampleRack[] SampleRacks
        {
            get
            {
                if (_SampleRacks == null)
                {
                    _SampleRacks = Constants.BJDict[typeof(T_BJ_SampleRack).Name].Select(item => item as T_BJ_SampleRack).ToArray();
                }
                return _SampleRacks;
            }
        }


        String[] scanerVals = new string[3];
        private void Scaner_DataReceived(AbstractScaner scaner, T_BJ_SampleRack sampleRack)
        {
            byte? curIndex = null;
            for (byte i = 0; i < 3; i++)
            {
                if (scanerVals[i] == null)
                {
                    scanerVals[i] = scaner.Read();
                    curIndex = i;
                    break;
                }
            }
            if (curIndex == 2)
            {
                byte i1, i2;
                if (scanerVals[1].Length > 2 && scanerVals[0].Length <= 2 && scanerVals[2].Length <= 2 && byte.TryParse(scanerVals[0], out i1) && byte.TryParse(scanerVals[2], out i2))
                {
                    if (Math.Abs(i1 - i2) == 1)
                    {
                        var w = (byte)(Math.Min(i1, i2) - 1);
                        if(Samples.Where(sam => sam.Index == w).FirstOrDefault()==null)
                        Samples.Add(new SampleInfo() { Index = w });
                        var sample = Samples.Where(sam => sam.Index == w).FirstOrDefault();
                        sample.SetBarcode(sampleRack.Index, scanerVals[1]);
                    }
                    scanerVals[0] = scanerVals[2];
                    scanerVals[1] = null;
                    scanerVals[2] = null;
                }
                else
                {
                    scanerVals[0] = scanerVals[1];
                    scanerVals[1] = scanerVals[2];
                    scanerVals[2] = null;
                }
            }

        }


        private void Pcb_OnChangeSampleRackStatus(byte[] indexs, byte eventType)
        {
            Byte index = (byte)(indexs[0] + 1);
            if (index < 5)//6路感应
            {
                if (eventType == 1)
                {
                    scanDevice.OpenSampleScaner(true);

                    var rack = Constants.BJDict[typeof(T_BJ_SampleRack).Name].Where(item => (item as T_BJ_SampleRack).Index == index).First();
                    //CurentSR = rack as T_BJ_SampleRack;
                    //scanDevice.SampleScaner.SampleRack = rack as T_BJ_SampleRack;
                    //scanDevice.SampleScaner.DataReceived += Scaner_DataReceived;
                    op.MoveScaner((byte)(index));
                }

            }
            else
            {
                if (eventType == 1)
                {
                    op.MoveScaner(0m);
                    Scaner.Stop();
                    //Scaner.Close();
                    //scanDevice.SampleScaner.DataReceived -= Scaner_DataReceived;
                }
            }
            Console.WriteLine("6路感应 index:{0} eventType:{1}", index, eventType);
        }

        public void CloseReaderRack()
        {
            if (!OpenedRack) return;
            op.MoveScaner(0m);
            OpenedRack = false;
            //scanDevice.SampleScaner.DataReceived -= Scaner_DataReceived;
            Scaner.Stop();
            op.OnChangeSampleRackStatus -= Pcb_OnChangeSampleRackStatus;
            // this.View.ShowHint(new MessageWin(res));
        }
        public void Ok()
        {
            IList<SKABO.Common.Models.NotDuplex.SampleInfo> list = new List<SKABO.Common.Models.NotDuplex.SampleInfo>();
            for(byte ri = 0; ri < Constants.SampleRackCount; ri++)
            {
                byte index = 0;
                foreach(var smap in Samples)
                {
                    
                    var barcode = smap.GetBarcode(ri + 1);
                    if (!String.IsNullOrEmpty(barcode))
                    {
                        var item = new SKABO.Common.Models.NotDuplex.SampleInfo() { Index = index, RackIndex = (byte)(ri + 1), Barcode = barcode };
                        list.Add(item);
                    }
                    index++;
                } 
            }
            var listConf = userService.QuerySysConfig("LisConifg").SnValue.ToInstance<LisConifg>();
            if (list.Count > 0)
            {
                if (listConf.TI >= 0)
                {
                    var gel=GelList.Where(g => g.LisGelClass == listConf.TI).FirstOrDefault();
                    var gelIndex = (byte)GelList.IndexOf(gel);
                    foreach(var item in list)
                    {
                        item.SetTestItem(gelIndex);
                    }
                    TestRobot.AddTestBag(GenerateTestBag(0, list.Where(s => s.TestItem1), SKABO.Common.Enums.TestLevelEnum.Normal));
                    TestRobot.AddTestBag(GenerateTestBag(1, list.Where(s => s.TestItem2), SKABO.Common.Enums.TestLevelEnum.Normal));
                    TestRobot.AddTestBag(GenerateTestBag(2, list.Where(s => s.TestItem3), SKABO.Common.Enums.TestLevelEnum.Normal));
                    TestRobot.AddTestBag(GenerateTestBag(3, list.Where(s => s.TestItem4), SKABO.Common.Enums.TestLevelEnum.Normal));
                    TestRobot.AddTestBag(GenerateTestBag(4, list.Where(s => s.TestItem5), SKABO.Common.Enums.TestLevelEnum.Normal));
                    TestRobot.AddTestBag(GenerateTestBag(5, list.Where(s => s.TestItem6), SKABO.Common.Enums.TestLevelEnum.Normal));
                    TestRobot.AddTestBag(GenerateTestBag(6, list.Where(s => s.TestItem7), SKABO.Common.Enums.TestLevelEnum.Normal));
                    TestRobot.AddTestBag(GenerateTestBag(7, list.Where(s => s.TestItem8), SKABO.Common.Enums.TestLevelEnum.Normal));
                    TestRobot.AddTestBag(GenerateTestBag(8, list.Where(s => s.TestItem9), SKABO.Common.Enums.TestLevelEnum.Normal));
                }
            }
            this.RequestClose();

            //
            var his_system = HisSystem.getInstance();
            his_system.SetDirs(listConf.ResultDir, listConf.DuplexDir);
            his_system.ClsReqSample();
            his_system.ClsTestSample();
            for (int i = 0; i < list.Count; i++)
            {
                his_system.AddReqSample(list[i]);
                his_system.AddTestSample(list[i], GelList);
            }
            his_system.WriteRequest();
            his_system.WriteTestItem();
        }
        //工作
        public void HisSystemWork()
        {
            var his_system = HisSystem.getInstance();
            his_system.WorkSetp();
            List<SKABO.Common.Models.NotDuplex.SampleInfo> list = his_system.GetWorkSampleList();
            if (list.Count != 0)
            {
                var listConf = userService.QuerySysConfig("LisConifg").SnValue.ToInstance<LisConifg>();
                TestRobot.AddTestBag(GenerateTestBag(0, list.Where(s => s.TestItem1), SKABO.Common.Enums.TestLevelEnum.Normal));
                TestRobot.AddTestBag(GenerateTestBag(1, list.Where(s => s.TestItem2), SKABO.Common.Enums.TestLevelEnum.Normal));
                TestRobot.AddTestBag(GenerateTestBag(2, list.Where(s => s.TestItem3), SKABO.Common.Enums.TestLevelEnum.Normal));
                TestRobot.AddTestBag(GenerateTestBag(3, list.Where(s => s.TestItem4), SKABO.Common.Enums.TestLevelEnum.Normal));
                TestRobot.AddTestBag(GenerateTestBag(4, list.Where(s => s.TestItem5), SKABO.Common.Enums.TestLevelEnum.Normal));
                TestRobot.AddTestBag(GenerateTestBag(5, list.Where(s => s.TestItem6), SKABO.Common.Enums.TestLevelEnum.Normal));
                TestRobot.AddTestBag(GenerateTestBag(6, list.Where(s => s.TestItem7), SKABO.Common.Enums.TestLevelEnum.Normal));
                TestRobot.AddTestBag(GenerateTestBag(7, list.Where(s => s.TestItem8), SKABO.Common.Enums.TestLevelEnum.Normal));
                TestRobot.AddTestBag(GenerateTestBag(8, list.Where(s => s.TestItem9), SKABO.Common.Enums.TestLevelEnum.Normal));
            }
            his_system.ClsWorkSample();
        }
        public void Close()
        {
            //var seph = new Semaphore(0, 1);
            //var win = new AlertView(IoC.Get<OtherPartDevice>().AlarmFun);
            //win.SetAlertParam("系统错误", "errMsg", seph,false);
            //var dig = win.ShowDialog();
            this.RequestClose();
        }
        private IList<T_Gel> _GelList;
        public IList<T_Gel> GelList
        {
            get
            {
                if (_GelList == null)
                {
                    var gelService = IoC.Get<IGelService>();
                    _GelList = gelService.QueryAllGel();

                }
                return _GelList;
            }
        }
        private IList<TestBag> GenerateTestBag(byte GelIndex, IEnumerable<SKABO.Common.Models.NotDuplex.SampleInfo> infos, SKABO.Common.Enums.TestLevelEnum testLevel)
        {
            if (infos.Count() == 0) return null;
            IList<TestBag> result = new List<TestBag>();
            var gp = infos.GroupBy(item => item.RackIndex);

            var count = gp.Count();
            T_Gel GelType = GelList[GelIndex];
            for (int i = 0; i < count; i++)
            {
                var RackIndex = gp.ElementAt(i).Key;
                var CurentSR = SampleRacks.Where(item => item.Index == RackIndex).FirstOrDefault();
                for (byte x = 0; x < CurentSR.Count; x++)
                {
                    if (CurentSR.Values[x, 0] == null || CurentSR.Values[x, 0].ToString().EndsWith(",F"))
                    {
                        InvokeSetValue(CurentSR, x, 0, null);
                    }
                }
                TestBag testBag = new TestBag(testLevel);
                testBag.GelType = GelType;
                foreach (var s in gp.ElementAt(i))
                {
                    testBag.Add(s.Barcode, s.RackIndex, s.Index);
                    CurentSR.SetValue(s.Index, 0, s.Barcode);
                    if (testBag.SamplesInfo.Count == TestBag.MaxCount)
                    {
                        result.Add(testBag);
                        testBag = new TestBag(testLevel);
                        testBag.GelType = GelType;
                    }
                }
                if (testBag.SamplesInfo.Count < TestBag.MaxCount)
                {
                    result.Add(testBag);
                }
            }



            return result;
        }
        private void InvokeSetValue(VBJ vb, byte x, byte y, Object val)
        {
            Constants.MainWindow.Dispatcher.Invoke(new Action(() => {
                vb.SetValue(x, y, val);
            }));
        }
    }
    }
