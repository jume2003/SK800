using SKABO.BLL.IServices.IUser;
using SKABO.Common;
using SKABO.Common.Models.User;
using SKABO.Common.Utils;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK_ABO.Views.User
{
    public class EditUserViewModel:Screen
    {
        public EditUserViewModel()
        {

        }
        [StyletIoC.Inject]
        private IUserService userService { get; set; }
        private IList<T_Role> _t_Roles;
        public IList<T_Role> roleList
        {
            get
            {
                if (_t_Roles == null)
                {
                    _t_Roles = userService.QueryAllRole();
                    if(_t_Roles!=null && Constants.Login.RoleName != "管理员")
                    {
                        _t_Roles.Remove(_t_Roles.Where(c => c.Name == "管理员").FirstOrDefault());
                    }
                }
                return _t_Roles;
            }
        }
        
        public T_User User { get; set; }

        public void AddUser()
        {
            var cloud = (this.View as EditUserView).TipCloud;
            
            if (String.IsNullOrEmpty(User.LoginName))
            {
                cloud.Content = "用户名不能为空";
                cloud.ShowHint();
                return;
            }
            if (User.LoginName.Length>20)
            {
                cloud.Content = "用户名太长";
                cloud.ShowHint();
                return;
            }
            if (userService.ExistLoginName(User.LoginName))
            {
                cloud.Content = "用户名已存在";
                cloud.ShowHint();
                return;
            }
            var role=roleList.Where(c => c.Code == User.RoleCode).FirstOrDefault();
            if (role == null)
            {
                cloud.Content = "角色不能空";
                cloud.ShowHint();
                return;
            }
                User.RoleName = role.Name;
                User.RightValue = role.RightValue;
            if (User.ID == 0)
            {
                User.LoginPwd = MD5Util.getEncryptedPwd(Tool.getAppSetting("DefaultPWD"));
            }
            User.PY = User.LoginName.GetSpellCode();
            userService.InsertOrUpdateUser(User);
            this.RequestClose(true);
        }
        public void Cancel()
        {
            this.RequestClose(false);
        }
    }
}
