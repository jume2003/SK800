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
using SK_ABO.UserCtrls.Base;

namespace SK_ABO.UserCtrls
{
    /// <summary>
    /// TipDish_Control1.xaml 的交互逻辑
    /// 吸头盒
    /// </summary>
    public partial class TipDish_Control : BJControl
    {
        /// <summary>
        /// 是否在下面显示计数，True 显示计数，False 显示TipName
        /// </summary>
        public bool IsShowCount { get; set; }
        /// <summary>
        /// 使用计数
        /// </summary>
        private int UsedCount;
        private bool HasLoaded = false;
        public static readonly DependencyProperty CountXProperty = DependencyProperty.Register("CountX", typeof(int), typeof(TipDish_Control), null, null);
        public static readonly DependencyProperty CountYProperty = DependencyProperty.Register("CountY", typeof(int), typeof(TipDish_Control), null, null);
        public static readonly DependencyProperty TipNameProperty = DependencyProperty.Register("TipName", typeof(String), typeof(TipDish_Control), null, null);
        public int CountX
        {
            get
            {
                return (int)GetValue(CountXProperty);
            }
            set
            {
                SetValue(CountXProperty, value);
            }
        }
        public int CountY
        {
            get
            {
                return (int)GetValue(CountYProperty);
            }
            set
            {
                SetValue(CountYProperty, value);
            }
        }
        public String TipName
        {
            get
            {
                return (String)GetValue(TipNameProperty);
            }
            set
            {
                SetValue(TipNameProperty, value);
            }
        }
        public TipDish_Control()
        {
            InitializeComponent();
        }

        private void AddControls()
        {
            for (int y = 0; y < CountY; y++)
            {
                for (int x = 0; x < CountX; x++)
                {
                    var ele = new Ellipse();
                    ele.Width = 8;
                    ele.Height = 8;
                    ele.Stroke = Brushes.White;
                    ele.Fill =Brushes.White;
                    ele.Name = String.Format("Seat_{0}_{1}", x, y);

                    docCanvas.Children.Add(ele);
                    Canvas.SetLeft(ele, 1+x * 10);
                    Canvas.SetTop(ele, 1 + y * 10);
                }
            }

        }

        private void TipDishControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (HasLoaded) return;
            HasLoaded = true;
            AddControls();
            if (DataContext is VBJ vbj)
            {
                vbj.ChangedValueMap = ChangedSeatVlaue;
            }
            base.RaiseAddedControls();
        }
        private void ChangedSeatVlaue(ChangeBJEventArgs e)
        {
            var ele = this.GetControl<Ellipse>(String.Format("Seat_{0}_{1}", e.X, e.Y));
            
            ele.SetResourceReference(Ellipse.FillProperty, e.NewVal != null ? "BJFillUsed" : "BJFillUnUsed");
            if (e.OldVal ==null ^ e.NewVal==null)
            {
                UsedCount += (e.NewVal == null ? -1 : 1);
            }
            if (IsShowCount)
            {
                TipName = (CountX * CountY - UsedCount).ToString();
            }
        }
    }
}
