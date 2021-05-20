using IBatisNet.DataMapper;
using SKABO.BLL.IServices.IJudger;
using SKABO.BLL.Services.Judger;
using SKABO.Camera;
using SKABO.Common.Models.Judger;
using SKABO.Common.UserCtrls;
using SKABO.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SKABO.Judger.Win
{
    /// <summary>
    /// DaHengAdjustWin.xaml 的交互逻辑
    /// </summary>
    public partial class DaHengAdjustWin : Window
    {
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
        public delegate void UpdatedT_CameraHandle(Object sender, EventArgs e);
        public event UpdatedT_CameraHandle UpdatedT_Camera;
        public T_Camera t_Camera { get {
                if (_t_Camera == null)
                {
                    _t_Camera = judgerParamerService.QueryCamera(Constant.MSN);
                    
                }
                return _t_Camera;
            } set => _t_Camera = value; }

        private T_Camera _t_Camera;
        double dMin_ExposureTime, dMax_ExposureTime;
        double dMin_Gain, dMax_Gain,dMin_RB,dMax_RB,dMin_GB,dMax_GB,dMin_BB,dMax_BB;
        bool isLoaded = false;//是否加载完成
        Thread ImageThread;
        
        ICameraDevice camera;
        public DaHengAdjustWin()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (camera == null) return;
            camera.SetBalanceWhiteAuto(Camera.Enums.BalanceWhiteAutoStatusEnum.BALANCE_WHITE_AUTO_ONCE);

            double val = 0;
            val = camera.GetExposureTime();
            NumUD_ExposureTime.Value = val;

            val = camera.GetGain();
            NumUD_Plus.Value = val;

            val = camera.GetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_BLUE);
            NumUD_BB.Value = val;

            val = camera.GetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_RED);
            NumUD_RB.Value = val;

            val = camera.GetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_GREEN);
            NumUD_GB.Value = val;
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (camera == null) return;
            t_Camera.Gain = Convert.ToInt32(NumUD_Plus.Value);
            t_Camera.ExposureTime = Convert.ToInt32(NumUD_ExposureTime.Value);
            t_Camera.BB = Convert.ToDecimal(NumUD_BB.Value);
            t_Camera.RB = Convert.ToDecimal(NumUD_RB.Value);
            t_Camera.GB = Convert.ToDecimal(NumUD_GB.Value);
            if (judgerParamerService.UpdateOrInsertCamera(t_Camera)) {
                if (this.UpdatedT_Camera != null)
                {
                    this.UpdatedT_Camera(t_Camera, null);
                }
                Tool.AppLogInfo("保存相机参数成功");
                MessageBox.Show("保存成功");
            }
            else
            {
                Tool.AppLogInfo("保存相机参数失败");
            }

        }  


        private void __UpdateUI()
        {
            if (camera == null)
            {
                return;
            }
            if (t_Camera == null)
            {
                t_Camera = new T_Camera() {
                    MSN = Constant.MSN,
                    ExposureTime=Convert.ToInt32(camera.GetExposureTime()),
                    Gain = Convert.ToInt32(camera.GetGain()),
                    BB= Convert.ToDecimal(camera.GetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_BLUE)),
                    RB= Convert.ToDecimal(camera.GetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_RED)),
                    GB= Convert.ToDecimal(camera.GetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_GREEN))
            };
            }
            else
            {
                camera.SetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_BLUE,Convert.ToDouble(t_Camera.BB));
                camera.SetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_GREEN, Convert.ToDouble(t_Camera.GB));
                camera.SetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_RED, Convert.ToDouble(t_Camera.RB));
                camera.SetExposureTime(t_Camera.ExposureTime);
                camera.SetGain(t_Camera.Gain);
            }

            

            dMin_ExposureTime = camera.GetMinExposureTime();
            dMax_ExposureTime = camera.GetMaxExposureTime();

            dMin_Gain = camera.GetMinGain();
            dMax_Gain = camera.GetMaxGain();

            dMin_RB = camera.GetMinBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_RED);
            dMax_RB= camera.GetMaxBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_RED);

            dMin_GB = camera.GetMinBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_GREEN);
            dMax_GB = camera.GetMaxBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_GREEN);

            dMin_BB = camera.GetMinBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_BLUE);
            dMax_BB = camera.GetMaxBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_BLUE);

            Lab_ExposureTime.Content = Lab_ExposureTime.Content.ToString() + "(" + dMin_ExposureTime + "-" + dMax_ExposureTime + ")";
            NumUD_ExposureTime.MaxValue = dMax_ExposureTime;
            NumUD_ExposureTime.MinValue = dMin_ExposureTime;

            Lab_Gain.Content = String.Format("{0}({1}-{2})", Lab_Gain.Content, dMin_Gain, dMax_Gain);
            NumUD_Plus.MaxValue = dMax_Gain;
            NumUD_Plus.MinValue = dMin_Gain;

            Lab_BB.Content= String.Format("{0}({1}-{2})", Lab_BB.Content, dMin_BB, dMax_BB);
            NumUD_BB.MaxValue = dMax_BB;
            NumUD_BB.MinValue = dMin_BB;
            Lab_GB.Content = String.Format("{0}({1}-{2})", Lab_GB.Content, dMin_GB, dMax_GB);
            NumUD_GB.MaxValue = dMax_GB;
            NumUD_GB.MinValue = dMin_GB;
            Lab_RB.Content = String.Format("{0}({1}-{2})", Lab_RB.Content, dMin_RB, dMax_RB);
            NumUD_RB.MaxValue = dMax_RB;
            NumUD_RB.MinValue = dMin_RB;

            double val = 0;
            val = t_Camera.ExposureTime;
            NumUD_ExposureTime.Value = val;

            val = t_Camera.Gain;
            NumUD_Plus.Value = val;

            val = Convert.ToDouble(t_Camera.BB);
            NumUD_BB.Value = val;

            val = Convert.ToDouble(t_Camera.RB);
            NumUD_RB.Value = val;

            val = Convert.ToDouble(t_Camera.GB);
            NumUD_GB.Value = val;

        }
        private double CheckVal(double val,double _min,double _max)
        {
            if(val>=_min && val <= _max)
            {
                return val;
            }
            if (val < _min)
            {
                return _min;
            }
            if (val > _max)
            {
                return _max;
            }
            return val;
        }
        private void NumUD_BB_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (camera == null ||!isLoaded) return;
            //isRealTime = false;
            //while (!isStoped)
            //{
            //    Thread.Sleep(10);
            //}
            camera.Stop();
            NumericUpDown_Control nud = sender as NumericUpDown_Control;
            if (nud.Tag.ToString() == "R")
            {
                camera.SetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_RED, CheckVal(nud.Value,dMin_RB,dMax_RB));
            }
            else if (nud.Tag.ToString() == "B")
            {
                camera.SetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_BLUE, CheckVal(nud.Value,dMin_BB,dMax_BB));
            }
            else if (nud.Tag.ToString() == "G")
            {
                camera.SetBalanceRatio(Camera.Enums.BalanceWhiteChanelEnum.BALANCE_RATIO_SELECTOR_GREEN, CheckVal(nud.Value,dMin_GB,dMax_GB));
            }
            else if (nud.Tag.ToString() == "Gain")
            {
                camera.SetGain(CheckVal(nud.Value,dMin_Gain,dMax_Gain));
            }
            else if (nud.Tag.ToString() == "Time")
            {
                camera.SetExposureTime(CheckVal(nud.Value,dMin_ExposureTime,dMax_ExposureTime));
            }
            camera.Play();
            //ImageThread.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
                camera = CameraFactory.CreateCamera();
                if (camera == null)
                {
                    MessageBox.Show("创建相机实例失败！");
                    return;
                }
            try
            {
                camera.Open();
                __UpdateUI();
                camera.Play();
                ImageThread = new Thread(new ThreadStart(Capture));
                ImageThread.Start();

                
                isLoaded = true;
            }
            catch(Exception ex)
            {
                Tool.AppLogError(ex);
                MessageBox.Show(ex.Message);
            }
            
        }
        bool isRealTime = true;
        private void Capture()
        {
            while (isRealTime)
            {
                try
                {
                    var bm = camera.GetBitmap();
                
                    if (bm != null)
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.TargetImg.Source = null;
                            this.TargetImg.Source = ImgUtil.BitmapToBitmapSource(bm);
                        }));
                }catch(Exception ex)
                {
                    Tool.AppLogWarn(ex);
                }
            }
        }
        private void Camera_BitmapGot(object sender, BitmapGotEventArgs e)
        {
            var bm = e.bitmap;
            if (bm != null)
                try
                {
                    

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }
        

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isRealTime = false;
            if (camera != null)
            {
                camera.Stop();
            }
        }
    }
}
