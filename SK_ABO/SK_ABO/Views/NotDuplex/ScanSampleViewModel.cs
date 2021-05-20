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
using System.Text.RegularExpressions;
using SKABO.Hardware.Enums;

namespace SK_ABO.Views.NotDuplex
{
    public class ScanSampleIndexData
    {
        public string []codes = {"","",""};
        public int index = 0;
        public ScanSampleIndexData(int iindex,string code1, string code2, string code3)
        {
            codes[0] = code1;
            codes[1] = code2;
            codes[2] = code3;
            index = iindex;
        }
        public bool IsMatching(List<string> codes_tem)
        {
            if(codes_tem.Count==3)
            {
                return (codes_tem[0] == codes[0] && codes_tem[2] == codes[2]|| codes_tem[0] == codes[2] && codes_tem[2] == codes[0]);
            }
            return false;
        }
    }
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
        [StyletIoC.Inject]
        private CameraDevice cameraDevice;

        public static BindableCollection<SampleInfo> Samples { get; set; } = new BindableCollection<SampleInfo>();
        public IGelService gelService = IoC.Get<IGelService>();
        public int rack_index = 0;
        public int sample_index = 0;
        private AbstractScaner scaner { get; set; } = null;
        private string scaner_port { get; set; } = "";
        public List<string> scaner_code_list { get; set; } = new List<string>();
        List<ScanSampleIndexData> template_code_list = new List<ScanSampleIndexData>();
        public bool visible = false;
        public static bool is_double = false;//是否双工
        public static int g_batch_id = 0;//实验批次号
        protected override void OnViewLoaded()
        {
            foreach(var sample in Samples)
            {
                sample.TestItem1 = false;
                sample.TestItem2 = false;
                sample.TestItem3 = false;
                sample.TestItem4 = false;
                sample.TestItem5 = false;
                sample.TestItem6 = false;
                sample.TestItem7 = false;
                sample.TestItem8 = false;
                sample.TestItem9 = false;
            }
            op.CanComm.SetListenFun(op.OP.SampleRackCoils[0].Addr, CanFunCodeEnum.UPLOAD_REGISTER, RackChange);
            var scaner_info = ResManager.getInstance().GetScaner("0");
            if (scaner_info == null) ErrorSystem.WriteActError("扫码器无法识别!", false);
            if (scaner_info != null)
            {
                scaner = IoC.Get<AbstractScaner>(scaner_info.ScanerType);
                scaner_port = scaner_info.Port;
            }
            visible = true;
            for (int i = 0; i < 24; i++)
            {
                
                template_code_list.Add(new ScanSampleIndexData(i, string.Format("{0:D2}", i + 1), "*", string.Format("{0:D2}", i + 2)));
            }
            if(Samples.Count==0)
            {
                for (int i = 0; i < 24; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        var sample_code = "";
                        var resinfo = ResManager.getInstance().SearchGelCard("T_BJ_SampleRack", "", "", i, j);
                        resinfo.SetCode(sample_code);
                        resinfo.Values[resinfo.CountX, resinfo.CountY] = resinfo;
                        resinfo.sampleinfo = new SampleInfo(sample_code, (byte)i, (byte)j);
                        Samples.Add(resinfo.sampleinfo);
                    }
                }
            }
            var samples_sort = Samples.ToList();
            samples_sort.Sort((a, b) =>
            {
                return (a.RackIndex * 100 + a.Index) - (b.RackIndex * 100 + b.Index);
            });
            Samples.Clear();
            foreach (var item in samples_sort)
            {
                Samples.Add(item);
            }

