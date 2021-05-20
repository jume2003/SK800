using SKABO.Common.Models.BJ;
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
using SKABO.Common.Utils;
using System.Windows.Shapes;
using SKABO.Common.Enums;
using SK_ABO.UserCtrls.Base;
using SKABO.ResourcesManager;

namespace SK_ABO.UserCtrls
{
    /// <summary>
    /// GelCardRack_Control.xaml 的交互逻辑
    /// Gel卡位及卡仓
    /// </summary>
    public partial class GelCardRack_Control : BJControl
    {
        private bool HasLoaded = false;
        public static readonly DependencyProperty CountProperty = DependencyProperty.Register("Count", typeof(int), typeof(GelCardRack_Control), null, null);
        public static readonly DependencyProperty GelNameProperty = DependencyProperty.Register("GelName", typeof(String), typeof(GelCardRack_Control), null, null);
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
        public String GelName
        {
            get
            {
                return (String)GetValue(GelNameProperty);
            }
            set
            {
                SetValue(GelNameProperty, value);
            }
        }
        public GelCardRack_Control()
        {
            InitializeComponent();
        }

        private void docCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            if (HasLoaded) return;
            HasLoaded = true;
            for (int x = 0; x < Count; x++)
            {
                var ele = new Rectangle()
                {
                    Width = this.Width - 2 * 3,
                    Height = 5,
                    Stroke = Brushes.White,
                    Fill = Brushes.White,
                    Name = String.Format("Seat_{0}", x)
                };
                docCanvas.Children.Add(ele);
                Canvas.SetLeft(ele, 3);
                Canvas.SetTop(ele, 2 + x * 7);
            }

            if (DataContext is VBJ vbj)
            {
                vbj.ChangedValueMap = ChangedSeatVlaue;
                base.RaiseAddedControls();
            }
        }


        private void ChangedSeatVlaue(ChangeBJEventArgs e)
        {
            var ele = this.GetControl<Rectangle>(String.Format("Seat_{0}", e.X));
            if(ele!=null)ele.SetResourceReference(Rectangle.FillProperty, e.NewVal != null ? "BJFillUsed" : "BJFillUnUsed");
        }

    }
}
