using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.BJ
{
    public class ChangeBJEventArgs
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Object OldVal { get; set; }
        public Object NewVal { get; set; }
        public bool IsComplete { get; set; }
        public string Code { get; set; }
    }
}
