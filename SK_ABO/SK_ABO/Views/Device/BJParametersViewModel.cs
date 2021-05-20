using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;
using System.Windows.Controls;
using SK_ABO.Pages.Device;
using SKABO.Common.Models.BJ;
using SKABO.BLL.IServices.IUser;
using SKABO.Common;
using SKABO.Common.Enums;

namespace SK_ABO.Views.Device
{
    public class BJParametersViewModel:Screen
    {
        private IViewManager viewManager;
        private IWindowManager windowManager;
        private IUserService userService;
        private int CurrentTabIndex = 0;
        public BJParametersViewModel(IWindowManager windowManager,IViewManager viewManager, IUserService userService)
        {
            this.windowManager = windowManager;
            this.viewManager = viewManager;
            this.userService = userService;
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            var list = this.View.GetControls<Frame>();
            foreach (var item in list)
            {
                var index = Convert.ToByte(item.Tag);
                BJParameterViewModel VM = IoC.Get<BJParameterViewModel>();
                VM.BJIndex = index;
                
                if (VM != null)
                {
                    var pv = viewManager.CreateAndBindViewForModelIfNecessary(VM);
                    item.SetValue(Frame.ContentProperty, pv);
                }
            }
        }
        public void Close()
        {
            RequestClose();
        }
        public bool CanSaveParameter
        {
            get { return Constants.Login.CheckRight(RightEnum.DeviceParameterModify); }
        }
        public void SaveParameter(Button btn)
        {
            bool DoContinue = false;
            if (!userService.ValidateConfigPWD(Constants.ConfigPwd)){
                var v = IoC.Get<ValidatePwdViewModel>();
                var res=windowManager.ShowDialog(v);
                if (res.HasValue)
                {
                    DoContinue = res.Value;
                }
            }
            else
            {
                DoContinue = true;
            }
            if (!DoContinue) return;

            TabControl tabControl = (this.View as BJParametersView).TabContent;

            var frames=(tabControl.SelectedItem as TabItem).GetControls<Frame>();
            if (frames.Count > 0)
            {
                var f = frames[0];
                BJParameterView BJView = f.Content as BJParameterView;
                BJParameterViewModel BjModel = BJView.DataContext as BJParameterViewModel;
                if(BjModel.SaveParameter(out String Msg))
                {
                    var msg = new MessageWin();
                    btn.ShowHint(msg);
                }
                else
                {
                    var msg = new MessageWin(Msg);
                    btn.ShowHint(msg);
                }
            }

            
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl t = (TabControl)sender;
            CurrentTabIndex = t.SelectedIndex;
        }
    }
}
