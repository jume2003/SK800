using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Judger
{
    public class T_Camera
    {
        

            public int ID { get; set; }
            public string MSN { get; set; }
            public int ExposureTime { get; set; }
            public int Gain { get; set; }
            public decimal RB { get; set; }
            public decimal GB { get; set; }
            public decimal BB { get; set; }
            public string Remark { get; set; }
    }
}
