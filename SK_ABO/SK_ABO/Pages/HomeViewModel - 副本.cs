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
using SK_ABO.robot;
using SKABO.Hardware.Scaner;
using System.Text.RegularExpressions;
using SKABO.Hardware.Core;
using SK_ABO.Views.NotDuplex;
using SKABO.BLL.IServices.IUser;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Common.Views;
using SK_ABO.Views.Duplex;
using SK_ABO.MAI.HisSystem;

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
        private MachineHandDevice handDevice;
        [StyletIoC.Inject]
        private InjectorDevice injDevice;
        [StyletIoC.Inject]
        private OtherPartDevice opDevice;
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
        protected override void OnViewLoaded()
        {
            if (loaded) return;
            CanNoDoubleBooldNormal = true;

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
            TipCount = int.Parse(UserService.QuerySysConfig("TipCount").SnValue) ;

            this.handDevice.PutGELToFeiKa += HandDevice_PutGELToFeiKa;
            injDevice.OutTipEvent += InjDevice_OutTipEvent;
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
                vm.CanSave = false;
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
        public async void Start()
        {
            Constants.UserAbort = false;
            Task<bool> InitResult = InitDeviceAsync();
            var vm = IoC.Get<FirstStepViewModel>();
            var result= windowManager.ShowDialog(vm);
            if(result.HasValue && result.Value)
            {
                String Key = "T_BJ_Tip";
                if (Constants.BJDict.ContainsKey(Key))
                {
                    var list = Constants.BJDict[Key];
                    for(int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        if(item is VBJ vbj)
                        {
                            if (i < vm.TipIndex)
                            {
                                vbj.FillAll(null);
                            }else if (i > vm.TipIndex)
                            {
                                vbj.FillAll(1);
                            }
                            else
                            {
                                IoC.Get<InjectorDevice>().CurrentTipBox = vbj as T_BJ_Tip; 
                                int MaxX = vbj.Values.GetLength(0);
                                int MaxY = vbj.Values.GetLength(1);
                                for (int ii = 0; ii < MaxX; ii++)
                                {
                                    for (int jj = 0; jj < MaxY; jj++)
                                    {
                                        vbj.SetValue(ii, jj, (ii < vm.CurrentX || (ii == vm.CurrentX && jj < vm.CurrentY)) ? null : (object)1);
                                    }
                                }
                            }
                        }
                    }
                }
                String msg = null;
                bool init = false;
                try
                {
                    cmDevice.StartMixer();
                    init =await InitResult;
                    
                }catch(AggregateException exs)
                {
                    msg = exs.Message;
                }
                catch(InvalidOperationException ex)
                {
                    msg = ex.Message;
                }
                //init = true;
                SetBtnCan(!init);
                IoC.Get<InjectorDevice>().CurrentDeepPlate = Constants.BJDict[typeof(T_BJ_DeepPlate).Name].FirstOrDefault() as T_BJ_DeepPlate;
                if (init)
                {
                    windowManager.ShowMessageBox("开机成功，请确保移出台面多余的GEL卡！");
                    //SetBtnCan(false);
                }
                else
                {
                    windowManager.ShowMessageBox((String.IsNullOrEmpty(msg) && String.IsNullOrEmpty(ErrorMsg ))? "开机失败！":msg+ErrorMsg);
                    ErrorMsg = null;
                }
            }
        }
        /// <summary>
        /// 初始化设备
        /// </summary>
        /// <returns></returns>
        private bool InitDevice()
        {
            String msg = null;
            var result=OPDevice.InitAllDevice(out msg);
            Console.WriteLine("开启机器人线程");
            if (result)
            {
                TestRobot.ResetVBJ<T_BJ_GelSeat>();
                TestRobot.ResetVBJ<T_BJ_Centrifuge>();
                TestRobot.ResetVBJ<T_BJ_SampleRack>();
                TestRobot.ClearTesgBag();
                StartTestRobot();
                InitCentrifugeEvent();
            }
            else
            {
                ErrorMsg = msg;
            }
            return result;
        }
        private void InitCentrifugeEvent()
        {
            foreach(var cent in this.centDevice.Centrifuges)
            {
                cent.ChangeStatusEvent -= Cent_ChangeStatusEvent;
                cent.ChangeStatusEvent += Cent_ChangeStatusEvent;
            }
            
        }

        private void Cent_ChangeStatusEvent(SKABO.Common.Models.Communication.Unit.Centrifuge centrifuge, SKABO.Common.Models.CentrifugeStatusChangeEventArg e)
        {
            var centBj=Constants.BJDict["T_BJ_Centrifuge"].Where(bj => ((T_BJ_Centrifuge)bj).Code == e.Code).FirstOrDefault();
            var i = Constants.BJDict["T_BJ_Centrifuge"].IndexOf(centBj);
            this.View.Dispatcher.Invoke(()=> {
                var centCtrl = (this.View as HomeView).GetControl<Centrifuge_Control >("C_" + i) as Centrifuge_Control;
                if (e.StatusEnum == CentrifugeStatusEnum.Ready)
                {
                    centCtrl.LowSpeedTime.Content = $"低:{centrifuge.LowAction.KeepSpeedTime.SetValue}s";
                    centCtrl.HightSpeedTime.Content = $"高:{centrifuge.HighAction.KeepSpeedTime.SetValue}s";
                    centCtrl.Start(1);
                    centCtrl.IsLow = false;
                }
                else if (e.StatusEnum == CentrifugeStatusEnum.AddSpeed)
                {
                    centCtrl.IsLow = !centCtrl.IsLow;
                    centCtrl.Start(centCtrl.IsLow ? 2 : 3);
                }
                else if (e.StatusEnum == CentrifugeStatusEnum.KeepSpeed)
                {

                    if (centCtrl.IsLow)
                    {
                        centCtrl.LowSpeedTime.Content = $"低:{e.Time}s";
                    }
                    else
                    {
                        centCtrl.HightSpeedTime.Content = $"高:{e.Time}s";
                    }
                }
                else if (e.StatusEnum == CentrifugeStatusEnum.MinusSpeed)
                {
                    centCtrl.Start(1);
                }
                else if (e.StatusEnum == CentrifugeStatusEnum.Stop)
                {
                    centCtrl.Stop();
                }
            });
            
        }

        private async Task<bool> InitDeviceAsync()
        {
            bool result = await Task.Run(() => {
                if (InitDevice())
                {
                    try
                    {
                        String msg = null;
                        var res= gwDevice.ScanGelCards(out msg);
                        if (!res)
                        {
                            ErrorMsg += msg;
                        }
                        return res;
                    }catch(Exception ex)
                    {
                        ErrorMsg = ex.Message;
                        return false;
                    }
                }
                return false;
                });
            return result;
        }
        
        public bool CanStop { get; set; }
        /// <summary>
        /// 关机
        /// </summary>
        public void Stop()
        {
            StopTestRobot();
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
                Stop();
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
            //CanDoDouble = false;
            //Constants.IsDouble = true;
            //Thread thread = new Thread(new ThreadStart(DoubleDetected));
            //thread.Start();

            Constants.IsDouble = true;
            CanDoDouble = false;
            this.windowManager.ShowDialog(IoC.Get<DupScanSampleViewModel>());
            //HisSystem.getInstance().StartWork();

        }
        /// <summary>
        /// 无双常规血型
        /// </summary>
        public bool CanNoDoubleBooldNormal { get; set; }
        /// <summary>
        /// 无双常规血型
        /// </summary>
        public void NoDoubleBooldNormal()
        {
            Constants.IsDouble = false;
            CanDoDouble = true;
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
            OPDevice.SetColor_R(5);
            OPDevice.SetColor_G(5);
            OPDevice.SetFinishForRack(0,1,2,3,4,5);
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
        /// <summary>
        /// 开启机器人
        /// </summary>
        private void StartTestRobot()
        {
            TestRobot.IsEnabled = true;
            TestRobot.StartNextResetEvent.Set();
            for (byte i = 0; i < 2; i++)
            {
                Task task = new Task(() => CreateRobot(i).RunTest(),TaskCreationOptions.LongRunning);
                task.Start();
            }
        }
        private void StopTestRobot()
        {
            TestRobot.IsEnabled = false;
        }
        private TestRobot CreateRobot(byte taskID)
        {
            var robot = new TestRobot(taskID);
            robot.OnError += (string msg, TestRobot robot1) => {
                Robot_OnError(msg, robot1);

            };
            return robot;
        }

        private void Robot_OnError(string msg, TestRobot robot)
        {
            this.View.Dispatcher.Invoke(() =>
            {
                var win=new AlertView(OPDevice.AlarmFun);
                win.SetAlertParam("系统错误", msg, robot.RunSemaphore);
                var dig= win.ShowDialog();
                if(dig.HasValue && !dig.Value)
                {
                    TestRobot.ClearTesgBag();
                    TestRobot.ResetVBJ<T_BJ_Centrifuge>();
                    TestRobot.ResetVBJ<T_BJ_GelSeat>();
                    TestRobot.ResetVBJ<T_BJ_SampleRack>();
                }
            });

        }
        /// <summary>
        /// 双工侦测线程
        /// </summary>
        private void DoubleDetected()
        {
            lock (doubleMonitor)
            {
                TraceService.InsertT_Trace("开始双工监测");
                opDevice.OnChangeSampleRackStatus += OPDevice_OnChangeSampleRackStatus;
                while (Constants.IsDouble)
                {
                            
                    Monitor.Pulse(doubleMonitor);
                    Monitor.Wait(doubleMonitor);
                }
                opDevice.OnChangeSampleRackStatus -= OPDevice_OnChangeSampleRackStatus;
            }
        }

        private void OPDevice_OnChangeSampleRackStatus(byte[] indexs, byte eventType)
        {
            lock (doubleMonitor)
            {
                byte index = indexs[0];
                //Monitor.Wait(doubleMonitor);
                var list = Constants.BJDict[typeof(T_BJ_SampleRack).Name];
                if (index >= list.Count) return;
                T_BJ_SampleRack sampleRack = list.Where(item=>(item as T_BJ_SampleRack).Index==index+1).FirstOrDefault() as T_BJ_SampleRack;
                //样本载架离开
                if (eventType == 0)
                {
                    TraceService.InsertT_Trace($"{index}# 样本架脱离");
                    OPDevice.MoveScaner(index);
                    scanSamResult.Item1 = 0;
                    scanSamResult.Item2 = "";
                    scanDevice.SampleScaner.DataReceived -= SampleScaner_DataReceived;
                    try
                    {
                        scanDevice.OpenSampleScaner();
                    }catch(Exception ex)
                    {
                        Tool.AppLogError(ex);
                    }
                    scanDevice.SampleScaner.SampleRack = sampleRack;
                    scanDevice.SampleScaner.DataReceived += SampleScaner_DataReceived;
                }
                else if (eventType == 1)//样本载架复位
                {
                    TraceService.InsertT_Trace($"{index}# 样本架复位");
                    scanDevice.SampleScaner.DataReceived -= SampleScaner_DataReceived;
                    Monitor.Pulse(doubleMonitor);
                }
            }
        }
        private ValueTuple<Byte, String> scanSamResult=ValueTuple.Create((Byte)0,"");
        private Regex reg = new Regex(@"^\d+$");
        private void SampleScaner_DataReceived(SKABO.Ihardware.Core.AbstractScaner scaner, T_BJ_SampleRack sampleRack)
        {
            String barCode = scaner.Read();
            if (barCode.Length <= 2 && reg.IsMatch (barCode))
            {
                Byte CurIndex = Byte.Parse(barCode);

                if(Math.Abs(scanSamResult.Item1-CurIndex)==1 && !String.IsNullOrEmpty(scanSamResult.Item2))
                {
                    sampleRack.SetValue(Math.Min(scanSamResult.Item1, CurIndex), 0, scanSamResult.Item2);
                    scanSamResult.Item2 = null;
                }
                scanSamResult.Item1 = CurIndex;
            }
            else
            {
                scanSamResult.Item2 = barCode;
            }
        }
    }

}
