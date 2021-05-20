using SKABO.BLL.IServices.IUser;
using System;
using System.Windows;
using System.Windows.Controls;
using SKABO.Common;
using SKABO.Common.Utils;

namespace SK_ABO.Views.User
{
    /// <summary>
    /// ChangePwdView.xaml 的交互逻辑
    /// </summary>
    public partial class ChangePwdView : Window
    {
        public ChangePwdView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            String OldPwd = OldPwdBox.Password;
            String NewPwd = NewPwdBox.Password;
            String CNewPwd = ConfirmNewPwdBox.Password;
            String Message = "";
            bool res = false;
            if (NewPwd != CNewPwd)
            {
                Message = "两次密码不一样";
            }
            else
            {
                var userService = IoC.Get<IUserService>();
                res=userService.ChangeUserPwd(NewPwd, OldPwd, out Message);
            }
            Button btn = sender as Button;
            MessageWin win = new MessageWin(Message);
            btn.ShowHint(win);
            if (res)
            {
                this.Close();
            }
        }
    }
}
