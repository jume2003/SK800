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

namespace SK_ABO.UserCtrls
{
    /// <summary>
    /// TipCount_Control.xaml 的交互逻辑
    /// </summary>
    public partial class TipCount_Control : UserControl
    {
        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(int), typeof(TipCount_Control),new PropertyMetadata(0, CountChanged));
        public TipCount_Control()
        {
            InitializeComponent();
        }

        private static void CountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is TipCount_Control _this)
            {
                _this.CountLab.Text = ((int)e.NewValue).ToString("D4");
            }
        }
        
        
        public int Count
        {
            get
            {
                return (int)GetValue(CountProperty);
            }
            set
            {
                SetValue(CountProperty, value);
            }
        }
    }
}
