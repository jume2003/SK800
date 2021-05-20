using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Judger
{
    public class T_ParseTubeParameter
    {
        public T_ParseTubeParameter()
        {
            Layer1 = 25;
            Layer2 = 18;
            Layer3 = 18;
            Layer4 = 10;
            Layer5 = 10;
            Layer6 = 19;
        }
        public int ID { get; set; }
        public decimal TSpace { get; set; }
        public int BottomHeight { get; set; }
        public int TestWidth { get; set; }
        public Byte Threshold { get; set; }

        public byte Angle { get; set; }

        public int Layer1 { get; set; }
        public int Layer2 { get; set; }
        public int Layer3 { get; set; }
        public int Layer4 { get; set; }
        public int Layer5 { get; set; }
        public int Layer6 { get; set; }

        public decimal HueMaxThreshold { get; set; }
        public decimal HueMinThreshold { get; set; }

        public decimal SMaxThreshold { get; set; }
        public decimal SMinThreshold { get; set; }

        public decimal BMaxThreshold { get; set; }
        public decimal BMinThreshold { get; set; }
    }
}
