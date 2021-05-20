using SKABO.Common.Models.Communication.Unit;
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

namespace SK_ABO.UserCtrls.DeviceParam
{
    /// <summary>
    /// DoubleSpeedMotor_control.xaml 的交互逻辑
    /// </summary>
    public partial class DoubleSpeedMotor_control : UserControl
    {
        public static readonly DependencyProperty MotorProperty = DependencyProperty.Register("Motor", typeof(DoubleSpeedMotor), typeof(DoubleSpeedMotor_control), null, null);
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(String), typeof(DoubleSpeedMotor_control), null, null);
        public DoubleSpeedMotor_control()
        {
            InitializeComponent();
        }
        public String Header
        {
            get
            {
                return GetValue(HeaderProperty) as String;
            }
            set
            {
                SetValue(HeaderProperty, value);
            }
        }
        public DoubleSpeedMotor Motor
        {
            get
            {
                return GetValue(MotorProperty) as DoubleSpeedMotor;
            }
            set
            {
                SetValue(MotorProperty, value);
            }
        }
    }
}
