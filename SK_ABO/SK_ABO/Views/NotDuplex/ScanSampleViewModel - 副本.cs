using SK_ABO.robot;
using SKABO.BLL.IServices.IGel;
using SKABO.Common;
using SKABO.Common.Enums;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.GEL;
using SKABO.Common.Models.NotDuplex;
using SKABO.Common.Models.TestStep;
using SKABO.Hardware.Core;
using SKABO.Hardware.Model;
using SKABO.Hardware.RunBJ;
using SKABO.Hardware.Scaner;
using SKABO.Ihardware.Core;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using SKABO.Common.Utils;
using SKABO.ActionEngine;
using SKABO.ResourcesManager;
using SKABO.ActionGeneraterEngine;
using SKABO.MAI.ErrorSystem;

namespace SK_ABO.Views.NotDuplex
{
    public class ScanSampleViewModel:Screen
    {
        [StyletIoC.Inject]
        private OtherPartDevice op;
        [StyletIoC.Inject]
        private ScanDevice scanDevice;
        [StyletIoC.Inject]
        private InjectorDevice injectorDevice;
        [StyletIoC.Inject]
        private MachineHandDevice handDevice;
        [StyletIoC.Inject]
        private PiercerDevice piercerDevice;
        [StyletIoC.Inject]
        private GelWarehouseDevice gelwareDevice;

