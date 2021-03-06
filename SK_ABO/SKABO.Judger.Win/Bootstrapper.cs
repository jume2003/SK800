using System;
using System.IO;
using log4net.Config;
using SKABO.BLL.IServices.IJudger;
using SKABO.BLL.Services.Judger;
using SKABO.Common.Utils;
using SKABO.Judger.Win.Wins;
using Stylet;
using StyletIoC;

namespace SKABO.Judger.Win
{
    public class Bootstrapper : Bootstrapper<StartUpViewModel>
    {
       
        protected override void OnStart()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "log4net.config"));
            Tool.AppLogInfo("启动主程序");
        }
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            
        }

        protected override void Configure()
        {
            // Perform any other configuration before the application starts
        }
    }
}
