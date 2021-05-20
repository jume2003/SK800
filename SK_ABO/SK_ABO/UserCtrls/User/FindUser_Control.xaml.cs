using SKABO.BLL.IServices.IUser;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SKABO.Common;

namespace SK_ABO.UserCtrls.User
{
    /// <summary>
    /// FindUser_Control.xaml 的交互逻辑
    /// </summary>
    public partial class FindUser_Control : UserControl
    {
        public static readonly DependencyProperty LoginNameProperty= DependencyProperty.Register("LoginName", typeof(String), typeof(FindUser_Control), null, null);
        
            
        private IUserService _userService;
        private IUserService userService
        {
            get
            {
                if (_userService == null)
                {
                    _userService=  IoC.Get<IUserService>();
                }
                return _userService;
            }
        }
        public FindUser_Control()
        {
            InitializeComponent();
        }
        public String LoginName
        {
            get
            {
                return GetValue(LoginNameProperty) as String;
            }
            set
            {
                SetValue(LoginNameProperty, value);
            }
        }

        private void Com_LoginName_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Back)
            {
                ComboBox cmb = (ComboBox)sender;
                if (cmb.Text!=null && cmb.Text.Trim().Length > 1)
                {
                    var list = userService.GetLoginNameByPY(cmb.Text.Trim());
                    if (!cmb.IsDropDownOpen && list != null && list.Count() > 0)
                    {
                        cmb.IsDropDownOpen = true;
                        var txt = cmb.Template.FindName("PART_EditableTextBox", cmb) as TextBox;
                        txt.SelectionStart = txt.Text.Length;
                    }
                    cmb.ItemsSource = list;
                }
            }
        }
    }
}
