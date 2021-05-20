using SK_ABO.UserCtrls.DeviceParam;
using SKABO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SK_ABO.Pages.Device
{
    /// <summary>
    /// CommunicationSettingView.xaml 的交互逻辑
    /// </summary>
    public partial class CommunicationSettingView : Page
    {
        public CommunicationSettingView()
        {
            InitializeComponent();
            PaintCouvesues();
            PaintSampleRack();
        }
        private void BindingValue(String Path, DependencyProperty TargetProperty, FrameworkElement Target)
        {
            Binding bindingValue = new Binding(Path) { Mode = BindingMode.TwoWay };

            bindingValue.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Target.SetBinding(TargetProperty, bindingValue);
        }
        /// <summary>
        /// 绘制样本架组件
        /// </summary>
        private void PaintSampleRack()
        {
            for (byte i = 0; i < Constants.SampleRackCount; i++)
            {
                Label lab = new Label() { Content = String.Format("{0}# ", i + 1) };
                TextBox text = new TextBox();
                BindingValue(String.Format("Param.SampleRackCoils[{0}].Addr", i), TextBox.TextProperty, text);
                CheckBox sval = new CheckBox() { IsEnabled = false };
                BindingValue(String.Format("Param.SampleRackCoils[{0}].SetValue", i), CheckBox.IsCheckedProperty, sval);
                CheckBox cval = new CheckBox() { IsEnabled = false };
                BindingValue(String.Format("Param.SampleRackCoils[{0}].CurrentValue", i), CheckBox.IsCheckedProperty, cval);
                this.SS_Lab_panel.Children.Add(lab);
                this.SS_Addr_panel.Children.Add(text);
                this.SS_Set_panel.Children.Add(sval);
                this.SS_Current_panel.Children.Add(cval);
            }
        }
        /// <summary>
        /// 绘制孵育器组件
        /// </summary>
        private void PaintCouvesues()
        {
            //<CheckBox Content="1#" IsChecked="{Binding injector.Logic0.Valid,Mode=TwoWay}" Style="{DynamicResource chkLogic}" />
            for (byte i = Constants.CouveuseCount; i >0; i--)
            {
                Couveuse_Control Couveuse = new Couveuse_Control() { Header = String.Format("{0}# 孵育器", i ) };
                
                BindingValue(String.Format("CouMixer.Couveuses[{0}]", i-1), Couveuse_Control.CouvProperty, Couveuse);
                this.CouveusePanel.Children.Insert(0, Couveuse);
            }
        }
    }
}
