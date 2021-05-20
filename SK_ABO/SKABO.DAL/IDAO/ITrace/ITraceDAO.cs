using SKABO.Common.Models.Trace;
using SKABO.Common.Parameters.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.IDAO.ITrace
{
    public interface ITraceDAO
    {
        bool InsertT_Trace(T_Trace t_Trace);
        IList<T_Trace> QueryT_Trace(TraceParameter traceParameter);
    }
}
