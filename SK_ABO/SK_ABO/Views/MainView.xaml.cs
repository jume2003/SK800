using SKABO.Common;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SK_ABO.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            
        }

        public void Vm_SwitchPageEvent(object sender, EventArgs e)
        {
            Frame frame = sender as Frame;
            ContentControl.Content = frame;
            this.UpdateLayout();
        }
        public void InitStatusBar()
        {
            LaunchTimer();
            Lab_SN.Content = "SN:" + Constants.MSN;
            Lab_Ver.Content = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Lab_Login.Content = "登录人：" + Constants.Login.LoginName;
        }
        private void TimerTick(object sender, EventArgs e)
        {
            Lab_SystemTime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    
        private void LaunchTimer()
        {
            DispatcherTimer innerTimer = new DispatcherTimer(TimeSpan.FromSeconds(1.0),
                DispatcherPriority.Loaded, new EventHandler(this.TimerTick), this.Dispatcher);
            innerTimer.Start();
        }
    }
}
