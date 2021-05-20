using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace SKABO.Judger.Win.UserCtrls
{
    class TubeThumb:Thumb
    {
        public TubeThumb() : base()
        {
            this.Focusable = true;
            this.KeyDown += TubeThumb_KeyDown;
            this.GotFocus += TubeThumb_GotFocus;
            this.LostFocus += TubeThumb_LostFocus;
        }

        private void TubeThumb_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            (Template.FindName("Lab_Type", this) as Label).Foreground = Brushes.Blue;
            (Template.FindName("Lab_Coordinate", this) as Label).Foreground = Brushes.Blue;
        }

        private void TubeThumb_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            //
            (Template.FindName("Lab_Type", this) as Label).Foreground = Brushes.OrangeRed;
            (Template.FindName("Lab_Coordinate", this) as Label).Foreground = Brushes.OrangeRed;
        }

        private void TubeThumb_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Right)
            {
                Canvas.SetLeft(this, Canvas.GetLeft(this)+1);
            }else if (e.Key == System.Windows.Input.Key.Left)
            {
                Canvas.SetLeft(this, Canvas.GetLeft(this) -1);
            }
            else if (e.Key == System.Windows.Input.Key.Up)
            {
                Canvas.SetTop(this, Canvas.GetTop(this) -1);
            }
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                Canvas.SetTop(this, Canvas.GetTop(this) + 1);
            }
            e.Handled = true;
            if (PositionChanged != null)
                PositionChanged(sender, new EventArgs());
        }
        public void ChangePosition(double left,double top)
        {
            Canvas.SetLeft(this, left);

            Canvas.SetTop(this, top);
            if (PositionChanged != null)
                PositionChanged(this, new EventArgs());
        }
        //定义委托
        public delegate void PositionChangedHandle(object sender, EventArgs e);
        public event PositionChangedHandle PositionChanged;
        public int X { get; set; }
        public int Y { get; set; }
        public int EndX { get; set; }
        public int EndY { get; set; }
        public double MarginX { get; set; }
        public double MarginY { get; set; }
        public String TipToolValue
        {
            get
            {
                var lab = this.Template==null?null: this.Template.FindName("Lab_Type", this);
                var coorLab = this.Template == null ? null : this.Template.FindName("Lab_Coordinate", this);
                String tip= (lab==null?"":((lab as Label).Content.ToString()))+"\r\n"+(coorLab==null?"": ((coorLab as Label).Content.ToString()));
                tip += String.Format("\r\n宽:{0},高:{1}", EndX - X, EndY - Y);
                return tip;
            }
        }
    }
}
