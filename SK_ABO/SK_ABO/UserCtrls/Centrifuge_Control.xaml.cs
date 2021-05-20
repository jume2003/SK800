using SK_ABO.UserCtrls.Base;
using SKABO.ActionEngine;
using SKABO.Common.Models.BJ;
using SKABO.Hardware.RunBJ;
using SKABO.ResourcesManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SK_ABO.UserCtrls
{
    /// <summary>
    /// Centrifuge_Control.xaml 的交互逻辑
    /// </summary>
    public partial class Centrifuge_Control : BJControl
    {
        private bool HasLoaded = false;
        private Storyboard SB;
        public Centrifuge_Control()
        {
            InitializeComponent();
            SB = (Storyboard)this.Resources["Centrifuge_Run_H"];
        }
        public void Start()
        {
            Start(1);
        }
        public void Start(Double SpeedRatio)
        {
            SB.SpeedRatio = SpeedRatio;
            SB.Begin();
        }
        public void Stop()
        {
            SB.Stop();
        }
        /// <summary>
        /// 装载卡，放卡
        /// </summary>
        /// <param name="indexes">最小值为1</param>
        public void LoadCard(params byte[] indexes)
        {
            centrifugeLeaf_Control.LoadCard(indexes);
        }
        /// <summary>
        /// 取卡
        /// </summary>
        /// <param name="indexes">最小值为1</param>
        public void UnLoadCard(params byte[] indexes)
        {
            centrifugeLeaf_Control.UnLoadCard(indexes);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (HasLoaded) return;
            HasLoaded = true;
            if (DataContext is VBJ vbj)
            {
                vbj.ChangedValueMap = ChangedSeatVlaue;
            }
            base.RaiseAddedControls();
        }
        public void ChangedSeatVlaue(ChangeBJEventArgs e)
        {
            if (e.NewVal == null)
            {
                UnLoadCard((byte)(e.X+1));
            }
            else
            {
                LoadCard((byte)(e.X + 1));
            }
            var cen = Engine.getInstance().cenMrg.GetCentrifugeByCode(e.Code);
            if(cen!=null)
            {
                HightSpeedTime.Content = "高速:" + cen.HightSpeedTime+"秒";
                LowSpeedTime.Content = "低速:" + cen.LowSpeedTime + "秒";
            }
        }
        public bool IsLow { get; set; }
    }
}
