using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using SKABO.Common.Utils;
using System.Windows;
using DrapControlLibrary;
using SKABO.BLL.IServices.ITrace;
using SKABO.Common.Enums;
using SKABO.BLL.IServices.IDevice;
using SKABO.Common.Models.BJ;
using System.Windows.Interactivity;
using System.Windows.Media;
using SKABO.Common;
using System.IO;
using SK_ABO.UserCtrls.Base;
using SK_ABO.Views.Device;
using System.Windows.Input;
using SK_ABO.UserCtrls;
using System.Threading;
using SK_ABO.Views.Start;
using SKABO.Common.UserCtrls;
using SKABO.Hardware.RunBJ;
using SK_ABO.Views;
using SKABO.Hardware.Scaner;
using System.Text.RegularExpressions;
using SKABO.Hardware.Core;
using SK_ABO.Views.NotDuplex;
using SKABO.BLL.IServices.IUser;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Common.Views;
using SKABO.MAI.ErrorSystem;
using SKABO.ActionEngine;
using SKABO.ActionGeneraterEngine;
using SKABO.ResourcesManager;
using SKABO.BLL.IServices.IGel;
using SKABO.Common.Models.NotDuplex;
using SKABO.Common.Models.Duplex;

namespace SK_ABO.Pages
{
    public class HomeViewModel: Screen
    {
        [StyletIoC.Inject]
        private IWindowManager windowManager;
        [StyletIoC.Inject]
        private ITraceService TraceService;
        [StyletIoC.Inject]
        private IBJService BJService;
        [StyletIoC.Inject]
        private IUserService UserService;
        [StyletIoC.Inject]
        private OtherPartDevice OPDevice;
        [StyletIoC.Inject]
        private ScanDevice scanDevice;
        [StyletIoC.Inject]
        private GelWarehouseDevice gwDevice;
        [StyletIoC.Inject]
        private CouveuseMixerDevice cmDevice;
        [StyletIoC.Inject]
        private CentrifugeDevice centDevice;
        [StyletIoC.Inject]
        private CentrifugeMrg centMrg;
        [StyletIoC.Inject]
        private MachineHandDevice handDevice;
        [StyletIoC.Inject]
        private PiercerDevice piercerDevice;
        [StyletIoC.Inject]
        private InjectorDevice injDevice;
        [StyletIoC.Inject]
        private OtherPartDevice opDevice;
        [StyletIoC.Inject]
        private GelWarehouseDevice gelwareDevice;
        [StyletIoC.Inject]
        private CameraDevice cameraDevice;
        [StyletIoC.Inject]
        private CentrifugeMrg cenMrg;
        //动作引擎
        public Engine engine = Engine.getInstance();
        public static System.Windows.Forms.Timer myTimer = null;
        /// <summary>
        /// 双工锁
        /// </summary>
        private object doubleMonitor = new object();

        private int _GelCount;
        private String ErrorMsg;
        /// <summary>
        /// 废Gel卡计数
        /// </summary>
        public int GelCount { get=>_GelCount; set {
                _GelCount = value;
                ShowWarnGelCount = value > 100;
            } }
        private int _TipCount;
        /// <summary>
        /// 废针计数
        /// </summary>
        public int TipCount { get=>_TipCount; set {
                _TipCount = value;
                ShowWarnTipCount = value > 400;
            } }
        /// <summary>
        /// 清理废Gel卡桶
        /// </summary>
        public bool ShowWarnGelCount { get; set; }
        /// <summary>
        /// 清理废针桶
        /// </summary>
        public bool ShowWarnTipCount { get; set; }
        private Canvas canvas;
        
        private bool loaded = false;
        private String LayoutPath;
        public void CleanCount(object sender, MouseButtonEventArgs e)
        {
            if (sender is TipCount_Control)
            {
                (Constants.BJDict["T_BJ_SampleRack"][1] as VBJ).SetValue(1, 0, "dsfdsf");
                TipCount = 0;
                
            }
            else
            {
                GelCount = 0;
            }
        }

        public HomeViewModel()
        {
            myTimer = new System.Windows.Forms.Timer();
            myTimer.Tick += new EventHandler(UpDataUI);
            myTimer.Enabled = true;
            myTimer.Interval = 1000;
            myTimer.Start();
        }

