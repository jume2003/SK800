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
    /// Centrifuge_plc_Control.xaml 的交互逻辑
    /// </summary>
    public partial class Centrifuge_plc_Control : UserControl
    {
        public static readonly DependencyProperty CentProperty = DependencyProperty.Register("Cent", typeof(Centrifuge), typeof(Centrifuge_plc_Control), null, null);
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(String), typeof(Centrifuge_plc_Control), null, null);
        public Centrifuge_plc_Control()
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
        public Centrifuge Cent
        {
            get
            {
                return GetValue(CentProperty) as Centrifuge;
            }
            set
            {
                SetValue(CentProperty, value);
            }
        }
    }
}
