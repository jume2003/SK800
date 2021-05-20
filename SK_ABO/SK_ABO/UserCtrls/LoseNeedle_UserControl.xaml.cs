using Microsoft.Expression.Shapes;
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
    /// LoseNeedle_UserControl.xaml 的交互逻辑
    /// 脱针器
    /// </summary>
    public partial class LoseNeedle_UserControl : UserControl
    {
        private bool HasLoaded = false;
        public int Count { get; set; }
        public LoseNeedle_UserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (HasLoaded) return;
            HasLoaded = true;
            var gg = new GeometryGroup();
            double height = 10;
            for (int x = 0; x < Count; x++)
            {
                double StartX = this.Width;
                double StartY = 2 + (height+2) * x;
                var g = new PathGeometry();
                {
                    PathFigure pathFigure = new PathFigure();
                    pathFigure.StartPoint = new Point(StartX, StartY);
                    pathFigure.Segments.Add(new LineSegment(new Point(StartX - 10, StartY), true));
                    pathFigure.Segments.Add(new ArcSegment(new Point(StartX - 10, StartY + height), new Size(height/2, height/2), 0, false, SweepDirection.Counterclockwise, true));
                    pathFigure.Segments.Add(new LineSegment(new Point(StartX, StartY + height), true));
                    g.Figures.Add(pathFigure);
                    pathFigure.IsClosed = true;
                };
                gg.Children.Add(g);
            }
            Path ele = new Path()
            {
                Fill = Brushes.White,
                Stroke = Brushes.Red
            };
            ele.Data = gg;
                docCanvas.Children.Add(ele);
        }
    }
}
