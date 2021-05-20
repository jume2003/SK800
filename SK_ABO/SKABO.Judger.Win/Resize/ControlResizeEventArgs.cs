using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Judger.Win.Resize
{
    class ControlResizeEventArgs : EventArgs
    {
        public double HorizontalChange { get; private set; }
        public double VerticalChange { get; private set; }
        public bool? LeftDirection { get; private set; }
        public bool? TopDirection { get; private set; }
        public object TargetObject { get; private set; }

        public override string ToString()
        {
            return String.Format("{0}({1}) {2}({3})", LeftDirection, HorizontalChange, TopDirection, VerticalChange);
        }

        public ControlResizeEventArgs(object TargetObject,double hori, double verti, bool? lefdir, bool? topdir)
        {
            this.TargetObject = TargetObject;
            HorizontalChange = hori;
            VerticalChange = verti;
            LeftDirection = lefdir;
            TopDirection = topdir;
        }
    }
}
