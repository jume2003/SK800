using SKABO.BLL.IServices.ITrace;
using SKABO.Common.Models.Trace;
using SKABO.Common.Parameters.Trace;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK_ABO.Views.Trace
{
    public class TraceManageViewModel:Screen
    {
        [StyletIoC.Inject]
        private ITraceService TraceService;
        public TraceParameter Param { get; set; }
        public IList<T_Trace> TraceList { get; set; }

        protected override void OnViewLoaded()
        {
            Param = new TraceParameter() { TraceTimeEnd = DateTime.Today, TraceTimeStart = DateTime.Today };
            TraceList = TraceService.QueryT_Trace(Param);
            base.OnViewLoaded();

        }
        public void QueryTrace()
        {
            TraceList = TraceService.QueryT_Trace(Param);
        }
        public void Close()
        {
            this.RequestClose();
        }
    }
}
