using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Parameters.Trace
{
    public class TraceParameter
    {
        public DateTime? TraceTimeStart { get; set; }
        public DateTime? TraceTimeEnd { get; set; }
        /// <summary>
        /// 查询开始时才赋值，比TraceTimeEnd多一天
        /// </summary>
        public DateTime? RealTraceTimeEnd { get; set; }
        public string TraceUser { get; set; }
    }
}
