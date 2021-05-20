using SKABO.Common.Enums;
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

namespace SKABO.Common.Views
{
    /// <summary>
    /// AlertView.xaml 的交互逻辑
    /// </summary>
    public partial class AlertView : Window
    {
        private Func<Boolean, Boolean> Alarm;
        private Semaphore semaphore;
        public static UserResultEnum ResultEnuml { get; private set; }

        public AlertView(Func<Boolean, Boolean> AlarmFunc)
        {
            Alarm= AlarmFunc;
            Alarm(true);
            InitializeComponent();
        }
        public void SetAlertParam(String title, String content, Semaphore semaphore, bool CanTryAgain = true, bool CanIngore = false)
        {
            this.Title = title;
            label_message.Content = content;
            this.semaphore = semaphore;
            this.Btn_Retry.IsEnabled = CanTryAgain;
            this.Btn_Ignore.IsEnabled = CanIngore;
        }
        //otherPartDevice.Alarm(true);
        private void DoCloseAlert(object sender, RoutedEventArgs e)
        {
            Alarm(false);
        }
        private void DoIgnore(object sender, RoutedEventArgs e)
        {
            Alarm(false);
            ResultEnuml = UserResultEnum.IGNORE;
            if (semaphore != null) semaphore.Release(1);
            
            this.DialogResult = true;
            this.Close();
        }
        private void Cancel(object sender, RoutedEventArgs e)
        {
            Alarm(false);
            ResultEnuml = UserResultEnum.ABORT;
            if (semaphore != null) semaphore.Release(1);
            this.DialogResult = false;
            this.Close();
        }
        private void DoRetry(object sender, RoutedEventArgs e)
        {
            Alarm(false);
            ResultEnuml = UserResultEnum.RETRY;
            if (semaphore != null) semaphore.Release(1);
            this.DialogResult = true;
            this.Close();
        }
    }
}
