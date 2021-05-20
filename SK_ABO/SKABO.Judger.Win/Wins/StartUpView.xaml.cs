using Microsoft.Win32;
using SKABO.BLL.IServices.IJudger;
using SKABO.BLL.Services.Judger;
using SKABO.Camera;
using SKABO.Common.Models.Judger;
using SKABO.Common.Utils;
using SKABO.Judger.Win.Resize;
using SKABO.Judger.Win.UserCtrls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Drawing.Imaging;

namespace SKABO.Judger.Win.Wins
{
    unsafe public partial class AiInter
    {
        [DllImport("ainumbertorch/AiNumberTorch.dll", EntryPoint = "LoadModel")]
        public static extern bool LoadModel(int id, String filename);
        [DllImport("ainumbertorch/AiNumberTorch.dll", EntryPoint = "RunModel", CharSet = CharSet.None, CallingConvention = CallingConvention.Cdecl)]
        public static extern int RunModel(int id, byte[] filename, byte[] ans);
        [DllImport("ainumbertorch/AiNumberTorch.dll", EntryPoint = "GetNumber", CharSet = CharSet.None, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumber(int id, byte[] filename, byte[] ans);
    }

    public class BindBox
    {
        public double x;
        public double y;
        public double w;
        public double h;
        public double score;
    }
    /// <summary>
    /// StartUpView.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class StartUpView : Window
    {
        private Judger.Core.Judger judger;
        private T_ParseTubeParameter _PTP;
        private T_ParseTubeParameter PTP
        {
            get
            {
                if (_PTP == null)
                {
                    _PTP = judgerParamerService.LoadPTP();
                    if (_PTP == null)
                    {
                        _PTP = new T_ParseTubeParameter()
                        {
                            TSpace = 189.4m,
                            BottomHeight = 15,
                            TestWidth = 60,
                            Threshold = 160,
                            Angle = 60,
                            HueMaxThreshold = 315,
                            HueMinThreshold = 23,
                            SMaxThreshold = 0.6m,
                            SMinThreshold=0.2m,
                            BMaxThreshold=0.7m,
                            BMinThreshold=0.05m
                            
                        };
                    }
                }
                return _PTP;
            }
        }
        private T_ParseLEDParameter _PLP;
        private T_ParseLEDParameter PLP
        {
            get
            {
                if (_PLP == null)
                {
                    _PLP = judgerParamerService.LoadPLP();
                    if (_PLP == null)
                    {
                        _PLP = new T_ParseLEDParameter()
                        {
                            DSpace = 248,
                            LEDBrushWidth = 29,
                            LEDThreshold = 25
                        };
                    }
                }
                return _PLP;
            }
        }
        public T_Camera t_Camera
        {
            get
            {
                if (_t_Camera == null)
                {
                    _t_Camera = judgerParamerService.QueryCamera(Constant.MSN);

                }
                return _t_Camera;
            }
            set => _t_Camera = value;
        }

        private T_Camera _t_Camera;
        private ICameraDevice _camera;
        private IJudgerParamerService _judgerParamerService;
        private IJudgerParamerService judgerParamerService
        {
            get
            {
                if (_judgerParamerService == null)
                {
                    _judgerParamerService = new JudgerParamerService(Constant.EntityMapper);
                }
                return _judgerParamerService;
            }
        }

        public ICameraDevice camera { get {
                if (_camera == null)
                {
                    _camera = CameraFactory.CreateCamera();
                }
                if (_camera == null)
                {
                    MessageBox.Show("创建相机实例失败！");
                    
                }
                return _camera;
            }  }

        private IList<T_JudgeParamer> _TJList;
        private IList<T_JudgeParamer> TJList
        {
            get
            {
                if (_TJList == null)
                {
                    _TJList = LoadByMSN(Constant.MSN);
                }
                if (_TJList == null)
                {
                    _TJList = new List<T_JudgeParamer>();
                }
                return _TJList;
            }
        }

        private bool IsInitTube;
        private Bitmap OrgBmp;
        /// <summary>
        /// 调整大小的控件与被调整大小控件的高度差
        /// </summary>
        private const double HeightDiff = 40;
        /// <summary>
        /// 图片的实际大小
        /// </summary>
        private double ImageWidth = 0, ImageHeight = 0;
        /// <summary>
        /// 窗口改变大小前Image控件大小
        /// </summary>
        private double BeforeWidth = 0, BeforeHeight = 0;
        public StartUpView()
        {
            bool createdNew = false;
            var run = new System.Threading.Mutex(true, "SKABO.Judger.Win", out createdNew);
            App.Current.Properties["Mutex"] = run;
            if (!createdNew)
            {
                MessageBox.Show("程序已经运行!", "系统提示");
                App.Current.Shutdown();
            }
            else
            {
                InitializeComponent();
                InitPLPView();
                InitPTPView();
                string path = System.Windows.Forms.Application.StartupPath.Replace("\\", "//");
                AiInter.LoadModel(0, path + "//ainumbertorch//ainumber.pt");
            }
            
        }
        private void InitPLPView()
        {
            Txt_DSpace.Text = PLP.DSpace.ToString();
            Txt_LEDHo.Text = PLP.LEDThreshold.ToString();
            Txt_BrushW.Text = PLP.LEDBrushWidth.ToString();
        }
        private void InitPTPView()
        {
            Txt_TSpace.Text = PTP.TSpace.ToString();
            Txt_BottomHeight.Text = PTP.BottomHeight.ToString();
            Txt_TestWidth.Text = PTP.TestWidth.ToString();
            Txt_Threshold.Text = PTP.Threshold.ToString();
            Txt_Angle.Text = PTP.Angle.ToString();
            Txt_HueMax.Text = PTP.HueMaxThreshold.ToString();
            Txt_HueMin.Text = PTP.HueMinThreshold.ToString();
            Txt_SMax.Text = PTP.SMaxThreshold.ToString();
            Txt_SMin.Text = PTP.SMinThreshold.ToString();
            Txt_BMax.Text = PTP.BMaxThreshold.ToString();
            Txt_BMin.Text = PTP.BMinThreshold.ToString();

        }
        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "请选择文件夹";
            dialog.Filter = "图片文件(*.jpg)|*.jpg";
            if (dialog.ShowDialog() == true)
            {
                var bm =new Bitmap(dialog.FileName);
                InitImageSource(bm);

            }
        }
        
        private void InitImageSource(Bitmap bm)
        {
            OrgBmp = bm;
            ImageWidth = bm.Width;
            ImageHeight = bm.Height;
            this.TargetImg.Source = ImgUtil.BitmapToBitmapSource(bm);
            Canvas1.UpdateLayout();
            InitTubes();
        }
        /// <summary>
        /// 订阅位置改变事件
        /// </summary>
        private void SubscribeThumbPositionChanged()
        {
            for (int i = 1; i < 9; i++)
            {
                var t = this.FindName("TubeT" + i) as TubeThumb;

                t.PositionChanged += T_PositionChanged;
            }
            for (int i = 1; i < 4; i++)
            {
                var t = this.FindName("TubeD" + i) as TubeThumb;
                t.PositionChanged += T_PositionChanged;
            }
        }

        private void T_PositionChanged(object sender, EventArgs e)
        {
            var t = sender as TubeThumb;
            if (t == null) return;
            MoveRect(t);
            UpdateCoordinate(t);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SubscribeThumbPositionChanged();
            ResizeImg();
            UpdateAllThumb(true);

            var resizer = new ControlResizer(Rect1, new Thickness(10), 15, null);
            //设置事件
            resizer.Resize += resizer_Resize;
        }
        void resizer_Resize(object sender, ControlResizeEventArgs e)
        {
            
            var ResizerObj = (sender as ControlResizer).Control;
            //左右拉伸
            if (e.LeftDirection.HasValue)
            {
                var value = ResizerObj.Width + e.HorizontalChange;
                if (value > ResizerObj.MinWidth)
                {
                    if (e.LeftDirection.Value)
                        Canvas.SetLeft(ResizerObj, Canvas.GetLeft(ResizerObj) - e.HorizontalChange);
                    ResizerObj.Width = value;
                }
            }
            //上下拉伸
            if (e.TopDirection.HasValue)
            {
                var value = ResizerObj.Height + e.VerticalChange;
                if (value > ResizerObj.MinHeight)
                {
                    if (e.TopDirection.Value)
                        Canvas.SetTop(ResizerObj, Canvas.GetTop(ResizerObj) - e.VerticalChange);
                    ResizerObj.Height = value;
                }
            }
            //调整被遮盖的控件大小
            var MyThumb = ResizerObj.Tag as Thumb;
            Canvas.SetLeft(MyThumb, Canvas.GetLeft(ResizerObj));
            Canvas.SetTop(MyThumb, Canvas.GetTop(ResizerObj)- HeightDiff);
            MyThumb.Width = ResizerObj.Width;
            MyThumb.Height = ResizerObj.Height + HeightDiff;
            UpdateCoordinate(MyThumb);
        }
        /// <summary>
        /// 根据显示位置，刷新图像中的位置
        /// </summary>
        /// <param name="UpdateType"></param>
        private void UpdateAllThumb(bool UpdateType)
        {
            for (int i = 1; i < 9; i++)
            {
                var t = this.FindName("TubeT" + i) as Thumb;
                if (UpdateType)
                {
                    (t.Template.FindName("Lab_Type", t) as Label).Content = "Type:T No:" + i;
                }
                UpdateCoordinate(t);
            }
            for (int i = 1; i < 4; i++)
            {
                var t = this.FindName("TubeD" + i) as Thumb;
                if (UpdateType)
                    (t.Template.FindName("Lab_Type", t) as Label).Content = "Type:D No:" + i;
                UpdateCoordinate(t);
            }
        }
        /// <summary>
        /// 根据图像中位置，刷新显示位置
        /// </summary>
        /// <param name="adjustSize">是否调整控件大小</param>
        private void UpdateAllThumbPoint()
        {
            for (int i = 1; i < 9; i++)
            {
                var t = this.FindName("TubeT" + i) as Thumb;

                UpdateMargin(t);
            }
            for (int i = 1; i < 4; i++)
            {
                var t = this.FindName("TubeD" + i) as Thumb;
                UpdateMargin(t);
            }
        }
        private void ResizeImg()
        {
            BeforeWidth = TargetImg.ActualWidth;
            BeforeHeight = TargetImg.ActualHeight;
            TargetImg.Width = this.Canvas1.ActualWidth;
            TargetImg.Height = this.Canvas1.ActualHeight;
            Canvas1.UpdateLayout();
            //TubeThumb
        }
        private void MoveRect(FrameworkElement FE)
        {
            Canvas.SetLeft(Rect1, Canvas.GetLeft(FE));
            Canvas.SetTop(Rect1, Canvas.GetTop(FE) + HeightDiff);
        }
        /// <summary>
        /// 更新Tube的位置换算出坐标值
        /// </summary>
        /// <param name="MyThumb"></param>
        private void UpdateCoordinate(Thumb MyThumb)
        {
            TubeThumb T = MyThumb as TubeThumb;
            T.X = Convert.ToInt32(ImageWidth == 0 ? Canvas.GetLeft(MyThumb) : (Canvas.GetLeft(MyThumb) / TargetImg.ActualWidth * ImageWidth));
            T.Y = Convert.ToInt32(ImageHeight == 0 ? Canvas.GetTop(MyThumb) + HeightDiff : ((Canvas.GetTop(MyThumb) + HeightDiff) / TargetImg.ActualHeight * ImageHeight));
            if (!Double.IsNaN(T.Width)) { 
            T.EndX = T.X + Convert.ToInt32(ImageWidth == 0 ? T.Width : (T.Width / TargetImg.ActualWidth * ImageWidth));
            T.EndY = T.Y + Convert.ToInt32(ImageHeight == 0 ? T.Height - HeightDiff : (T.Height - HeightDiff) / TargetImg.ActualHeight * ImageHeight);
                }
            (MyThumb.Template.FindName("Lab_Coordinate", MyThumb) as Label).Content = String.Format("X:{0} Y:{1}",T.X , T.Y);
            T.ToolTip = T.TipToolValue;
        }
        /// <summary>
        /// 根据图像中位置，刷新显示位置
        /// </summary>
        /// <param name="MyThumb"></param>
        private void UpdateMargin(Thumb MyThumb)
        {
            TubeThumb T = MyThumb as TubeThumb;
            T.MarginX = Math.Round(ImageWidth == 0 ? Canvas.GetLeft(MyThumb) : ( T.X / ImageWidth * TargetImg.ActualWidth), 3);
            T.MarginY = Math.Round(ImageHeight == 0 ? Canvas.GetTop(MyThumb) : (T.Y  / ImageHeight * TargetImg.ActualHeight- HeightDiff), 3);
            Canvas.SetLeft(MyThumb, T.MarginX);
            Canvas.SetTop(MyThumb, T.MarginY);
            if (!IsInitTube)
            {
                MyThumb.Width = (T.EndX-T.X) / ImageWidth * TargetImg.ActualWidth;
                MyThumb.Height = (T.EndY - T.Y) / ImageHeight * TargetImg.ActualHeight + HeightDiff;
                (MyThumb.Template.FindName("Lab_Coordinate", MyThumb) as Label).Content = String.Format("X:{0} Y:{1}", T.X, T.Y);
                T.ToolTip = T.TipToolValue;
            }
            else if (BeforeWidth != 0)
            {
                MyThumb.Width = MyThumb.Width / BeforeWidth * TargetImg.ActualWidth;
                MyThumb.Height = (MyThumb.Height-HeightDiff) / BeforeHeight * TargetImg.ActualHeight+ HeightDiff;
            }
        }
        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            ResizeImg();
            UpdateAllThumbPoint();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeImg();
            UpdateAllThumbPoint();
        }

