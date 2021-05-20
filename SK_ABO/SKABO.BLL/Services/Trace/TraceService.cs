using SKABO.BLL.IServices.ITrace;
using SKABO.Common;
using SKABO.Common.Enums;
using SKABO.Common.Models.Trace;
using SKABO.Common.Parameters.Trace;
using SKABO.DAL.IDAO.ITrace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.BLL.Services.Trace
{
    public class TraceService : ITraceService
    {
        private ITraceDAO traceDAO;
        public TraceService(ITraceDAO traceDAO)
        {
            this.traceDAO = traceDAO;
        }
        public bool InsertT_Trace(T_Trace t_Trace)
        {
            if (t_Trace.TraceLevel <= (byte)Constants.TraceLevel)
            {
                return traceDAO.InsertT_Trace(t_Trace);
            }
            return true;
        }
        public bool InsertT_Trace(TraceLevelEnum level,String TraceContent)
        {
            if (Constants.Login == null) return true;
            var t_Trace = new T_Trace() { TraceStr=TraceContent,TraceTime=DateTime.Now,TraceUser=Constants.Login.LoginName,TraceLevel=(byte)level};
            return traceDAO.InsertT_Trace(t_Trace);
        }
        public bool InsertT_Trace(String TraceContent)
        {
            return InsertT_Trace(TraceLevelEnum.HightImportant, TraceContent);
        }

        public IList<T_Trace> QueryT_Trace(TraceParameter traceParameter)
        {
            if (traceParameter.TraceTimeEnd.HasValue)
            {
                traceParameter.RealTraceTimeEnd = traceParameter.TraceTimeEnd.Value.AddDays(1);
            }
            else
            {
                traceParameter.RealTraceTimeEnd = null;
            }
            return traceDAO.QueryT_Trace(traceParameter);
        }
    }
}
