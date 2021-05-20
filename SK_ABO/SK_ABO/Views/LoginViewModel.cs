using SKABO.BLL.IServices.IUser;
using SKABO.Common.Models.User;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;
using System.Windows.Controls;
using System.Windows.Input;
using SKABO.Common;
using SKABO.BLL.IServices.ITrace;
using SKABO.Common.Enums;

namespace SK_ABO.Views
{
    public class LoginViewModel:Screen
    {
        [StyletIoC.Inject]
        private IUserService userService { get; set; }
        [StyletIoC.Inject]
        private ITraceService TraceService;

        private String _LoginName;
        public String LoginID
        { get => _LoginName; set
            {
                _LoginName = value==null?null:value.Trim().ToUpper();
            }
        }
        public String Pwd { get; set; }
        public String Message { get; set; }
        public LoginViewModel() : base()
        {
            DisplayName = "中山生科全自动血库检测系统";
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            (this.View as LoginView).findUser.Focus();
        }
        public void Login(Button button)
        {
            Message = null;
            try
            {
                Pwd =(this.View as LoginView).Txt_pwd.Password;
                var Login = new T_User() { LoginName = LoginID, LoginPwd = Pwd };
                if (userService.ValidatePWD(Login))
                {
                    Constants.Login = Login;
                    TraceService.InsertT_Trace(TraceLevelEnum.LowImportant,String.Format("【{0}】登录", Login.LoginName));
                    Login.LastLoginTime = DateTime.Now;
                    userService.UpdateLoginTime(Login);
                    this.RequestClose(true);
                }
                else
                {
                    Message = "用户名或密码错误";
                }
            }catch(Exception ex)
            {
                Message = ex.Message;
            }
            finally
            {
                if (!String.IsNullOrEmpty(Message))
                {
                    //var cloud = (this.View as LoginView).TipCloud;
                    //cloud.Content = Message;
                    //cloud.ShowHint();
                    MessageWin messageWin = new MessageWin(Message);
                    button.ShowHint(messageWin);
                }
            }
        }
        
    }
}