        protected override void OnViewLoaded()
        {
            if (loaded) return;
            CanNoDoubleBooldNormal = false;
            CanNoDoubleBooldCorss = false;
            CanDoDouble = false;

            loaded = true;
            CanStartAdjust = true;
            CanEndAdjust = false;
            CanStart = true;
            canvas = (this.View as HomeView).MainCanvas;
            base.OnViewLoaded();
            String str=null;
            LayoutPath = AppDomain.CurrentDomain.BaseDirectory + @"HomeLayout.json";
            try
            {
                str = System.IO.File.ReadAllText(LayoutPath);
            }catch(Exception ex)
            {
                windowManager.ShowMessageBox("读取布局文件失败！");
            }
            var list=str.ToInstance<List<Layout>>();
            
            foreach (UIElement ui in canvas.Children)
            {
                if (ui is FrameworkElement Fele)
                {
                    SetCorrate(Fele, list);
                }
            }
            Constants.BJDict.Clear();
            LoadT_BJ_AgentiaWarehouse(list);
            LoadT_BJ_Camera(list);
            LoadT_BJ_Centrifuge(list);
            LoadT_BJ_DeepPlate(list);
            LoadT_BJ_GelSeat(list);
            LoadT_BJ_GelWarehouse(list);
            LoadT_BJ_Piercer(list);
            LoadT_BJ_SampleRack(list);
            LoadT_BJ_Scaner(list);
            LoadT_BJ_Tip(list);
            LoadT_BJ_Unload(list);
            LoadT_BJ_WastedSeat(list);

            //初始化计数
            GelCount = int.Parse(UserService.QuerySysConfig("GelCount").SnValue);
            TipCount = int.Parse(UserService.QuerySysConfig("TipCount").SnValue);
            ResManager.getInstance().gel_count = GelCount;
            ResManager.getInstance().tip_count = TipCount;
            engine.injectorDevice = injDevice;
            engine.handDevice = handDevice;
            engine.gelwareDevice = gelwareDevice;
            engine.cenMrg = cenMrg;
            engine.cameraDevice = cameraDevice;
            engine.piercerDevice = piercerDevice;
            engine.opDevice = OPDevice;
            engine.couveuseMixer = cmDevice;
            engine.start();
            //双工文件夹
            var listConf = UserService.QuerySysConfig("LisConifg").SnValue.ToInstance<LisConifg>();
            if (Directory.Exists(listConf.ResultDir) && Directory.Exists(listConf.DuplexDir))
            {
                var his_system = HisSystem.getInstance();
                his_system.SetDirs(listConf.ResultDir, listConf.DuplexDir);
            }
        }
        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpDataUI(object sender, EventArgs e)
        {
            var resmanager = ResManager.getInstance();
            lock(ResManager.ui_lockObj)
            {
                //更新gel卡位
                foreach (var seat in resmanager.deepplate_list)
                {
                    seat.Refresh();
                }
                foreach (var seat in resmanager.agentiawa_list)
                {
                    seat.Refresh();
                }
                foreach (var seat in resmanager.centrifuge_list)
                {
                    seat.Refresh();
                }
                foreach (var seat in resmanager.gelseat_list)
                {
                    seat.Refresh();
                }
                foreach (var seat in resmanager.gelwarehouse_list)
                {
                    seat.Refresh();
                }
                foreach (var seat in resmanager.samplerack_list)
                {
                    seat.Refresh();
                }
                foreach (var seat in resmanager.tip_list)
                {
                    seat.Refresh();
                }
                GelCount = resmanager.gel_count;
                TipCount = resmanager.tip_count;
            }
        }

        private void InjDevice_OutTipEvent(int count, Enterclose[] ents)
        {
            TipCount+= count;
            UserService.UpdateSysConfig(new SKABO.Common.Models.Config.SysConfig("TipCount", TipCount.ToString()));
        }

        private void HandDevice_PutGELToFeiKa()
        {
            GelCount++;
            UserService.UpdateSysConfig(new SKABO.Common.Models.Config.SysConfig("GelCount", GelCount.ToString()));
        }

