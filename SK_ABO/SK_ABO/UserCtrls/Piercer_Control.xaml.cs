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
    /// Piercer_Control.xaml 的交互逻辑
    /// 打孔器
    /// </summary>
    public partial class Piercer_Control : UserControl
    {
        private bool HasLoaded = false;
        public static readonly DependencyProperty PierceNameProperty = DependencyProperty.Register("PierceName", typeof(String), typeof(Piercer_Control), null, null);
        public String PierceName
        {
            get { return (String)GetValue(PierceNameProperty); }
            set { SetValue(PierceNameProperty, value); }
        }
        public Piercer_Control()
        {
            InitializeComponent();
        }

        private void Piercer_Loaded(object sender, RoutedEventArgs e)
        {
            if (HasLoaded) return;
            HasLoaded = true;
            rect.Width = this.Width - 5*2;
        }
    }
}
