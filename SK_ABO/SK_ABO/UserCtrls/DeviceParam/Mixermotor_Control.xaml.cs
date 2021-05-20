using System;
using System.Windows;
using System.Windows.Controls;
using SKABO.Common.Models.Communication.Unit;

namespace SK_ABO.UserCtrls.DeviceParam
{
    /// <summary>
    /// Electromotor_Control.xaml 的交互逻辑
    /// </summary>
    public partial class Mixermotor_Control : UserControl
    {
        public static readonly DependencyProperty MotorProperty = DependencyProperty.Register("Motor", typeof(Mixer), typeof(Mixermotor_Control), null, null);
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(String), typeof(Mixermotor_Control), null, null);
        public Mixermotor_Control()
        {
            InitializeComponent();
            
            //Motor = new Electromotor();
            //this.DataContext = this;
        }
        public String Header { get
            {
                return GetValue(HeaderProperty) as String;
            }
            set
            {
                SetValue(HeaderProperty, value);
            }
        }
        public Electromotor Motor
        {
            get
            {
                return GetValue(MotorProperty) as Electromotor;
            }
            set
            {
                SetValue(MotorProperty, value);
            }
        }
    }
}
