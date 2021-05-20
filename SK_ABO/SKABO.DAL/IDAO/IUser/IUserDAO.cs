using SKABO.Common.Models.Config;
using SKABO.Common.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.IDAO.IUser
{
    public interface IUserDAO
    {
        T_User QueryTUserByLoginName(String LoginName);
        IList<T_User> GetLoginNameByPY(String py);
        bool ExistLoginName(String LoginName);
        bool UpdateLoginTime(T_User t_User);

        IList<T_User> QueryAllUser();
        IList<T_Role> QueryAllRole();
        bool DeleteUser(T_User t_User);
        bool UpdateRoleRight(T_Role t_Role);
        bool InsertOrUpdateUser(T_User t_User);

        bool ChangeUserPwd(T_User t_User);

        SysConfig QuerySysConfig(String SnKey);
        int UpdateSysConfig(SysConfig sysConfig);
    }
}
