using SKABO.BLL.IServices.IUser;
using SKABO.Common.Models.User;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using SKABO.Common.Utils;
using System.Windows.Controls;
using SK_ABO.UserCtrls.User;
using System.Windows.Input;
using System.Collections.ObjectModel;
using SKABO.Common;
using SKABO.Common.Enums;

namespace SK_ABO.Views.User
{
    public class UserManagerViewModel:Screen
    {
        [StyletIoC.Inject]
        private IWindowManager windowManager;
        [StyletIoC.Inject]
        private IUserService userService { get; set; }
        private IList<T_Role> _t_Roles;
        public IList<T_Role> t_Roles
        {
            get
            {
                if (_t_Roles == null)
                {
                    _t_Roles = userService.QueryAllRole();
                }
                return _t_Roles;
            }
        }

        private BindableCollection<T_User> _t_Users;
        public ObservableCollection<T_User> t_Users
        {
            get
            {
                if (_t_Users == null)
                {
                    _t_Users = new BindableCollection<T_User>();
                    var list = userService.QueryAllUser();
                    if (list != null)
                        _t_Users.AddRange(list);
                    list = null;
                }
                return _t_Users;
            }
        }
        public T_Role SelectedRole { get; set; }
        public T_User SelectedUser { get; set; }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            var right = this.View.GetControl<RightList_Control>("RightList");
            right.CanEdit = false;
        }

        public void Dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            if (dg.Name == "Dg_Role")
            {
                if (SelectedRole != null)
                {
                    var right = this.View.GetControl<RightList_Control>("RightList");
                    right.CanEdit = Constants.Login.CheckRight(RightEnum.ManageRole); 
                    right.Role = SelectedRole;
                }
            }
            else if (dg.Name == "Dg_User")
            {
                if (e.AddedItems.Count == 0) return;
                T_User t_User = e.AddedItems[0] as T_User;
                var role = new T_Role() { Code = t_User.RoleCode, Name = t_User.LoginName, RightValue = t_User.RightValue };
                var right = this.View.GetControl<RightList_Control>("RightList");
                right.CanEdit = false;
                right.Role = role;
            }

        }
        public void Dg_GotFocus(object sender, EventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            if (dg.Name == "Dg_Role")
            {
                if (SelectedRole != null)
                {
                    var right = this.View.GetControl<RightList_Control>("RightList");
                    right.CanEdit = Constants.Login.CheckRight(RightEnum.ManageRole);
                    right.Role = SelectedRole;
                }
            }
            else if (dg.Name == "Dg_User")
            {
                if (SelectedUser != null)
                {
                    var role = new T_Role() { Code = SelectedUser.RoleCode, Name = SelectedUser.LoginName, RightValue = SelectedUser.RightValue };
                    var right = this.View.GetControl<RightList_Control>("RightList");
                    right.CanEdit = false;
                    right.Role = role;
                }
            }
        }
        public void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var Row = DataGridUtil.GetRowMouseDoubleClick(sender, e);
            if (Row == null) return;
            var t_User = (T_User)Row.Item;
            showUserWin(t_User);
        }
        private bool? showUserWin(T_User t_User)
        {
            var VM = IoC.Get<EditUserViewModel>();
            VM.User = t_User;
            return windowManager.ShowDialog(VM);
        }
        public void EditUser()
        {
            if (SelectedUser == null) return;
            
            showUserWin(SelectedUser);
        }
       public void DelUser()
        {
            if (SelectedUser == null) return;
            if (SelectedUser.LoginName.ToUpper() == "ADMIN")
            {
                this.View.ShowHint(new MessageWin(String.Format("不能删除【{0}】!", SelectedUser.LoginName)));
                return;
            }
            
            var result = windowManager.ShowMessageBox(String.Format("确定删除【{0}】吗?",SelectedUser.LoginName), "系统提示", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk);
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                SelectedUser.Status = 0;
                userService.DeleteUser(SelectedUser);
                t_Users.Remove(SelectedUser);
                this.View.ShowHint(new MessageWin());
            }
        }
        public void AddUser()
        {
            var t_User = new T_User() { CreatedBy=Constants.Login.LoginName,CreatedTime=DateTime.Now};
            var result=showUserWin(t_User);
            if(result.HasValue && result.Value)
            {
                t_Users.Add(t_User);
            }
        }
        public void ResetUser()
        {
            if (SelectedUser == null) return;
            if (SelectedUser.LoginName.ToUpper() == "ADMIN")
            {
                return;
            }

            var result = windowManager.ShowMessageBox(String.Format("确定重置【{0}】密码吗?", SelectedUser.LoginName), "系统提示", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Asterisk);
            if (result == System.Windows.MessageBoxResult.Yes)
            {

                if (userService.ResetPwd(SelectedUser))
                {
                    this.View.ShowHint(new MessageWin());
                }
                else
                {
                    this.View.ShowHint(new MessageWin("重置失败"));
                }
            }
        }
    }
}
