using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace SK_ABO.UserCtrls
{
    /// <summary>
    /// CheckTube_UserControl.xaml 的交互逻辑
    /// </summary>
    public partial class CheckTube_UserControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(uint), typeof(CheckTube_UserControl),null, null);
        private readonly uint t1 = Convert.ToUInt32(Math.Pow(2, 0));
        private readonly uint t2 = Convert.ToUInt32(Math.Pow(2,1));
        private readonly uint t3 = Convert.ToUInt32(Math.Pow(2, 2));
        private readonly uint t4 = Convert.ToUInt32(Math.Pow(2, 3));
        private readonly uint t5 = Convert.ToUInt32(Math.Pow(2, 4));
        private readonly uint t6 = Convert.ToUInt32(Math.Pow(2, 5));
        private readonly uint t7 = Convert.ToUInt32(Math.Pow(2, 6));
        private readonly uint t8 = Convert.ToUInt32(Math.Pow(2, 7));
        public CheckTube_UserControl()
        {
            InitializeComponent();
            Chk1.Tag =  t1;
            Chk2.Tag =  t2;
            Chk3.Tag =  t3;
            Chk4.Tag =  t4;
            Chk5.Tag =  t5;
            Chk6.Tag =  t6;
            Chk7.Tag =  t7;
            Chk8.Tag =  t8;
        }
        public uint Value { get
            {
                return (uint) GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
                
            }
        }
        public void InitCheckBox()
        {
            Chk1.IsChecked = (Value & t1) == t1;
            Chk2.IsChecked = (Value & t2) == t2;
            Chk3.IsChecked = (Value & t3) == t3;
            Chk4.IsChecked = (Value & t4) == t4;
            Chk5.IsChecked = (Value & t5) == t5;
            Chk6.IsChecked = (Value & t6) == t6;
            Chk7.IsChecked = (Value & t7) == t7;
            Chk8.IsChecked = (Value & t8) == t8;
        }
        private void Chk1_Click(object sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            if (chk.IsChecked.Value)
            {
                this.SetValue(ValueProperty, Value + (uint)chk.Tag);
            }
            else
            {
                this.SetValue(ValueProperty, Value - (uint)chk.Tag);
            }
        }

        private void userControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCheckBox();
        }
    }
    [ValueConversion(typeof(double), typeof(double))]
    public class ChkHeightConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / double.Parse(parameter.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