        private void Rectangle_DragEnter(object sender, DragEventArgs e)
        {
            MessageBox.Show("Rectangle_DragEnter");
        }

        private void Rectangle_DragLeave(object sender, DragEventArgs e)
        {
            MessageBox.Show("Rectangle_DragLeave");
        }

        void onDragDelta(object sender, DragDeltaEventArgs e)
        {
            TubeThumb MyThumb = sender as TubeThumb;
            MyThumb.ChangePosition(Canvas.GetLeft(MyThumb) + e.HorizontalChange, Canvas.GetTop(MyThumb) + e.VerticalChange);
        }

        private void myThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            Thumb MyThumb = sender as Thumb;
            MyThumb.Background = System.Windows.Media.Brushes.Orange;
        }

        private void myThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Thumb MyThumb = sender as Thumb;
            MyThumb.Background = System.Windows.Media.Brushes.Blue;
            
        }

        /// <summary>
        /// 图像选择按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MenuOpen_Click(null, null);
        }
        /// <summary>
        /// 根据第一管位置绘制测试区其它取图范围
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_AdjustCoor_T_Click(object sender, RoutedEventArgs e)
        {
            if (ImageWidth == 0) return;
            double space = 0;

            if(!Double.TryParse(Txt_TSpace.Text,out space))
            {
                space = 189.4;
                Txt_TSpace.Text = space.ToString();
            }
            space = space / ImageWidth * TargetImg.ActualWidth;
            double X = Canvas.GetLeft(TubeT1);
            double Y = Canvas.GetTop(TubeT1);
            for (int i = 2; i < 9; i++)
            {
                var t = this.FindName("TubeT" + i) as Thumb;
                t.Width = TubeT1.Width;
                t.Height = TubeT1.Height;
                Canvas.SetLeft(t, X + (i - 1) * space);
                Canvas.SetTop(t, Y);
            }
            UpdateAllThumb(false);
        }
        /// <summary>
        /// 智能绘制管位置测试区取图范围
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_AI_T_Click(object sender, RoutedEventArgs e)
        {
            return;
            if(OrgBmp!=null)
            {
                string path = System.Windows.Forms.Application.StartupPath.Replace("\\", "//");
                path = path + "//ainumber" + "/bbox.jpg";
                OrgBmp.Save(path);
                byte[] str = Encoding.ASCII.GetBytes(path);
                byte[] ans = new byte[1024];
                AiInter.RunModel(1, str, ans);
                string result = Encoding.ASCII.GetString(ans).TrimEnd('\0');
                if (result.Length > 5)
                {
                    List<BindBox> bboxs = JsonConvert.DeserializeObject<List<BindBox>>(result);
                    double mesny = 0;
                    double mesnh = 0;
                    for (int i = 0; i < bboxs.Count; i++)
                    {
                        bboxs[i].x = (bboxs[i].x * 4-20) * (TargetImg.ActualWidth / ImageWidth);
                        bboxs[i].y = (bboxs[i].y * 4+70) * (TargetImg.ActualHeight / ImageHeight) - HeightDiff;
                        bboxs[i].w = (bboxs[i].w * 4+40) * (TargetImg.ActualWidth / ImageWidth);
                        bboxs[i].h = (bboxs[i].h * 4-80) * (TargetImg.ActualHeight / ImageHeight) + HeightDiff;
                        mesny += bboxs[i].y;
                        mesnh += bboxs[i].h;
                    }
                    mesny = mesny / bboxs.Count;
                    mesnh = mesnh / bboxs.Count;

                    bboxs.Sort(delegate (BindBox a, BindBox b)
                    {
                        int xdiff = a.x.CompareTo(b.x);
                        if (xdiff != 0) return xdiff;
                        else return a.y.CompareTo(b.y);
                    });
                    for (int i = 0; i < bboxs.Count; i++)
                    {
                        if (i >=8) break;
                        var t = this.FindName("TubeT" + (i+1)) as Thumb;
                        t.Width = bboxs[i].w;
                        t.Height = mesnh;
                        Canvas.SetLeft(t, bboxs[i].x);
                        Canvas.SetTop(t, mesny);
                    }
                    UpdateAllThumb(false);
                }
            }
        }
        /// <summary>
        /// 根据第一管位置绘制核对区其它取图范围
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_AdjustCoor_D_Click(object sender, RoutedEventArgs e)
        {
            if (ImageWidth == 0) return;
            double space = 0;

            if (!Double.TryParse(Txt_DSpace.Text, out space))
            {
                space = 248;
                Txt_DSpace.Text = space.ToString();
            }
            space = space / ImageHeight * TargetImg.ActualHeight;
            double X = Canvas.GetLeft(TubeD1);
            double Y = Canvas.GetTop(TubeD1);
            for (int i = 2; i < 4; i++)
            {
                var t = this.FindName("TubeD" + i) as Thumb;
                t.Width = TubeD1.Width;
                t.Height = TubeD1.Height;
                Canvas.SetLeft(t, X);
                Canvas.SetTop(t, Y + (i - 1) * space);
            }
            UpdateAllThumb(false);

            //var kk = ImgUtil.Cut(OrgBmp, new System.Drawing.Point(TubeT1.X, TubeT1.Y), TubeT1.EndX - TubeT1.X, TubeT1.EndY - TubeT1.Y);
            //kk = ImgUtil.OtsuThreshold(kk);
            //kk.Save(@"E:\kk.jpg");
            //kk.Dispose();

        }
        /// <summary>
        /// 相机实时调参
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Adjust_Click(object sender, RoutedEventArgs e)
        {
            DaHengAdjustWin win = new DaHengAdjustWin();
            win.UpdatedT_Camera += Win_UpdatedT_Camera;
            win.ShowDialogEx();
        }

        private void Win_UpdatedT_Camera(object sender, EventArgs e)
        {
            T_Camera t = sender as T_Camera;
            this.t_Camera = t;
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void TubeT1_MouseEnter(object sender, MouseEventArgs e)
        {
            Thumb MyThumb = sender as Thumb;
            MyThumb.Cursor = Cursors.SizeAll;
            MoveRect(MyThumb);
            Rect1.Width = MyThumb.ActualWidth;
            Rect1.Height = MyThumb.ActualHeight- HeightDiff;
            Rect1.Tag = MyThumb;
        }

        private void TubeT1_MouseLeave(object sender, MouseEventArgs e)
        {
            Thumb MyThumb = sender as Thumb;
            MyThumb.Cursor = Cursors.Arrow;
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            
            String MSN = Constant.MSN;
            for (int i = 1; i < 9; i++)
            {
                var t = this.FindName("TubeT" + i) as TubeThumb;

                T_JudgeParamer TJ = (TJList.Where<T_JudgeParamer>(c => c.AreaType == "T" && c.TNo == i).FirstOrDefault());
                if (TJ == null)
                {
                    TJ = new T_JudgeParamer();
                    TJList.Add(TJ);
                    
                }
                TJ.AreaType = "T";
                TJ.TNo = i;
                TJ.MSN = MSN;
                TJ.StartX = t.X;
                TJ.StartY = t.Y;
                TJ.EndX = t.EndX;
                TJ.EndY = t.EndY;

            }
            for (int i = 1; i < 4; i++)
            {
                var t = this.FindName("TubeD" + i) as TubeThumb;
                T_JudgeParamer TJ = TJList.Where<T_JudgeParamer>(c => c.AreaType == "D" && c.TNo == i).FirstOrDefault();
                if (TJ == null)
                {
                    TJ = new T_JudgeParamer();
                    TJList.Add(TJ);
                    
                }
                TJ.AreaType = "D";
                TJ.TNo = i;
                TJ.MSN = MSN;
                TJ.StartX = t.X;
                TJ.StartY = t.Y;
                TJ.EndX = t.EndX;
                TJ.EndY = t.EndY;
            }
            judgerParamerService.UpdateT_JudgeParamer(TJList);
            
            
            int BrushW = 0;
            byte Ho = 0;//
            decimal val = 0;
            if (!int.TryParse(Txt_BrushW.Text, out BrushW))
            {
                BrushW = 29;
            }
            if (!byte.TryParse(Txt_LEDHo.Text, out Ho))
            {
                Ho = 29;
            }
            if(!decimal.TryParse(Txt_DSpace.Text, out val))
            {
                val = 248;
            }
            PLP.DSpace = val;
            PLP.LEDBrushWidth = BrushW;
            PLP.LEDThreshold = Ho;
            judgerParamerService.SavePLP(PLP);

            UpdatePTP();
            judgerParamerService.SavePTP(PTP);
        }
        /// <summary>
        /// 识别LED
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_CheckLed_D_Click(object sender, RoutedEventArgs e)
        {
            int BrushW = 0;
            byte Ho = 0;//
            if (!int.TryParse(Txt_BrushW.Text,out BrushW)){
                BrushW = 29;
            }
            if (!byte.TryParse(Txt_LEDHo.Text, out Ho))
            {
                Ho = 29;
            }
            var TJP = TJList.Where<T_JudgeParamer>(c => c.AreaType == "D");
            int[] vals;
            Lab_LED.Content = ImgUtil.AnalsyAreaDigitalTube(OrgBmp, TJP, Ho, BrushW,out vals);

            for (int i=0;i< TJP.Count(); i++)
            {
                var p = TJP.ElementAt(i);
                UpdateImage(i, vals[i].ToString(),ImgUtil.Threshoding(ImgUtil.Cut(OrgBmp,new System.Drawing.Point(p.StartX,p.StartY),p.EndX-p.StartX,p.EndY-p.StartY), Ho), null);
            }
        }

        public static void ClsBlackBitmap(Bitmap bm,bool isdown)
        {
            int width = bm.Width;
            int height = bm.Height;
            BitmapData data = bm.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* p = (byte*)data.Scan0;
                int scanWidth = width * 4;
                int cutcount = 0;
                int cutscanindex = 0;
                bool iscls = false;
                for (int y = 0; y < height; y++)
                {
                    bool isblackline = true;
                    for (int x = 0; x < width; x++)
                    {
                        if (p[x*4] == 255)
                        {
                            isblackline = false;
                            cutcount = 0;
                            break;
                        }
                    }
                    if (isblackline) cutcount++;
                    if(cutcount>50)
                    {
                        cutscanindex = y;
                        iscls = true;
                        break;
                    }
                    p += scanWidth;
                }
                if(iscls)
                {
                    if(isdown)
                    {
                        p = (byte*)data.Scan0;
                        for (int y = 0; y < cutscanindex; y++)
                        {
                            for (int x = 0; x < width * 4; x++)
                            {
                                p[x] = 0;
                            }
                            p += scanWidth;
                        }
                    }
                    else
                    {
                        p = (byte*)data.Scan0 + scanWidth * cutscanindex;
                        for (int y = cutscanindex; y < height; y++)
                        {
                            for (int x = 0; x < width * 4; x++)
                            {
                                p[x] = 0;
                            }
                            p += scanWidth;
                        }
                    }
                }
            }
            bm.UnlockBits(data);
            //bm.Dispose();
        }

        private void Btn_CheckLed_T_Click(object sender, RoutedEventArgs e)
        {
            int BrushW = 0;
            byte Ho = 0;//
            if (!int.TryParse(Txt_BrushW.Text, out BrushW))
            {
                BrushW = 29;
            }
            if (!byte.TryParse(Txt_LEDHo.Text, out Ho))
            {
                Ho = 29;
            }
            var TJP = TJList.Where<T_JudgeParamer>(c => c.AreaType == "D");
            int[] vals = {0,0,0};
            string path = System.Windows.Forms.Application.StartupPath.Replace("\\", "//");
            path = path + "//ainumbertorch";
            Lab_LED.Content = "";
            bool[] isdowns = { false, true, true };
            for (int i = 0; i < TJP.Count(); i++)
            {
                var p = TJP.ElementAt(i);
                var image = ImgUtil.Threshoding(ImgUtil.Cut(OrgBmp, new System.Drawing.Point(p.StartX, p.StartY), p.EndX - p.StartX, p.EndY - p.StartY), Ho);
                ClsBlackBitmap(image, isdowns[i]);
                image.Save(path+"/test.jpg");


                TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                image.Save(path + "/test"+i + Convert.ToInt64(ts.TotalSeconds).ToString() + ".jpg");
                byte[] str = Encoding.ASCII.GetBytes(path + "/test.jpg");
                byte[] ans = new byte[1024];
                AiInter.GetNumber(0, str, ans);
                string retstr = Encoding.ASCII.GetString(ans);
                int ret = int.Parse(retstr);
                vals[i] = ret;
                Lab_LED.Content += vals[i].ToString();
                UpdateImage(i, vals[i].ToString(), image, null);
            }
           


        }

        private void UpdatePTP()
        {
            Decimal tempD = 0;
            if (Decimal.TryParse(Txt_TSpace.Text, out tempD))
            {
                PTP.TSpace = tempD;
            }
            if (Decimal.TryParse(Txt_HueMax.Text, out tempD))
            {
                PTP.HueMaxThreshold = tempD;
            }
            if (Decimal.TryParse(Txt_HueMin.Text, out tempD))
            {
                PTP.HueMinThreshold = tempD;
            }

            if (Decimal.TryParse(Txt_SMax.Text, out tempD))
            {
                PTP.SMaxThreshold = tempD;
            }
            if (Decimal.TryParse(Txt_SMin.Text, out tempD))
            {
                PTP.SMinThreshold = tempD;
            }

            if (Decimal.TryParse(Txt_BMax.Text, out tempD))
            {
                PTP.BMaxThreshold = tempD;
            }
            if (Decimal.TryParse(Txt_BMin.Text, out tempD))
            {
                PTP.BMinThreshold = tempD;
            }


            int tempI = 0;
            if (Int32.TryParse(Txt_BottomHeight.Text, out tempI))
            {
                PTP.BottomHeight = tempI;
            }

            if (Int32.TryParse(Txt_TestWidth.Text, out tempI))
            {
                PTP.TestWidth = tempI;
            }
            byte tempB = 0;
            if (Byte.TryParse(Txt_Threshold.Text, out tempB))
            {
                PTP.Threshold = tempB;
            }

            if (Byte.TryParse(Txt_Angle.Text, out tempB))
            {
                PTP.Angle = tempB;
            }
        }
        private void Btn_Test_T_Click(object sender, RoutedEventArgs e)
        {
            //PTP = new T_ParseTubeParameter();
            UpdatePTP();
            try
            {
                this.Dispatcher.Invoke(new Action(()=>{ UpdateInfo(null); }));
                var list = TJList.Where(c => c.AreaType == "T");
                if (judger != null) judger.Dispose();
                judger = new Core.Judger(OrgBmp, !Chk_Color.IsChecked.Value, false);
                {
                    var res = judger.Judge(list, PTP);
                    
                    for (int i = 0; i < res.Count(); i++)
                    {
                        UpdateImage(i, res[i].GetDescription(), judger.Tubes[i].Bm, judger);
                    }
                    this.Dispatcher.Invoke(new Action(() => { UpdateInfo(judger); }));
                }
            }catch(Exception ex)
            {
                Tool.AppLogError(ex);
                MessageBox.Show(ex.Message);
            }
            
        }
        private void UpdateImage(int i,String labContent,Bitmap source, Judger.Core.Judger j)
        {
            var obj = Grid_Content.GetControl<System.Windows.Controls.Image>("img" + i);
            var labObj = Grid_Content.GetControl<System.Windows.Controls.Label>("labRes" + i);
            var TubeLayerViewerObj = Grid_Content.GetControl<SKABO.Common.UserCtrls.TubeLayerViewer_Control>("TV" + i);

            System.Windows.Controls.Image img1 = obj ?? new System.Windows.Controls.Image() ;
            System.Windows.Controls.Label labRes = labObj ?? new System.Windows.Controls.Label() ;

            SKABO.Common.UserCtrls.TubeLayerViewer_Control TubeLayerViewer = null;
                if (PTP != null) {
                TubeLayerViewer = TubeLayerViewerObj ?? new SKABO.Common.UserCtrls.TubeLayerViewer_Control(new int[] { PTP.Layer1, PTP.Layer2, PTP.Layer3, PTP.Layer4, PTP.Layer5, PTP.Layer6 }) ;
            }
            if (obj == null)
            {
                img1.Name = "img" + i;
                img1.HorizontalAlignment = HorizontalAlignment.Left;
                img1.VerticalAlignment = VerticalAlignment.Top;
                img1.SetValue(Grid.RowProperty, 1);
                img1.Margin = new Thickness(100 * i, 0, 0, 0);
                Grid_Content.Children.Add(img1);
            }
            if (labObj == null)
            {
                labRes.Name = "labRes" + i;
                labRes.HorizontalAlignment = HorizontalAlignment.Left;
                labRes.VerticalAlignment = VerticalAlignment.Top;
                labRes.HorizontalContentAlignment = HorizontalAlignment.Center;
                labRes.SetValue(Grid.RowProperty, 0);
                labRes.Margin = new Thickness(100 * i + 30, 0, 0, 0);
                labRes.Foreground = System.Windows.Media.Brushes.Blue;
                Grid_Content.Children.Add(labRes);
            }
            if (TubeLayerViewerObj == null && TubeLayerViewer!=null)
            {
                TubeLayerViewer.Name = "TV" + i;
                TubeLayerViewer.HorizontalAlignment = HorizontalAlignment.Left;
                TubeLayerViewer.VerticalAlignment = VerticalAlignment.Top;
                TubeLayerViewer.SetValue(Grid.RowProperty, 1);
                TubeLayerViewer.Margin = new Thickness(100 * i, 0, 0, 0);
                Grid_Content.Children.Add(TubeLayerViewer);

            }
            labRes.Content = labContent;
            img1.Source = ImgUtil.BitmapToBitmapSource(source);
            Grid_Content.UpdateLayout();
            if (j != null && TubeLayerViewer!=null)
            {
                TubeLayerViewer.Visibility = Visibility.Visible;
                TubeLayerViewer.Width = img1.ActualWidth;
                if(judger.Tubes[i].MaxY!=0)
                TubeLayerViewer.Height = (judger.Tubes[i].MaxY - PTP.BottomHeight) * (img1.ActualHeight / judger.Tubes[i].Height);
            }
            else if(TubeLayerViewer!=null)
            {
                TubeLayerViewer.Visibility = Visibility.Hidden;
            }
        }
        private void UpdateInfo(Judger.Core.Judger judger)
        {
            if (judger == null)
            {
                TxtB_Info.Text = "";
            }
            else
            {
                TxtB_Info.Text += String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", "序号", "面积1", "面积2", "面积3",  "面积4", "面积5", "面积6", "结果");
                for (int i = 0; i < judger.Tubes.Length; i++)
                {
                    var t = judger.Tubes[i];
                    TxtB_Info.Text += Environment.NewLine;
                    TxtB_Info.Text += String.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", i+1, t.c1, t.c2, t.c3, t.c4, t.c5, t.c6,t.Result.GetDescription());

                }
            }
        }

        private void MenuCapture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (camera == null)
                {
                    return;
                }
                if (camera.Open())
                {
                    if (t_Camera != null)
                    {
                        camera.SetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_BLUE, Convert.ToDouble(t_Camera.BB));
                        camera.SetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_GREEN, Convert.ToDouble(t_Camera.GB));
                        camera.SetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_RED, Convert.ToDouble(t_Camera.RB));
                        camera.SetExposureTime(t_Camera.ExposureTime);
                        camera.SetGain(t_Camera.Gain);
                    }
                    camera.Play();
                    Camera_BitmapGot(null, new BitmapGotEventArgs(camera.GetBitmap()));
                }
                else
                {
                    MessageBox.Show("打开相机失败！");
                }
            }catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            
        }

        private void MenuSaveImg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (camera == null)
                {
                    return;
                }
                if (camera.Open())
                {
                    if (t_Camera != null)
                    {
                        camera.SetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_BLUE, Convert.ToDouble(t_Camera.BB));
                        camera.SetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_GREEN, Convert.ToDouble(t_Camera.GB));
                        camera.SetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_RED, Convert.ToDouble(t_Camera.RB));
                        camera.SetExposureTime(t_Camera.ExposureTime);
                        camera.SetGain(t_Camera.Gain);
                    }
                    camera.Play();
                    var img = camera.GetBitmap();
                    Camera_BitmapGot(null, new BitmapGotEventArgs(img));

                    string localFilePath = "";
                    //string localFilePath, fileNameExt, newFileName, FilePath; 
                    System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                    //设置文件类型 
                    sfd.Filter = "jpg图片（*.jpg）|*.jpg";

                    //设置默认文件类型显示顺序 
                    sfd.FilterIndex = 1;

                    //保存对话框是否记忆上次打开的目录 
                    sfd.RestoreDirectory = true;


                    System.DateTime datetime = System.DateTime.Now;
                    string timestr = string.Format("{0:0000}-{1:00}-{2:00}-{3:00}-{4:00}-{5:00}", datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, datetime.Second);

                    sfd.FileName = timestr;

                    //点了保存按钮进入 
                    if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        localFilePath = sfd.FileName.ToString(); //获得文件路径 
                        img.Save(localFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }

                   
                }
                else
                {
                    MessageBox.Show("打开相机失败！");
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        }
        
        private void Camera_BitmapGot(object sender, BitmapGotEventArgs e)
        {
            var bm = e.bitmap;
            if (bm != null)
                try
                {
                    TargetImg.Dispatcher.Invoke(new Action(() => {
                        InitImageSource(bm);
                        
                    }));
                    
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            //Camera.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (OrgBmp != null)
            {
                this.OrgBmp.Dispose();
                this.OrgBmp = null;
            }
            try
            {
                if (this.camera != null)
                {
                    this.camera.Close();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cmb_Resolution_Initialized(object sender, EventArgs e)
        {
            if (camera == null) return;
            var cmb = sender as ComboBox;
            cmb.ItemsSource = camera.SupportResolution();
            cmb.SelectedIndex = 0;
        }

        private void Btn_Show2_T_Click(object sender, RoutedEventArgs e)
        {
            if (judger == null) return;
           

                
                for (int i = 0; i < judger.Tubes.Length; i++)
                {
                    var obj = Grid_Content.GetControl<System.Windows.Controls.Image>("img" + i);
                    
                    System.Windows.Controls.Image img1 = obj ?? new System.Windows.Controls.Image() ;

                if (obj == null) continue;
                    img1.Source = ImgUtil.BitmapToBitmapSource(ImgUtil.Threshoding(judger.Tubes[i].Bm,PTP.Threshold));
                    Grid_Content.UpdateLayout();
                }
        }

        private IList<T_JudgeParamer> LoadByMSN(String MSN)
        {
            return judgerParamerService.QueryALlParamerByMSN(MSN);
        }
        bool Inited = false;
        private void InitTubes()
        {
            if (Inited)
            {
                UpdateAllThumb(false);
                return;
            }
            Inited = true;
            if (TJList.Count == 0) return;
            foreach(var item in TJList)
            {
                TubeThumb T = this.FindName(String.Format("Tube{0}{1}", item.AreaType, item.TNo)) as TubeThumb;
                if (T == null) continue;
                T.X = item.StartX;
                T.Y = item.StartY;
                T.EndX = item.EndX;
                T.EndY = item.EndY;
            }
            UpdateAllThumbPoint();
            IsInitTube = true;
        }
    }


}
