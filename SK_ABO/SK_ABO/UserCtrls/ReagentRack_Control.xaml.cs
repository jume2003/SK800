using SKABO.Common.Models.BJ;
using System;
using System.Collections.Generic;
using System.Linq;
using SKABO.Common.Utils;
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
using SK_ABO.UserCtrls.Base;

namespace SK_ABO.UserCtrls
{
    /// <summary>
    /// ReagentRack_Control.xaml 的交互逻辑
    /// </summary>
    public partial class ReagentRack_Control : BJControl
    {
        private bool HasLoaded;
        public int Count { get; set; }
        public ReagentRack_Control()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (HasLoaded) return;
            HasLoaded = true;
            for (int x = 0; x < Count; x++)
            {
                var ele = new Ellipse()
                {
                    Width = this.Width - 2 * 3,
                    Height = this.Width - 2 * 3,
                    Stroke = Brushes.Black,
                    Fill = Brushes.White,
                    Name = String.Format("STRack_{0}", x),
                    Margin = new Thickness(2, 3 , 0, 0)
                };
                MainPanel.Children.Add(ele);
                var lab = new TextBlock()
                {
                    Text = (x + 1).ToString(),
                    Height = this.Width,
                    Width=this.Width,
                    TextAlignment=TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };
                
                MainPanel.Children.Add(lab);
               // StackPanel.SetZIndex(lab, 1000 + x);
            }
            if (DataContext is VBJ vbj)
            {
                vbj.ChangedValueMap = ChangedSeatVlaue;
            }
            base.RaiseAddedControls();
        }
        public void ChangedSeatVlaue(ChangeBJEventArgs e)
        {
            var ele=this.GetControl<Ellipse>(String.Format("STRack_{0}", e.X));
            ele.SetResourceReference(Ellipse.FillProperty, e.NewVal != null?"BJFillUsed": "BJFillUnUsed");
            ele.ToolTip = e.NewVal;
        }

    }
}
