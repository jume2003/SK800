using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Judger
{
    public class T_Picture
    {
        public long ID { get; set; }
        public string RawFile { get; set; }
        public byte[] Tube1 { get; set; }
        public byte[] Tube2 { get; set; }
        public byte[] Tube3 { get; set; }
        public byte[] Tube4 { get; set; }
        public byte[] Tube5 { get; set; }
        public byte[] Tube6 { get; set; }
        public byte[] Tube7 { get; set; }
        public byte[] Tube8 { get; set; }
        public int T1 { get; set; }
        public int T2 { get; set; }
        public int T3 { get; set; }
        public int T4 { get; set; }
        public int T5 { get; set; }
        public int T6 { get; set; }
        public int T7 { get; set; }
        public int T8 { get; set; }
        public string LED { get; set; }
        public string MD5 { get; set; }
    }
}
