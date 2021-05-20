using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Parameters.Judger
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class ResultParameter
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 查询开始时才赋值，比TraceTimeEnd多一天
        /// </summary>
        public DateTime? RealEndTime { get; set; }
        public string GelBarcode { get; set; }
        public string SmpBarcode { get; set; }
        public string TestUser { get; set; }
        public string EditUser { get; set; }
        public string VerifyUser { get; set; }
        public string ReportUser { get; set; }
        public string DonorBarcode { get; set; }
        public int? GelID { get; set; }

        public void Clear()
        {
            var ps=this.GetType().GetProperties();
            foreach(var item in ps)
            {
                item.SetValue(this, null);
            }
        }
    }
}