        public Stylet.BindableCollection<SampleInfo> Samples { get; set; }
        public IGelService gelService = IoC.Get<IGelService>();
        protected override void OnViewLoaded()
        {
            Samples = new BindableCollection<SampleInfo>();
            OpenReaderRack();
            base.OnViewLoaded();
            for(int i=0;i<5;i++)
            {
                for (int j = 0; j < 24; j++)
                {
                    var resinfo = ResManager.getInstance().SearchGelCard("T_BJ_SampleRack", "", "", j,i);
                    if (resinfo.sampleinfo != null)
                    {
                        Samples.Add(resinfo.sampleinfo);
                    }
                }
            }
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
            for(byte i = 0; i < 3; i++)
            {
                if (scanerVals[i] == null)
                {
                    scanerVals[i] = scaner.Read();
                    curIndex = i;
                    break;
                }
            }
            if (curIndex == 2  )
            {
                byte i1, i2;
                if(scanerVals[1].Length > 2 && scanerVals[0].Length<= 2&& scanerVals[2].Length <= 2&& byte.TryParse(scanerVals[0],out i1) && byte.TryParse(scanerVals[2],out i2))
                {
                    if (Math.Abs(i1 - i2) == 1)
                    {
                        var w = (byte)(Math.Min(i1, i2) - 1);
                        var sample= Samples.Where(sam => sam.RackIndex == sampleRack.Index && sam.Index == w).FirstOrDefault();
                        if (sample == null)
                        {
                            sample = new SampleInfo() { RackIndex = sampleRack.Index, Index = w };
                            Samples.Add(sample);
                        }
                        sample.Barcode = scanerVals[1];
                        var list = Samples.OrderBy(item => item.RackIndex.ToString() + item.Index.ToString("00")).ToArray();
                        Samples.Clear();
                        Samples.AddRange(list);
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
            Byte index = (byte)(indexs[0]+1);
            if (index < 5)//6路感应
            {
                if (eventType == 1)
                {
                    scanDevice.OpenSampleScaner(true);
                    
                    var rack=Constants.BJDict[typeof(T_BJ_SampleRack).Name].Where(item => (item as T_BJ_SampleRack).Index == index).First();
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

            //if (pcbComm is PcbComm pcb)
            //{
            //    pcb.OnChangeSampleRackStatus -= Pcb_OnChangeSampleRackStatus;
            //}
            op.OnChangeSampleRackStatus -= Pcb_OnChangeSampleRackStatus;
            // this.View.ShowHint(new MessageWin(res));
        }
        //private T_BJ_SampleRack CurentSR;
        public static List<ResInfoData>  lastinfo_list = new List<ResInfoData>();
        public void Ok()
        {
            var resmanager = ResManager.getInstance();
            var actiongen = ActionGenerater.getInstance();
            var GelList = (this.View as ScanSampleView).GelList;
            List<T_GelStep> gelstep_list = new List<T_GelStep>();
            foreach (var sample in Samples)
            {
                var test_list = sample.GetTestList();
                var gel_list = new List<ResInfoData>();
                foreach (var test in test_list)
                {
                    var ware_seat = resmanager.GetResByCode(GelList[test].GelMask + "*", "T_BJ_GelWarehouse","","", lastinfo_list);
                    if (ware_seat == null)
                    {
                        ErrorSystem.WriteActError("无卡",true,false);
                        return;
                    }
                    ware_seat.sampleinfo = sample;
                    lastinfo_list.Add(ware_seat);
                    gel_list.Add(ware_seat);
                }
                foreach (var ware_seat in gel_list)
                {
                    var gelend = new T_GelStep();
                    gelstep_list = gelstep_list.Concat(actiongen.ResolveActions(ware_seat)).ToList();
                    gelend.StepClass = TestStepEnum.GELEND;
                    gelstep_list.Add(gelend);
                }
            }
            //破孔位还有多小个
            int paperseat_count = 9;
            for(int i=0;i< gelstep_list.Count;i++)
            {
                if (gelstep_list[i].StepClass == TestStepEnum.LoadGel&& paperseat_count>0)
                {
                    gelstep_list[i].StepIndex = 10000 - i;
                    paperseat_count--;
                }
                else
                    gelstep_list[i].StepIndex = 1000-i;
            }
            gelstep_list.Sort((T_GelStep a, T_GelStep b) => { return a.StepIndex < b.StepIndex ? 1 : -1; });
            var action_tree = actiongen.DivideIntoGroups(gelstep_list, 1);
            InjLogicManager.getInstance().AddAction(action_tree);

            this.RequestClose();
        }
        public void Close()
        {
            this.RequestClose();
        }
        private IList<TestBag> GenerateTestBag(byte GelIndex,IEnumerable<SampleInfo> infos, SKABO.Common.Enums.TestLevelEnum testLevel)
        {
            if (infos.Count() == 0) return null;
            IList<TestBag> result = new List<TestBag>();
            var GelList = (this.View as ScanSampleView).GelList;
            var gp=infos.GroupBy(item => item.RackIndex).OrderBy(item=>item.Key);
            
            var count = gp.Count();
            T_Gel GelType = GelList[GelIndex];
            for (int i = 0; i < count; i++)
            {
                var RackIndex = gp.ElementAt(i).Key;
                var CurentSR = SampleRacks.Where(item => item.Index == RackIndex).FirstOrDefault();
                for (byte x = 0; x < CurentSR.Count; x++)
                {
                    if(CurentSR.Values[x, 0] == null || CurentSR.Values[x, 0].ToString().EndsWith(",F"))
                    {
                        InvokeSetValue(CurentSR, x, 0, null);
                    }
                }
                TestBag testBag = new TestBag(testLevel);
                testBag.GelType = GelType;
                var StartIndex = -1;
                foreach (var s in gp.ElementAt(i))
                {
                    if (s.Index - StartIndex > 1 && testBag.SamplesInfo.Count>0)
                    {
                        result.Add(testBag);
                        testBag = new TestBag(testLevel);
                        testBag.GelType = GelType;
                    }
                    StartIndex = s.Index;
                    testBag.Add(s.Barcode, s.RackIndex, s.Index);
                    CurentSR.SetValue(s.Index, 0, s.Barcode);
                    if (testBag.SamplesInfo.Count == TestBag.MaxCount* testBag.GelType.GelRenFen)
                    {
                        result.Add(testBag);
                        testBag = new TestBag(testLevel);
                        testBag.GelType = GelType;
                    }
                }
                if (testBag.SamplesInfo.Count>0 && testBag.SamplesInfo.Count < TestBag.MaxCount* GelType.GelRenFen)
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
        public void TestDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid datagrid = sender as DataGrid;
            Point aP = e.GetPosition(datagrid);
            IInputElement obj = datagrid.InputHitTest(aP);
            if (!(obj is Visual)) return;
            DependencyObject target = obj as DependencyObject;


            while (target != null)
            {
                
                
                if (target is DataGridColumnHeader)
                {
                    break;
                }
                target = VisualTreeHelper.GetParent(target);
                
            }
            if(target is DataGridColumnHeader head)
            {
                var index = head.Column.DisplayIndex - 2;
                var tp = typeof(SampleInfo).GetProperty("TestItem" + index);
                if(Samples.Any(item=> !(bool)(tp.GetValue(item)))){
                    foreach (var samp in Samples)
                    {
                        tp.SetValue(samp, true);
                    }
                }
                else
                {
                    foreach (var samp in Samples)
                    {
                        tp.SetValue(samp, false);
                    }
                }
                
            }
        }
    }
}
