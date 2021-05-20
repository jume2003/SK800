using SKABO.BLL.IServices.IUser;
using SKABO.Common.Models.User;
using SKABO.DAL.IDAO.IUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;
using SKABO.Common;
using SKABO.Common.Models.Config;

namespace SKABO.BLL.Services.User
{
    public class UserService : IUserService
    {
        private IUserDAO userDAO;
        public UserService(IUserDAO userDAO)
        {
            this.userDAO = userDAO;
        }
        public IList<T_User> GetLoginNameByPY(string py)
        {
            py = py.Trim();
            py = py.SafeSqlLiteral();
           return userDAO.GetLoginNameByPY(py);
        }

        public bool ValidatePWD(T_User _User)
        {
            string loginName = _User.LoginName, Pwd = _User.LoginPwd;
            if (String.IsNullOrEmpty(loginName)) return false;
            loginName = loginName.ToUpper();
            if (Constants.AdminAccount == loginName)
            {
                var result = ValidateAdmin(loginName, Pwd);
                if (result)
                {
                    var user = userDAO.QueryTUserByLoginName( loginName);
                    if (user != null)
                    {
                        TransExpUtil<T_User, T_User>.CopyValue(user, _User);
                    }
                    else
                    {
                        result = false;
                    }
                }
                return result;
            }
            else if (Constants.SKAccount == loginName)
            {
                return ValidateSKAdmin(loginName, Pwd);
            }
            else
            {
                var user = userDAO.QueryTUserByLoginName( loginName);
                if (user == null) return false;
                var res = (Common.Utils.MD5Util.validPassword(Pwd, user.LoginPwd));
                if (res)
                {
                    TransExpUtil<T_User, T_User>.CopyValue(user, _User);
                }
                return res;
            }
        }
        private bool ValidateSKAdmin(string loginName, string Pwd)
        {
            //内置生科管理员密码
            String key = Tool.getAppSetting("Innerkey");

            return MD5Util.validPassword(Pwd, key);
        }
        public bool ValidateConfigPWD(String Pwd)
        {
            if (String.IsNullOrEmpty(Pwd))
            {
                return false;
            }
            bool res = false;
            res = ValidateDayCode(Pwd);
            if (!res)
            {
                res = ValidateSKAdmin(Pwd);
            }
            return res;
        }
        public bool ValidateSKAdmin(string Pwd)
        {
            return ValidateSKAdmin(null, Pwd);
        }

        private bool ValidateAdmin(string loginName, string Pwd)
        {
            return ValidateDayCode(Pwd);
        }
        private bool ValidateDayCode(String Pwd)
        {
            bool IgnoreCase ="1"== Tool.getAppSetting("DayCodeIgnoreCase");
            return MD5Util.GeneratePWDByDate(DateTime.Today).Equals(Pwd, IgnoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
        }
        public bool ExistLoginName(String LoginName)
        {
            if (LoginName == SKABO.Common.Constants.SKAccount)
                return true;
            return userDAO.ExistLoginName(LoginName);
        }
        public IList<T_User> QueryAllUser()
        {
            return userDAO.QueryAllUser();
        }
        public IList<T_Role> QueryAllRole()
        {
            return userDAO.QueryAllRole();
        }
        public bool DeleteUser(T_User t_User)
        {
            return userDAO.DeleteUser(t_User);
        }
        public bool UpdateRoleRight(T_Role t_Role)
        {
            return userDAO.UpdateRoleRight(t_Role);
        }
        public bool InsertOrUpdateUser(T_User t_User)
        {
            return userDAO.InsertOrUpdateUser(t_User);
        }

        public bool UpdateLoginTime(T_User t_User)
        {
            return userDAO.UpdateLoginTime(t_User);
        }

        public bool ChangeUserPwd(string NewPwd, string OldPwd,out String Message)
        {
            Message = "";
            bool result = false;
            if (Constants.Login.LoginName == Constants.AdminAccount || Constants.Login.LoginName == Constants.SKAccount  )
            {
                result= true;
            }
            else
            {
                var res = (Common.Utils.MD5Util.validPassword(OldPwd, Constants.Login.LoginPwd));
                if (res)
                {
                    string oldPwd = Constants.Login.LoginPwd;
                    Constants.Login.LoginPwd = MD5Util.getEncryptedPwd(NewPwd);
                    result = userDAO.ChangeUserPwd(Constants.Login);

                }
                else
                {
                    Message = "旧密码不正确！";
                    return false;
                }
            }
            Message = result ? "更改成功" : "更改失败";
            return result;
        }

        public bool ResetPwd(T_User t_User)
        {
            t_User.LoginPwd = MD5Util.getEncryptedPwd(Tool.getAppSetting("DefaultPWD"));
            return  userDAO.ChangeUserPwd(t_User);
        }

        public SysConfig QuerySysConfig(string SnKey)
        {
            return userDAO.QuerySysConfig(SnKey);
        }

        public int UpdateSysConfig(SysConfig sysConfig)
        {
            return userDAO.UpdateSysConfig(sysConfig);
        }
    }
}