        private void SetCorrate(FrameworkElement Fele, IList<Layout> list)
        {
            if (list == null) return;
            Layout ly = list.Where(c => c.ID == Fele.Name).FirstOrDefault();
            if (ly.ID != null)
            {
                Canvas.SetLeft(Fele, ly.Left);
                Canvas.SetTop(Fele, ly.Top);
            }
        }
        private void LoadT_BJ_AgentiaWarehouse(IList<Layout> Lylist)
        {
            var list = BJService.QueryBJ<T_BJ_AgentiaWarehouse>();
            if (list != null && list.Count > 0)
            {
                String Key = "T_BJ_AgentiaWarehouse";
                Constants.BJDict.Add(Key, list);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is T_BJ_AgentiaWarehouse item)
                    {
                        UserCtrls.ReagentRack_Control ReagentRack = new UserCtrls.ReagentRack_Control()
                        {
                            Width = 20,
                            Count = item.Count,
                            Height = item.Count * 20 * 2 - ((item.Count) * 3),
                            ToolTip = item.Name,
                            Name = "ReagentRack_" + i,
                            Tag=Key,
                            DataContext = item
                        };
                        ReagentRack.MouseDoubleClick += Control_MouseDoubleClick;
                        Interaction.GetBehaviors(ReagentRack).Add(new DragInCanvasBehavior());
                        canvas.Children.Add(ReagentRack);
                        SetCorrate(ReagentRack, Lylist);
                        if (ReagentRack is BJControl bjc)
                        {
                            bjc.Index = i;
                            bjc.AddedControls += Bjc_AddedControls;
                        }
                    }
                    
                }
            }
        }
        private void LoadT_BJ_Piercer(IList<Layout> Lylist)
        {
            var list = BJService.QueryBJ<T_BJ_Piercer>();
            if (list != null && list.Count > 0)
            {
                Constants.BJDict.Add("T_BJ_Piercer", list);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is T_BJ_Piercer item)
                    {
                        UserCtrls.Piercer_Control Piercer = new UserCtrls.Piercer_Control()
                        {
                            Width = 60,
                            Height = 40,
                            PierceName=item.Name,
                            ToolTip = item.Name,
                            Name = "Piercer_" + i
                        };
                        Interaction.GetBehaviors(Piercer).Add(new DragInCanvasBehavior());
                        canvas.Children.Add(Piercer);
                        SetCorrate(Piercer, Lylist);
                    }
                }
            }
        }
        private void LoadT_BJ_Scaner(IList<Layout> Lylist)
        {
            var list = BJService.QueryBJ<T_BJ_Scaner>();
            if (list != null && list.Count > 0)
            {
                Constants.BJDict.Add("T_BJ_Scaner", list);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is T_BJ_Scaner item)
                    {
                        UserCtrls.Barcode_Control Scaner = new UserCtrls.Barcode_Control()
                        {
                            Width=49,
                            Height=46,
                            ToolTip = item.Name,
                            Name = "Scaner_" + i
                        };
                        //tipDish_Control.in
                        Interaction.GetBehaviors(Scaner).Add(new DragInCanvasBehavior());
                        canvas.Children.Add(Scaner);
                        SetCorrate(Scaner, Lylist);
                    }
                }
            }
        }
        private void LoadT_BJ_Centrifuge(IList<Layout> Lylist)
        {
            var list = BJService.QueryBJ<T_BJ_Centrifuge>();
            if (list != null && list.Count > 0)
            {
                String Key = "T_BJ_Centrifuge";
                Constants.BJDict.Add(Key, list);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is T_BJ_Centrifuge item)
                    {
                        UserCtrls.Centrifuge_Control Centriguge = new UserCtrls.Centrifuge_Control()
                        {
                            Width = 160,
                            Height = 160,
                            ToolTip = item.Name,
                            DataContext = item,
                            Tag = Key,
                            Name = "C_" + i
                        };
                        Centriguge.MouseDoubleClick += Control_MouseDoubleClick;
                        Interaction.GetBehaviors(Centriguge).Add(new DragInCanvasBehavior());
                        canvas.Children.Add(Centriguge);
                        SetCorrate(Centriguge, Lylist);

                        if (Centriguge is BJControl bjc)
                        {
                            bjc.Index = i;
                            bjc.AddedControls += Bjc_AddedControls;
                        }
                    }
                }
            }
        }
        private void LoadT_BJ_Camera(IList<Layout> Lylist)
        {
            var list = BJService.QueryBJ<T_BJ_Camera>();
            if (list != null && list.Count > 0)
            {
                Constants.BJDict.Add("T_BJ_Camera", list);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is T_BJ_Camera item)
                    {
                        UserCtrls.Camera_Control Camera = new UserCtrls.Camera_Control()
                        {
                            Name = "CA_" + i,
                            ToolTip = item.Name
                        };
                        //tipDish_Control.in
                        Interaction.GetBehaviors(Camera).Add(new DragInCanvasBehavior());
                        canvas.Children.Add(Camera);
                        SetCorrate(Camera, Lylist);
                    }
                }
            }
        }
        private void LoadT_BJ_WastedSeat(IList<Layout> Lylist)
        {
            var list = BJService.QueryBJ<T_BJ_WastedSeat>();
            if (list != null && list.Count > 0)
            {
                Constants.BJDict.Add("T_BJ_WastedSeat", list);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is T_BJ_WastedSeat item)
                    {
                        UserCtrls.RubbishBin_Control RubbishBin = new UserCtrls.RubbishBin_Control()
                        {
                            ToolTip=item.Name,
                            Name = "Ru_" + i
                        };
                        //tipDish_Control.in
                        Interaction.GetBehaviors(RubbishBin).Add(new DragInCanvasBehavior());
                        canvas.Children.Add(RubbishBin);
                        SetCorrate(RubbishBin, Lylist);
                    }
                }
            }
        }

        private void LoadT_BJ_Unload(IList<Layout> Lylist)
        {
            var list = BJService.QueryBJ<T_BJ_Unload>();
            if (list != null && list.Count > 0)
            {
                Constants.BJDict.Add("T_BJ_Unload", list);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is T_BJ_Unload item)
                    {
                        UserCtrls.LoseNeedle_UserControl LoseNeedle = new UserCtrls.LoseNeedle_UserControl()
                        {
                            Count = item.Count,
                            Width = 30,
                            Height = item.Count * 12 + 2,
                            Name = "Lo_" + i,
                            ToolTip = item.Name,
                            Background = Brushes.Gray
                        };
                        Interaction.GetBehaviors(LoseNeedle).Add(new DragInCanvasBehavior());
                        canvas.Children.Add(LoseNeedle);
                        SetCorrate(LoseNeedle, Lylist);
                    }
                }
            }
        }
        private void LoadT_BJ_GelWarehouse(IList<Layout> Lylist)
        {
            var list = BJService.QueryBJ<T_BJ_GelWarehouse>();
            if (list != null && list.Count > 0)
            {
                String Key = "T_BJ_GelWarehouse";
                Constants.BJDict.Add(Key, list);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is T_BJ_GelWarehouse item)
                    {
                        UserCtrls.GelCardRack_Control GelWarehouse = new UserCtrls.GelCardRack_Control()
                        {
                            Count = item.Count,
                            Width = 50,
                            Height = item.Count * 7 + 2 + 20,
                            Name = "GC_" + i,
                            GelName = item.Name,
                            ToolTip = item.Name,
                            DataContext = item,
                            Tag = Key,
                            Background = Brushes.Gray
                        };
                        GelWarehouse.MouseDoubleClick += Control_MouseDoubleClick;
                        Interaction.GetBehaviors(GelWarehouse).Add(new DragInCanvasBehavior());
                        canvas.Children.Add(GelWarehouse);
                        SetCorrate(GelWarehouse, Lylist);
                        if (GelWarehouse is BJControl bjc)
                        {
                            bjc.Index = i;
                            bjc.AddedControls += Bjc_AddedControls;
                        }
                    }
                }
            }
        }
        private void LoadT_BJ_DeepPlate(IList<Layout> Lylist)
        {
            var list = BJService.QueryBJ<T_BJ_DeepPlate>();
            if (list != null && list.Count > 0)
            {
                String Key = "T_BJ_DeepPlate";
                Constants.BJDict.Add(Key, list);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is T_BJ_DeepPlate item)
                    {
                        UserCtrls.TipDish_Control dp = new UserCtrls.TipDish_Control()
                        {
                            CountX = item.CountX,
                            CountY = item.CountY,
                            Width = item.CountX * 10,
                            Height = item.CountY * 10 + 20,
                            TipName = item.Name,
                            ToolTip = item.Name,
                            IsShowCount = true,
                            DataContext = item,
                            Tag = Key,
                            Name = "dp_" + i
                        };
                        dp.MouseDoubleClick += Dp_MouseDoubleClick;
                        Interaction.GetBehaviors(dp).Add(new DragInCanvasBehavior());
                        canvas.Children.Add(dp);
                        SetCorrate(dp, Lylist);
                        if (dp is BJControl bjc)
                        {
                            bjc.Index = i;
                            bjc.AddedControls += Bjc_AddedControls;
                        }
                    }
                }
            }
        }

        private void Dp_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var vm = IoC.Get<ShowDeepPlateViewModel>();
            if (sender is UserControl uc)
            {
                vm.Key = uc.Tag.ToString();
                vm.CanSave = true;
                e.Handled = true;
            }
            windowManager.ShowDialog(vm);
        }

        private void LoadT_BJ_GelSeat(IList<Layout> Lylist)
        {
            var list = BJService.QueryBJ<T_BJ_GelSeat>();
            if (list != null && list.Count > 0)
            {
                String Key = "T_BJ_GelSeat";
                Constants.BJDict.Add(Key, list);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is T_BJ_GelSeat item)
                    {
                        UserCtrls.GelCardRack_Control GelSeat = new UserCtrls.GelCardRack_Control()
                        {
                            Count = item.Count,
                            Width = 50,
                            Height = item.Count * 7 + 2 + 20,
                            Name = "GS_" + i,
                            GelName = item.Name,
                            ToolTip = item.Name,
                            DataContext = item,
                            Tag = Key,
                            Background = Brushes.Gray
                        };
                        GelSeat.MouseDoubleClick += Control_MouseDoubleClick;

                        Interaction.GetBehaviors(GelSeat).Add(new DragInCanvasBehavior());
                        canvas.Children.Add(GelSeat);
                        SetCorrate(GelSeat, Lylist);
                        if(GelSeat is BJControl bjc)
                        {
                            bjc.Index = i;
                            bjc.AddedControls += Bjc_AddedControls;
                        }
                    }
                }
            }
        }
        

        private void LoadT_BJ_Tip(IList<Layout> Lylist)
        {
            var list=BJService.QueryBJ<T_BJ_Tip>();
            if (list != null && list.Count > 0)
            {
                Constants.BJDict.Add("T_BJ_Tip", list);
                for (int i= 0;i < list.Count;i++)
                {
                    if (list[i] is T_BJ_Tip item)
                    {
                        UserCtrls.TipDish_Control tipDish_Control = new UserCtrls.TipDish_Control()
                        {
                            CountX = item.CountX,
                            CountY = item.CountY,
                            Width = item.CountX * 10,
                            Height = item.CountY * 10 + 20,
                            TipName = item.Name,
                            ToolTip = item.Name,
                            DataContext = item,
                            Name = "Tip_" + i,
                            Tag = "T_BJ_Tip"
                        };
                        tipDish_Control.MouseDoubleClick += Tip_MouseDoubleClick;
                        Interaction.GetBehaviors(tipDish_Control).Add(new DragInCanvasBehavior());
                        canvas.Children.Add(tipDish_Control);
                        SetCorrate(tipDish_Control, Lylist);
                        if (tipDish_Control is BJControl bjc)
                        {
                            bjc.Index = i;
                            bjc.AddedControls += Bjc_AddedControls;
                        }
                    }
                }
            }
        }

        private void Tip_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var vm = IoC.Get<FirstStepViewModel>();
            windowManager.ShowDialog(vm);
        }

        private void Control_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var vm = IoC.Get<BJConfigViewModel>();
            if (sender is UserControl uc) {
                vm.Key = uc.Tag.ToString();
                vm.CanSave = false;
            }
            windowManager.ShowDialog(vm);
        }

        private void LoadT_BJ_SampleRack(IList<Layout> Lylist)
        {
            int w = 16;
            var list = BJService.QueryBJ<T_BJ_SampleRack>();
            if (list != null && list.Count > 0)
            {
                String Key = "T_BJ_SampleRack";
                Constants.BJDict.Add(Key, list);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is T_BJ_SampleRack item)
                    {
                        UserCtrls.SampleRack_Control SampleRack = new UserCtrls.SampleRack_Control()
                        {
                            Count = item.Count,
                            Width = w,
                            Height = item.Count * w + 20,
                            ToolTip = item.Name,
                            RackName = (i + 1).ToString(),
                            DataContext = item,
                            Tag=Key,
                            Name = "SRack_" + i
                        };
                        SampleRack.MouseDoubleClick += Control_MouseDoubleClick;
                        Interaction.GetBehaviors(SampleRack).Add(new DragInCanvasBehavior());
                        canvas.Children.Add(SampleRack);
                        SetCorrate(SampleRack, Lylist);
                        if(SampleRack is BJControl bjc)
                        {
                            bjc.Index = i;
                            bjc.AddedControls += Bjc_AddedControls;
                        }
                    }
                }
            }
        }

        private void Bjc_AddedControls(object sender, AddedControlsEventArgs e)
        {
            if(sender is UserControl u && u.DataContext is VBJ vbj)
            {
                LoadValues(e.TName + "_" + e.Index, vbj);
            }
            
        }

        private object[,] LoadValues(String BJName, VBJ vbj)
        {
            object[,] res = null;
            String f= System.AppDomain.CurrentDomain.BaseDirectory + @"Config\BJParameters\"+BJName+".json";
            if (File.Exists(f))
            {
                String jsonstr = File.ReadAllText(f, Encoding.Default);
                try
                {
                    res = jsonstr.ToInstance<object[,]>();
                    if (res != null)
                    {
                        for(int x = 0; x < Math.Min(res.GetLength(0), vbj.Values.GetLength(0)); x++)
                        {
                            for (int y = 0; y < Math.Min(res.GetLength(1), vbj.Values.GetLength(1)); y++)
                            {
                                vbj.SetValue(x, y, res[x, y]);
                                //vbj.Values[x, y] = res[x, y];
                            }
                        }
                    }
                }catch(Exception ex)
                {
                    Tool.AppLogError(f,ex);
                }
            }
            return res;
        }
        public bool CanStartAdjust { get; set; }
        public bool CanEndAdjust { get; set; }
        public void StartAdjust()
        {
            DragInCanvasBehavior.CanMove = true;
            CanStartAdjust = false;
            CanEndAdjust = true;
        }
        public void EndAdjust()
        {
            CanStartAdjust = true;
            CanEndAdjust = false;
            IList<Layout> list = new List<Layout>();
            foreach(UIElement ui in canvas.Children)
            {
                if(ui is FrameworkElement Fele){
                    Layout ly = new Layout();
                    ly.ID = Fele.Name;
                    ly.Left = Canvas.GetLeft(ui);
                    ly.Top = Canvas.GetTop(ui);
                    list.Add(ly);
                }
            }
            String str=list.ToJsonStr();
            DragInCanvasBehavior.CanMove = false;
            try
            {
                TraceService.InsertT_Trace(TraceLevelEnum.Important, "更改首页布局");
                System.IO.File.WriteAllText(LayoutPath, str);
            }catch(Exception ex)
            {
                Tool.AppLogError(ex);
                windowManager.ShowMessageBox(ex.Message);
            }
        }
        public void DeleteAdjust()
        {
            if(File.Exists(LayoutPath))
            File.Delete(LayoutPath);
            windowManager.ShowMessageBox("重新登录系统才能生效","系统提示");
        }
        public struct Layout
        {
            public String ID;
            public double Left;
            public double Top;
        }
        public bool CanStart { get; set; }
        /// <summary>
        /// 开机
        /// </summary>
        public void Start()
        {
            Constants.UserAbort = false;
            ResManager.getInstance().ClsRes();
            ExperimentLogic.getInstance().DelAllPackage();
            var vm = IoC.Get<FirstStepViewModel>();
            var result = windowManager.ShowDialog(vm);
            ActionManager.getInstance().removeAllActions();
            //初始化配平卡
            if (Constants.BJDict.ContainsKey("T_BJ_GelSeat"))
            {
                foreach (var cendevi in cenMrg.CentrifugeMDevices)
                {
                    var peiseat = ResManager.getInstance().GetResByCode("null", "T_BJ_GelSeat", "", "2");
                    if (peiseat != null)
                    {
                        peiseat.AddCode("pei" + cendevi.Centrifugem.Code.SetValue);
                        peiseat.Values[peiseat.CountX, peiseat.CountY] = peiseat;
                    }
                }
            }
            cameraDevice.Open();
            InitDevice();
        }
        /// <summary>
        /// 初始化设备
        /// </summary>
        /// <returns></returns>
        private bool InitDevice()
        {
            Sequence act_init = (Sequence)OPDevice.InitAllDevice();
            act_init.AddAction(SkCallBackFun.create((ActionBase act_tem) =>
            {
                CanStop = true;
                CanPause = true;
                CanNoDoubleBooldNormal = true;
                CanNoDoubleBooldCorss = true;
                CanDoDouble = true;
                return true;
            }));
            act_init.runAction(OPDevice);
            
            return true;
        }

        public bool CanStop { get; set; }
        /// <summary>
        /// 关机
        /// </summary>
        public void Stop()
        {
            SetBtnCan(true);
        }
        public bool CanPause { get; set; }
        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            Constants.PauseResetEvent.Reset();
            var model = IoC.Get<PauseViewModel>();
            var result=windowManager.ShowDialog(model);
            if(result.HasValue && !result.Value)
            {
                //用户终止
                Engine.getInstance().stop();
            }
            else
            {
                Engine.getInstance().start();
            }
        }
        /// <summary>
        /// 试剂扫码
        /// </summary>
        public void ScanAgent()
        {
            var vm = IoC.Get<BJConfigViewModel>();
            vm.Key = "T_BJ_AgentiaWarehouse";
            vm.CanSave = true;
            windowManager.ShowDialog(vm);
        }
        public bool CanDoDouble { get; set; }
        /// <summary>
        /// 双工通信
        /// </summary>
        public void DoDouble()
        {
            Constants.IsDouble = false;
            CanDoDouble = true;
            ScanSampleViewModel.is_double = true;
            this.windowManager.ShowDialog(IoC.Get<ScanSampleViewModel>());

            //CanDoDouble = false;
            //Constants.IsDouble = true;
            //Thread thread = new Thread(new ThreadStart(DoubleDetected));
            //thread.Start();

            //Constants.IsDouble = true;
            //CanDoDouble = false;
            //this.windowManager.ShowDialog(IoC.Get<DupScanSampleViewModel>());
            //HisSystem.getInstance().StartWork();

        }
        /// <summary>
        /// 无双常规血型DoDouble
        /// </summary>
        public bool CanNoDoubleBooldNormal { get; set; }
        /// <summary>
        /// 无双常规血型
        /// </summary>
        public void NoDoubleBooldNormal()
        {
            Constants.IsDouble = false;
            CanDoDouble = true;
            ScanSampleViewModel.is_double = false;
            this.windowManager.ShowDialog(IoC.Get<ScanSampleViewModel>());
        }
        public bool CanNoDoubleBooldCorss { get; set; }
        /// <summary>
        /// 无双交叉配血
        /// </summary>
        public void NoDoubleBooldCorss()
        {
            Constants.IsDouble = false;
            CanDoDouble = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val">为CanStart 的值</param>
        private void SetBtnCan(bool val)
        {
            CanStart = val;
            CanStop = !val;
            CanPause = CanStop;
            CanDoDouble = CanStop;
            CanNoDoubleBooldNormal = CanStop;
            CanNoDoubleBooldCorss = CanStop;
        }
        
        public void EidtDeepplate()
        {
            if (CanEndAdjust) return;
            var vm = IoC.Get<ShowDeepPlateViewModel>();
                vm.Key = "T_BJ_DeepPlate";
                vm.CanSave = true;
            windowManager.ShowDialog(vm);
        }
        public void OpenGelWarehouse()
        {
            if (CanEndAdjust) return;
            windowManager.ShowMessageBox("正在打开卡仓");
        }
        public void CloseAlarm()
        {
            if (CanEndAdjust) return;
            //OPDevice.SetFinishForRack(0,1,5);
            //OPDevice.Alarm(false);
        }
        public void ClosePopup(String PopupType)
        {
            switch (PopupType)
            {
                case "Tip":
                    {
                        ShowWarnTipCount = false;
                        break;
                    }
                case "Gel":
                    {
                        ShowWarnGelCount = false;
                        break;
                    }
            }
        }
        public void switchLight(object sender, System.Windows.RoutedEventArgs e)
        {
            ToggleSwitch_Control swt = sender as ToggleSwitch_Control;
            if (swt.Tag != null && swt.Tag.ToString() == "light")
            {
                OPDevice.Light(swt.IsChecked);
            }
            else {
                OPDevice.CameraLight(swt.IsChecked);
            }
        }

    }

}
