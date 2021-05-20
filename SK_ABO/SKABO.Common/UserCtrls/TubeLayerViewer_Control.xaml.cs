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

namespace SKABO.Common.UserCtrls
{
    /// <summary>
    /// TubeLayerViewer_Control.xaml 的交互逻辑
    /// </summary>
    public partial class TubeLayerViewer_Control : UserControl
    {
        private int[] _layerHeights;
        public TubeLayerViewer_Control()
        {
            InitializeComponent();
        }

        public int[] LayerHeights { get => _layerHeights; set => _layerHeights = value; }
        public TubeLayerViewer_Control(int[] LayerHeights)
        {
            InitializeComponent();
            this.LayerHeights = LayerHeights;
        }

        private void TubeLayerViewer_Loaded(object sender, RoutedEventArgs e)
        {
            if (LayerHeights == null) return;
            if (Double.IsNaN(this.Height)) return;
            double Persent = 0;
            for (int i = 0; i < Math.Min(6, LayerHeights.Length); i++)
            {
                Persent += LayerHeights[i];
                switch (i)
                {
                    case 0:
                        {
                            layer1.Y2 = this.Height * (Persent / 100.0);
                            break;
                        }
                    case 1:
                        {
                            layer2.Y2 = this.Height * (Persent / 100.0);
                            break;
                        }
                    case 2:
                        {
                            layer3.Y2 = this.Height * (Persent / 100.0);
                            break;
                        }
                    case 3:
                        {
                            layer4.Y2 = this.Height * (Persent / 100.0);
                            break;
                        }
                    case 4:
                        {
                            layer5.Y2 = this.Height * (Persent / 100.0);
                            break;
                        }
                }
            }
        }
    }
    /// <summary>
    /// Value Conversion Class for the button height.
    /// Divides the Hight of the control by two to get the height for one button.
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class YConverter : IValueConverter
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
            var rate = System.Convert.ToDouble(parameter);
            return (double)value*rate / 100.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    }
