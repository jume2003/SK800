using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;
using System.Windows.Controls;
using SKABO.Common;
using SKABO.BLL.IServices.IUser;

namespace SK_ABO.Views
{
    public class ValidatePwdViewModel:Screen
    {
        public ValidatePwdViewModel(IUserService userService)
        {
            this.userService = userService;
            this.DisplayName = "请输入";
        }
        private IUserService userService;
        public void ValidatePwd(Button btn)
        {
            PasswordBox pBox = this.View.GetControl<PasswordBox>("pwdBox");
            Constants.ConfigPwd = pBox.Password;
            if (userService.ValidateConfigPWD(Constants.ConfigPwd))
            {
                RequestClose(true);
            }
            else
            {
                var msg = new MessageWin("密码错误！");
                btn.ShowHint(msg);
            }
        }
        public void Cancel()
        {
            RequestClose(false);
        }
    }
}
