using SKABO.BLL.IServices.ITrace;
using SKABO.BLL.IServices.IUser;
using SKABO.Common.Models.User;
using Stylet;
using System;
using SKABO.Common.Utils;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKABO.Common.Enums;

namespace SK_ABO.Views
{
    class SecurityViewModel:Screen
    {
        [StyletIoC.Inject]
        private IUserService userService { get; set; }
        [StyletIoC.Inject]
        private ITraceService TraceService;

        private String _LoginName;
        public String LoginID
        {
            get => _LoginName; set
            {
                _LoginName = value == null ? null : value.Trim().ToUpper();
            }
        }
        public String Pwd { get; set; }
        public String Message { get; set; }
        
        public void Login()
        {
            Message = null;
            try
            {
                Pwd = (this.View as SecurityView).Txt_pwd.Password;
                var Security = new T_User() { LoginName = LoginID, LoginPwd = Pwd };
                if (userService.ValidatePWD(Security))
                {
                    TraceService.InsertT_Trace(TraceLevelEnum.HightImportant, String.Format("【{0}】安全确认", Security.LoginName));
                    this.RequestClose(true);
                }
                else
                {
                    Message = "用户名或密码错误";
                }
            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }
            finally
            {
                if (!String.IsNullOrEmpty(Message))
                {
                    MessageWin messageWin = new MessageWin(Message);
                    this.View.ShowHint(messageWin);
                }
            }
        }

    }
}
