using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Trace
{
    public class T_Trace
    {
        public long ID { get; set; }
        public DateTime TraceTime { get; set; }
        public string TraceUser { get; set; }
        public string TraceStr { get; set; }
        public byte TraceLevel { get; set; }
    }
}
