using System;
using System.Diagnostics;
using SK_ABO.Views;
using SKABO.BLL.IServices.IJudger;
using SKABO.Common.Utils;
using Stylet;

namespace SK_ABO.Pages
{
    public class ShellViewModel : Screen
    {

        [StyletIoC.Inject]
        private IJudgerParamerService judgerParamerService;
        private IWindowManager windowManager;
        private IViewManager viewManage;
        public ShellViewModel(IWindowManager windowManager,IViewManager viewManager)
        {
            this.windowManager = windowManager;
            this.viewManage = viewManager;
        }
        public void DoSomething(string argument)
        {
            Debug.WriteLine(String.Format("Argument is {0}", argument));
            //var v=viewManage.CreateViewForModel(new Views.TestViewModel());
            //windowManager.ShowWindow(
            var ff = judgerParamerService.QueryALlParamerByMSN(Tool.getAppSetting("MSN"));
            
            //this.
        }
    }
}
