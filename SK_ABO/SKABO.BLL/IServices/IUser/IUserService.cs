using SKABO.Common.Models.Config;
using SKABO.Common.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.BLL.IServices.IUser
{
    public interface IUserService
    {
        bool ValidatePWD(T_User _User);
        IList<T_User> GetLoginNameByPY(String py);
        bool ValidateSKAdmin(String Pwd);
        /// <summary>
        /// 验证厂家配置密码，天密码及内置账号密码都可以通过
        /// </summary>
        /// <param name="Pwd"></param>
        /// <returns></returns>
        bool ValidateConfigPWD(String Pwd);
        bool ExistLoginName(String LoginName);
        bool UpdateLoginTime(T_User t_User);
        IList<T_User> QueryAllUser();
        IList<T_Role> QueryAllRole();
        bool DeleteUser(T_User t_User);
        bool UpdateRoleRight(T_Role t_Role);
        bool InsertOrUpdateUser(T_User t_User);
        bool ChangeUserPwd(String NewPwd, String OldPwd, out String Message);
        bool ResetPwd(T_User t_User);
        SysConfig QuerySysConfig(String SnKey);
        int UpdateSysConfig(SysConfig sysConfig);
    }
}
