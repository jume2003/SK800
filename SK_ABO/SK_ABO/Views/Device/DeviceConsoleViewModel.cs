using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;
using System.Windows.Controls;
using SK_ABO.Pages.Device;
using SKABO.Common.Enums;
using SKABO.Common;
using System.Windows.Input;
using SK_ABO.Pages;

namespace SK_ABO.Views.Device
{
    public class DeviceConsoleViewModel: Stylet.Screen
    {
        private IWindowManager windowManager;
        private IViewManager viewManager;
        public DeviceConsoleViewModel(IWindowManager windowManager,IViewManager viewManager)
        {
            this.windowManager = windowManager;
            this.viewManager = viewManager;
            this.DisplayName = "部件控制台";
        }
        IList<Frame> superList = new List<Frame>();
        List<Stylet.Screen> VM_list = new List<Screen>();
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            var list=this.View.GetControls<Frame>();
            foreach(var item in list)
            {
                var index = Convert.ToInt16(item.Tag);
                if (index <= 0)
                {
                    (item.Parent as TabItem).Visibility = System.Windows.Visibility.Collapsed;
                    superList.Add(item);
                }
                Stylet.Screen VM = null;
                switch (index)
                {
                    case -3:
                        {
                            VM = IoC.Get<CommunicationSettingViewModel>();
                            break;
                        }
                    case -2:
                        {
                            VM = IoC.Get<GlobalSettingViewModel>();
                            break;
                        }
                    case -1:
                        {
                            VM = IoC.Get<JYQParameterViewModel>();
                            break;
                        }
                    case 0:
                        {
                            //VM = IoC.Get<JYQViewModel>();
                            break;
                        }
                    case 1:
                        {
                            VM = IoC.Get<JYQViewModel>();
                            break;
                        }
                    case 2:
                        {
                            VM = IoC.Get<MachineHandViewModel>();
                            break;
                        }
                    case 3:
                        {
                            VM = IoC.Get<CentrifugeViewModel>();
                            break;
                        }
                    case 4:
                        {
                            VM = IoC.Get<MixerAndCouveuseViewModel>();
                            break;
                        }
                    case 5:
                        {
                            VM = IoC.Get<PiercerViewModel>();
                            break;
                        }
                    case 6:
                        {
                            VM = IoC.Get<ScanerViewModel>();
                            break;
                        }
                }
                if (VM != null)
                {
                    var pv = viewManager.CreateAndBindViewForModelIfNecessary(VM);
                    item.SetValue(Frame.ContentProperty, pv);
                    VM_list.Add(VM);
                }
            }
        }
        protected override void OnClose()
        {
            IoC.Get<ScanerViewModel>().CloseReaderRack();
            JYQViewModel.myTimer.Stop();
            JYQViewModel.myTimer.Dispose();
            base.OnClose();
        }
        public void Close()
        {
            this.RequestClose(false);
        }
        public void ShowBJ()
        {
            ShowWin<BJParametersViewModel>(RightEnum.DeviceParameter);
            foreach(var vm in VM_list)
            {
                if (vm is MachineHandViewModel hand)
                {
                    hand.UpdataBjList();
                }
                if (vm is JYQViewModel jyq)
                {
                    jyq.UpdataBjList();
                }
            }
        }
        public void ShowTaskManager()
        {
             ShowWin<TaskManagementViewModel>(RightEnum.Console);
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
        private String ShowHidden = "";
        public void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && (e.Key==Key.L || e.Key == Key.X || e.Key == Key.L))
            {
                e.Handled = true;
                ShowHidden += e.Key.ToString();
                if (ShowHidden.Equals("LXL", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach(var item in superList)
                    {
                        (item.Parent as TabItem).Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
            else
            {
                ShowHidden = "";
            }
        }
    }
}
