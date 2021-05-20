using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Judger.Core.Models
{
    public class BloodCell
    {
        public BloodCell() { }
        public BloodCell(int R, int G, int B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
        }
        public int R { get; set; }
        public int B { get; set; }
        public int G { get; set; }
    }
}
