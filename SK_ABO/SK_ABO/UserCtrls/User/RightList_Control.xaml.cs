using System;
using SKABO.Common;
using System.Windows;
using System.Windows.Controls;
using SKABO.BLL.IServices.IUser;
using SKABO.Common.Enums;
using SKABO.Common.Models.User;
using SKABO.Common.Utils;

namespace SK_ABO.UserCtrls.User
{
    /// <summary>
    /// RightList_Control.xaml 的交互逻辑
    /// </summary>
    public partial class RightList_Control : UserControl
    {
        public static readonly DependencyProperty RightValueProperty =
            DependencyProperty.Register("RightValue", typeof(long), typeof(RightList_Control), null, null);
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(String), typeof(RightList_Control), null, null);
        public static readonly DependencyProperty CanEditProperty =
            DependencyProperty.Register("CanEdit", typeof(bool), typeof(RightList_Control), null, null);
        public RightList_Control()
        {
            InitializeComponent();
            var list = this.GetControls<CheckBox>();
            var rights = Enum.GetValues(typeof(RightEnum));
            for (int i=0;i<list.Count;i++)
            {
                var item = list[i];
                RightEnum rightEnum = (RightEnum)rights.GetValue(i);
                item.Tag = rightEnum;
                item.Content = rightEnum.GetDescription();
                item.Click += CheckBox_Click;
            }
        }
        private T_Role _Role;
        public T_Role Role { get => _Role;
            set {
                _Role = value;
                if (value != null)
                {
                    RightValue = value.RightValue;
                    Header = String.Format("【{0}】的权限", value.Name.Trim());
                }
            } }
        public bool CanEdit
        {
            get
            {
                return (bool)GetValue(CanEditProperty);
            }
            set
            {
                SetValue(CanEditProperty, value);
                BtnCheckAll.Visibility = CanEdit ? Visibility.Visible : Visibility.Hidden;
                BtnCheckNo.Visibility = CanEdit ? Visibility.Visible : Visibility.Hidden;
                BtnSave.Visibility = CanEdit ? Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// GroupBox的header
        /// </summary>
        public String Header
        {
            get
            {
                return (String)GetValue(HeaderProperty);
            }
            set
            {
                SetValue(HeaderProperty, value);
                
            }
        }
        /// <summary>
        /// 权限值
        /// </summary>
        public long RightValue
        {
            get
            {
                return (long)GetValue(RightValueProperty);
            }
            set
            {
                SetValue(RightValueProperty, value);
                var list = this.GetControls<CheckBox>();
                foreach (var item in list)
                {
                    long r = Convert.ToInt64(item.Tag);
                    item.IsChecked = (value&r)== r;
                }
            }
        }
        private void BtnCheckNo_Click(object sender, RoutedEventArgs e)
        {
            
            RightValue = 0;
        }

        private void BtnCheckAll_Click(object sender, RoutedEventArgs e)
        {
            var list = this.GetControls<CheckBox>();
            SetValue(RightValueProperty, 0L);
            foreach (var item in list)
            {
                item.IsChecked = true;
                SetValue(RightValueProperty, RightValue + Convert.ToInt64(item.Tag));
            }
            
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            if (chk.IsChecked.Value)
            {
                SetValue(RightValueProperty, RightValue + Convert.ToInt64(chk.Tag));
            }
            else
            {
                SetValue(RightValueProperty, RightValue - Convert.ToInt64(chk.Tag));
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            
            if (!CanEdit) return;
            if (Role == null) return;
            Role.RightValue = RightValue;
            var userService = IoC.Get<IUserService>();
            if (userService.UpdateRoleRight(Role))
            {
                TipCloud.ShowHint();
            }
            
        }
    }
}
