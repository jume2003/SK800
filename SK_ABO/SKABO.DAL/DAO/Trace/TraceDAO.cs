using IBatisNet.DataMapper;
using SKABO.Common.Models.Trace;
using SKABO.Common.Parameters.Trace;
using SKABO.DAL.IDAO.ITrace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.DAO.Trace
{
    public class TraceDAO : ITraceDAO
    {
        ISqlMapper mapper { get; set; }
        public TraceDAO(ISqlMapper mapper)
        {
            this.mapper = mapper;
        }
        public bool InsertT_Trace(T_Trace t_Trace)
        {
            mapper.Insert("InsertT_Trace", t_Trace);
            return t_Trace.ID > 0;
        }

        public IList<T_Trace> QueryT_Trace(TraceParameter traceParameter)
        {
            return mapper.QueryForList<T_Trace>("QueryT_Trace", traceParameter);
        }
    }
}
