using SK_ABO.Views;
using SK_ABO.Views.GEL;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;
using System.Windows.Controls;
using SKABO.Common;
using SKABO.Common.Enums;
using SK_ABO.Views.User;
using SK_ABO.Views.Device;
using SK_ABO.Views.Trace;
using SK_ABO.Views.SystemConfig;
using SK_ABO.Views.LogicProgram;

namespace SK_ABO.Pages
{
    public class SystemViewModel:Screen
    {
        IWindowManager windowManager;
        IViewManager viewManager;
        public SystemViewModel(IWindowManager windowManager, IViewManager viewManager)
        {
            this.windowManager = windowManager;
            this.viewManager = viewManager;
        }
        /// <summary>
        /// 显示关于对话框
        /// </summary>
        public void ShowAbout()
        {
            new AboutUs().ShowDialog();

        }
        /// <summary>
        /// 显示统计分析
        /// </summary>
        public void ShowStatisticAnalysis()
        {
            var vm = IoC.Get<StatisticAnalysisViewModel>();
            windowManager.ShowDialog(vm);
        }
        /// <summary>
        /// 显示质检图
        /// </summary>
        public void ShowQcEtc()
        {
            var vm = IoC.Get<QcEtcViewModel>();
            windowManager.ShowDialog(vm);
        }
        /// <summary>
        /// 显示实验监测
        /// </summary>
        public void ExperManagement()
        {
            var vm = IoC.Get<ExperimentManagementViewModel>();
            windowManager.ShowDialog(vm);
        }
        /// <summary>
        /// 血型卡管理
        /// </summary>
        public void ShowGelCardWin()
        {
            //ShowWin<GelManagerViewModel>(RightEnum.);
            var vm = IoC.Get<GelManagerViewModel>();
            windowManager.ShowDialog(vm);
        }
        /// <summary>
        /// 用户管理
        /// </summary>
        public void UserManage()
        {
            ShowWin<UserManagerViewModel>(RightEnum.ManageUser);
        }
        /// <summary>
        /// 注销登录
        /// </summary>
        public void Logout(Button btn)
        {
            System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
            App.Current.MainWindow.Close();
            Constants.Login = null;
            Constants.ConfigPwd = null;
            var loginModel = IoC.Get<LoginViewModel>();
            bool? dialogResult = windowManager.ShowDialog(loginModel);
            if (dialogResult.HasValue && dialogResult.Value)
            {
                System.Windows.Application.Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
                var mainModel = IoC.Get<MainViewModel>();
                mainModel.StopApp();
                windowManager.ShowDialog(mainModel);
            }
            else
            {
                System.Windows.Application.Current.Shutdown(0);
            }
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        public void ChangePWD()
        {
            ChangePwdView w = new ChangePwdView();
            w.ShowDialog();
        }
        /// <summary>
        /// 日志管理
        /// </summary>
        public void TraceManage()
        {
            ShowWin<TraceManageViewModel>(RightEnum.ManageLog);
        }
        /// <summary>
        /// 部件控制台
        /// </summary>
        public void ShowDevceConsole()
        {
            ShowWin<DeviceConsoleViewModel>(RightEnum.Console);
        }
        /// <summary>
        /// 任务管理
        /// </summary>
        public void TaskManagement()
        {
            ShowWin<TaskManagementViewModel>(RightEnum.Console);
        }
        /// <summary>
        /// 部件参数
        /// </summary>
        public void ShowBjParamter()
        {
            ShowWin<BJParametersViewModel>(RightEnum.DeviceParameter);
        }
        public void ShowSysConfig()
        {
            var VM = IoC.Get<SystemConfigViewModel>();
            windowManager.ShowDialog(VM);
        }
        public void LogicManage()
        {
            var VM = IoC.Get<LogicManagerViewModel>();
            windowManager.ShowDialog(VM);
        }
        private void ShowWin<T>(RightEnum rightEnum)
        {
            if (Constants.Login.CheckRight(rightEnum))
            {
                var VM = IoC.Get<T>();
                windowManager.ShowDialog(VM);
            }
            else
            {
                var msg = new MessageWin("权限不足");
                this.View.ShowHint(msg);
            }
        }
    }
}
