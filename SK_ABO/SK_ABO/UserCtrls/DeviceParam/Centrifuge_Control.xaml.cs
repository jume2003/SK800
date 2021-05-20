using System;
using System.Windows;
using System.Windows.Controls;
using SKABO.Common.Models.Communication;
using SKABO.Common.Models.Communication.Unit;

namespace SK_ABO.UserCtrls.DeviceParam
{
    /// <summary>
    /// Centrifuge_Control.xaml 的交互逻辑
    /// </summary>
    public partial class Centrifuge_Control : UserControl
    {
        public static readonly DependencyProperty CentrProperty = DependencyProperty.Register("Centrifuge", typeof(CentrifugeM), typeof(Centrifuge_Control), null, null);
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(String), typeof(Centrifuge_Control), null, null);
        public Centrifuge_Control()
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
        public CentrifugeM Centrifuge
        {
            get
            {
                return GetValue(CentrProperty) as CentrifugeM;
            }
            set
            {
                SetValue(CentrProperty, value);
            }
        }
    }
}