            base.OnViewLoaded();
        }
        public bool IsInScanerCodeList(string value)
        {
            foreach(var code in scaner_code_list)
            {
                if (code == value) return true;
            }
            return false;
        }

        public int GetSampleIndex(List<string> codes, List<ScanSampleIndexData> templates)
        {
            foreach(var item in templates)
            {
                if (item.IsMatching(codes))
                    return item.index;
            }
            return -1;
        }

        private void Scaner_DataReceived(AbstractScaner scaner_tem)
        {
            string sample_code = "";
            var scan_code = scaner_tem.Read();
            if(IsInScanerCodeList(scan_code)==false)
            scaner_code_list.Add(scan_code);
            if (scaner_code_list.Count > 3) scaner_code_list.RemoveAt(0);
            if(scaner_code_list.Count==3)
            {
                sample_index = GetSampleIndex(scaner_code_list, template_code_list);
                if (sample_index != -1)
                {
                    sample_code = scaner_code_list[1];
                    var sample_find = Samples.Where(item => item.Index == sample_index && item.RackIndex == rack_index).ToList();
                    if (sample_find.Count != 0)
                    {
                        sample_find[0].Barcode = sample_code;
                        sample_find[0].RackIndex = (byte)rack_index;
                        sample_find[0].Index = (byte)sample_index;
                        var resinfo = ResManager.getInstance().SearchGelCard("T_BJ_SampleRack", "", "", sample_index, rack_index);
                        if(resinfo!=null)
                        {
                            resinfo.SetCode(sample_code);
                            resinfo.Values[resinfo.CountX, resinfo.CountY] = resinfo;
                        }
                    }
                }
            }

        }

        public int GetRackIndex(byte data)
        {
            for(int i=0;i<6;i++)
            {
                if((data&(0x01<<i))!=0x00)
                {
                    return 5-i;
                }
            }
            return -1;
        }

        public void RackChange(int tagerid,byte []data)
        {
            if (visible == false) return;
            rack_index = GetRackIndex(data[5]);
            var seq_move = Sequence.create();
            if (rack_index >= 0)
            {
                var rack_info = ResManager.getInstance().GetSampleRack(rack_index+1);
                seq_move.AddAction(MoveTo.create(op, 3000, (int)rack_info.ReaderX));
                seq_move.AddAction(SkCallBackFun.create((ActionBase act_tem) => {
                    scaner.CancelAllEvent();
                    scaner.Open(scaner_port);
                    var res = scaner.Start(false);
                    if (res)
                    {
                        scaner.DataReceived += Scaner_DataReceived;
                    }
                    else
                    {
                        scaner.Close();
                    }
                    return true;
                }));
            }
            else
            {
                scaner.Stop();
                scaner.Close();
                seq_move.AddAction(MoveTo.create(op, 3000, 0));
            }
            seq_move.runAction(op);
        }

        protected override void OnClose()
        {
            scaner.Stop();
            scaner.Close();
            var task = Task.Run(() =>
            {
                var seq_move = Sequence.create();
                seq_move.AddAction(MoveTo.create(op, 3000, 0));
                seq_move.runAction(op);
            });
            visible = false;
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

        public List<SampleInfo> GetCrossMatchingSampleInfo(SampleInfo sample)
        {
            var sample_list = new List<SampleInfo>();
            bool beg_find = false;
            foreach (var item in Samples)
            {
                if(beg_find)
                {
                    if(item.Barcode!=""&&item.RackIndex == sample.RackIndex)
                    {
                        sample_list.Add(item);
                    }
                    else
                    {
                        break;
                    }
                }
                if(item.Index== sample.Index&& item.RackIndex== sample.RackIndex)
                {
                    beg_find = true;
                }
            }
           return sample_list;
        }

        public void Ok()
        {
            var resmanager = ResManager.getInstance();
            var actiongen = ActionGenerater.getInstance();
            var GelList = (this.View as ScanSampleView).GelList;
            List<T_GelStep> gelstep_list = new List<T_GelStep>();
            bool isRepeat = Samples.Where(item=>item.Barcode!="").GroupBy(i => i.Barcode).Any(g => g.Count() > 1);
            if(isRepeat)
            {
                ErrorSystem.WriteActError("样本条码有重复!",true,false);
            }
            else
            {
                var samples_list = Samples.Where(item => item.Barcode != "").ToList();
                foreach (var sample in samples_list)
                {
                    if(sample.Barcode!="")
                    {
                        var test_list = sample.GetTestList();
                        var gel_list = new List<T_Gel>();
                        foreach (var test in test_list)
                        {
                            var gel = GelList[test].clone();
                            if(gel.IsCrossMatching)
                            {
                                //交叉配血
                                //寻找当前木样本架连续位
                                var sample_list = GetCrossMatchingSampleInfo(sample);
                                foreach(var sample_tem in sample_list)
                                {
                                    gel = GelList[test].clone();
                                    var exp_package = ExperimentPackage.Create(actiongen.ResolveActions(gel), gel.GelMask, sample.Barcode, sample_tem.Barcode, sample.GetLever(), gel.GelType, gel.GelRenFen, gel.AfterKKTime, gel.IsUsedGel, gel.ID,gel.IsCrossMatching, g_batch_id,is_double);
                                    ExperimentLogic.getInstance().AddPackage(exp_package);
                                }
                            }
                            else
                            {
                                var exp_package = ExperimentPackage.Create(actiongen.ResolveActions(gel), gel.GelMask, sample.Barcode,"", sample.GetLever(), gel.GelType, gel.GelRenFen, gel.AfterKKTime, gel.IsUsedGel, gel.ID,gel.IsCrossMatching, g_batch_id, is_double);
                                ExperimentLogic.getInstance().AddPackage(exp_package);
                            }
                        }
                    }
                }
                ExperimentLogic.getInstance().UpDataAction();
                this.RequestClose();
                g_batch_id++;
            }
        }
        public void Close()
        {
            this.RequestClose();
        }
        private void InvokeSetValue(VBJ vb, byte x, byte y, Object val)
        {
            Constants.MainWindow.Dispatcher.Invoke(new Action(() => {
                vb.SetValue(x, y, val);
            }));
        }
        public void TestDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        public void TestDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            var data_grid = (System.Windows.Controls.DataGrid)sender;
            int index = e.Column.DisplayIndex-3;
            var GelList = (this.View as ScanSampleView).GelList;
            var geltest = GelList[index];
            if(geltest.IsCrossMatching)
            {
                int ok_count = 0;
                foreach (var item in data_grid.Items)
                {
                    var info = (SKABO.Common.Models.NotDuplex.SampleInfo)item;
                    if (info.Barcode != "") ok_count++;
                    else ok_count = 0;
                    if (ok_count==1) info.SetTestItem(index, !info.GetTestItem(index));
                }
            }
            else
            {
                foreach (var item in data_grid.Items)
                {
                    var info = (SKABO.Common.Models.NotDuplex.SampleInfo)item;
                    info.SetTestItem(index, !info.GetTestItem(index));
                }
            }
            e.Handled = true;
        }

    }
}
