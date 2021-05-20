using SKABO.Common.Enums;
using SKABO.Common.Models.Trace;
using SKABO.Common.Parameters.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.BLL.IServices.ITrace
{
    public interface ITraceService
    {
        bool InsertT_Trace(T_Trace t_Trace);
        bool InsertT_Trace(TraceLevelEnum level,String TraceContent);
        bool InsertT_Trace( String TraceContent);
        IList<T_Trace> QueryT_Trace(TraceParameter traceParameter);
    }
}
