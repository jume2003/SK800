using IBatisNet.DataMapper;
using SKABO.Common;
using SKABO.Common.Models.Config;
using SKABO.Common.Models.User;
using SKABO.Common.Utils;
using SKABO.DAL.IDAO.IUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.DAO.User
{
    public class UserDAO : IUserDAO
    {
        ISqlMapper mapper { get; set; }
        public UserDAO(ISqlMapper mapper)
        {
            this.mapper = mapper;
        }
        public IList<T_User> GetLoginNameByPY(string py)
        {
            if (String.IsNullOrEmpty(py))
            {
                return null;
            }
            return mapper.QueryForList<T_User>("QueryT_UserByPY",py);
        }
        public bool ExistLoginName(String LoginName)
        {
            var user = QueryTUserByLoginName( LoginName);
            bool res = user != null;
            user = null;
            return res;
        }
        public T_User QueryTUserByLoginName(String LoginName)
        {
            return mapper.QueryForObject<T_User>("QueryT_User", LoginName);
        }
        
        public IList<T_User> QueryAllUser()
        {
            return mapper.QueryForList<T_User>("QueryAllUser",null);
        }

        public IList<T_Role> QueryAllRole()
        {
            return mapper.QueryForList<T_Role>("QueryAllRole", null);
        }

        public bool DeleteUser(T_User t_User)
        {
            return mapper.Update("DeleteT_User", t_User)>0;
        }
        public bool UpdateRoleRight(T_Role t_Role)
        {
            return mapper.Update("UpdateRoleRight", t_Role) > 0;
        }
        public bool InsertOrUpdateUser(T_User t_User)
        {
            if (t_User.ID == 0)
            {
                mapper.Insert("InsertT_User", t_User);
                return t_User.ID > 0;
            }
            else
            {
                return mapper.Update("UpdateT_User", t_User)> 0;
            }
        }

        public bool UpdateLoginTime(T_User t_User)
        {
            return mapper.Update("UpdateT_User_LastLoginTime", t_User) > 0;
        }
        public bool ChangeUserPwd(T_User t_User)
        {
            return mapper.Update("UpdateT_User_PWD", t_User) > 0;
        }

        public SysConfig QuerySysConfig(string SnKey)
        {
            return mapper.QueryForObject<SysConfig>("QuerySysConfig", SnKey);
        }

        public int UpdateSysConfig(SysConfig sysConfig)
        {
            return mapper.Update("UpdateSysConfig", sysConfig);
        }
    }
}
