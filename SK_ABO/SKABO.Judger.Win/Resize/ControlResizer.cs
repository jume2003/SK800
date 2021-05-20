using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SKABO.Judger.Win.Resize
{
    class ControlResizer
    {
        private FrameworkElement _control;
        bool _pressed;
        Point _prevPoint;
        public Thickness Thickness { get; private set; }
        public double Radius { get; private set; }
        public Cursor DefaultCursor { get; private set; }

        public bool? LeftDirection { get; private set; }
        public bool? TopDirection { get; private set; }
        public FrameworkElement Control { get => _control;private set => _control = value; }

        public ControlResizer(FrameworkElement control, Thickness thickness, double radius, Cursor defCursor)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            Thickness = thickness;
            Radius = radius;
            DefaultCursor = defCursor;
            Control = control;
            Control.MouseEnter += _control_MouseEnter;
            Control.MouseMove += _control_MouseMove;
            Control.MouseDown += _control_MouseDown;
            Control.MouseUp += _control_MouseUp;
        }

        public event EventHandler<ControlResizeEventArgs> Resize;

        protected virtual void OnResize(ControlResizeEventArgs e)
        {
            var handler = this.Resize;
            if (handler != null)
                handler(this, e);
        }

        void _control_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _pressed = false;
            Control.ReleaseMouseCapture();
        }

        void _control_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(Control);
            _pressed = true;
            Control.CaptureMouse();
            _prevPoint = Control.PointToScreen(point);
        }

        void _control_MouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(Control);
            if (!_pressed)
            {
                _SetCursor(point);
            }
            else
            {
                double vertiChange, horiChange;
                vertiChange = horiChange = 0;
                var pointScr = Control.PointToScreen(point);
                if (LeftDirection.HasValue)
                {
                    horiChange = pointScr.X - _prevPoint.X;
                    if (LeftDirection.Value)
                        horiChange *= -1;
                }
                if (TopDirection.HasValue)
                {
                    vertiChange = pointScr.Y - _prevPoint.Y;
                    if (TopDirection.Value)
                        vertiChange *= -1;
                }
                OnResize(new ControlResizeEventArgs(Control,horiChange, vertiChange, LeftDirection, TopDirection));
                _prevPoint = pointScr;
            }
        }

        void _control_MouseEnter(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(Control);
            if (!_pressed)
            {
                _SetCursor(point);
            }
        }

        void _SetCursor(Point point)
        {
            var left = point.X;
            var top = point.Y;
            var right = Control.ActualWidth - left;
            var bottom = Control.ActualHeight - top;

            LeftDirection = TopDirection = null;
            if (left < Thickness.Left)
                LeftDirection = true;
            else if (right < Thickness.Right)
                LeftDirection = false;

            if (top < Thickness.Top)
                TopDirection = true;
            else if (bottom < Thickness.Bottom)
                TopDirection = false;


            Cursor cur = null;
            if (LeftDirection.HasValue)
            {
                //如果上下没有进入区域，尝试按照Radius属性再次计算
                if (!TopDirection.HasValue)
                {
                    if (top < Radius)
                        TopDirection = true;
                    else if (bottom < Radius)
                        TopDirection = false;
                }

                if (TopDirection.HasValue)
                {
                    if (LeftDirection.Value == TopDirection.Value)
                        cur = Cursors.SizeNWSE;
                    else
                        cur = Cursors.SizeNESW;
                }
                else
                    cur = Cursors.SizeWE;
            }
            else if (TopDirection.HasValue)
            {
                //这里leftDirection.HasValue必定是false，所以直接计算
                if (left < Radius)
                    LeftDirection = true;
                else if (right < Radius)
                    LeftDirection = false;

                if (LeftDirection.HasValue)
                {
                    if (LeftDirection.Value == TopDirection.Value)
                        cur = Cursors.SizeNWSE;
                    else
                        cur = Cursors.SizeNESW;
                }
                else
                    cur = Cursors.SizeNS;
            }
            if (cur != null)
                Control.Cursor = cur;
            else
                Control.Cursor = DefaultCursor;
        }
    }
}
