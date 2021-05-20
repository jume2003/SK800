using log4net.Config;
using SKABO.Common.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SK_ABO
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
	{
        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew = false;
            var run = new System.Threading.Mutex(true, "SK_ABO.ShengKe", out createdNew);
            App.Current.Properties["Mutex"] = run;
            if (!createdNew)
            {
                MessageBox.Show("程序已经运行!", "系统提示");
                System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
                App.Current.Shutdown();
            }
            else
            {
                if (SystemParameters.MenuDropAlignment)
                {
                    var t = typeof(SystemParameters);
                    var field = t.GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
                    field.SetValue(null, false);

                }
                Thread.CurrentThread.CurrentCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();

                Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
                System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
                XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "log4net.config"));
                Tool.AppLogInfo("启动主程序");
                base.OnStartup(e);
            }
        }
    }
}
