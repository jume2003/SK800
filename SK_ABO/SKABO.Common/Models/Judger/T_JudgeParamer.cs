using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Judger
{
    public class T_JudgeParamer
    {
        public int ID { get; set; }
        public string MSN { get; set; }
        public string AreaType { get; set; }
        public int TNo { get; set; }
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int EndX { get; set; }
        public int EndY { get; set; }
    }
}
