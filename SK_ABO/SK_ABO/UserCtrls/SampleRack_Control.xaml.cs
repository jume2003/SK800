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
using SKABO.ResourcesManager;

namespace SK_ABO.UserCtrls
{
    /// <summary>
    /// SampleRack_Control.xaml 的交互逻辑
    /// 样本条架
    /// </summary>
    public partial class SampleRack_Control : BJControl
    {
        private bool HasLoaded = false;
        public static readonly DependencyProperty RackNameProperty = DependencyProperty.Register("RackName", typeof(String), typeof(SampleRack_Control), null, null);
        public String RackName
        {
            get
            {
                return (String)GetValue(RackNameProperty);
            }
            set { SetValue(RackNameProperty, value); }
        }
        public SampleRack_Control()
        {
            InitializeComponent();
        }
        public int Count { get; set; }
        private void Canvas_Loaded(object sender, RoutedEventArgs e)
        {
            if (HasLoaded) return;
            HasLoaded = true;
            if (sender is Canvas docCanvas)
            for (int x = 0; x < Count; x++)
            {
                var ele = new Ellipse()
                {
                    Width = this.Width- 2*3,
                    Height = this.Width - 2 * 3,
                    Stroke = Brushes.Black,
                    Fill = Brushes.White,
                    Name = String.Format("SRack_Ellipse_{0}", x),
                };
                docCanvas.Children.Add(ele);
                Canvas.SetLeft(ele, 2);
                Canvas.SetTop(ele,3+ x * this.Width);
            }

            if (DataContext is VBJ vbj)
            {
                vbj.ChangedValueMap = ChangedSeatVlaue;
                base.RaiseAddedControls();
            }
        }
        
        private void ChangedSeatVlaue(ChangeBJEventArgs e)
        {
            var ele = this.GetControl<Ellipse>(String.Format("SRack_Ellipse_{0}", e.X));
            if (ele == null) return;
            if (e.IsComplete)
            {
                ele.SetResourceReference(Ellipse.FillProperty, "CompleteStatus");
            }
            else
            {
                var reinfo = (ResInfoData)e.NewVal;
                bool is_ok = reinfo != null&& reinfo.GetCodeAt(0)!= "";
                ele.SetResourceReference(Ellipse.FillProperty, is_ok ? "BJFillUsed" : "BJFillUnUsed");
            }
        }
    }
}
