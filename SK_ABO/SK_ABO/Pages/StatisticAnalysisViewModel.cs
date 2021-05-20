using SKABO.BLL.IServices.IJudger;
using SKABO.Common;
using SKABO.Common.Models.Judger;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKABO.BLL.IServices.IUser;
using SKABO.Common.Utils;
using SKABO.Common.Models.Duplex;
using System.IO;
using SKABO.Common.Views;
using SKABO.Hardware.RunBJ;
using SKABO.Common.Parameters.Judger;

namespace SK_ABO.Pages
{
    public class StatisticAnalysisInfo
    {
        public string GelName { get; set; }
        public int SampleCount { get; set; }
        public int GelCount { get; set; }
        public int Index { get; set; }
        public StatisticAnalysisInfo(string gelname, int samplecount, int gelcount,int index)
        {
            GelName = gelname;
            SampleCount = samplecount;
            GelCount = gelcount;
            Index = index;
        }
    }
    public class StatisticAnalysisTemInfo
    {
        public List<string> GelBarcode = new List<string>();
        public List<string> SampleBarcode = new List<string>();
    }

    class StatisticAnalysisViewModel:Screen
    {
        [StyletIoC.Inject]
        private IResultService resultService;

        public ResultParameter resultParameter { get; set; } = new ResultParameter { StartTime = DateTime.Today, EndTime = DateTime.Today };
        private List<T_Result> QueryResultList = new List<T_Result>();
        private Stylet.BindableCollection<StatisticAnalysisInfo> _ResultList = new BindableCollection<StatisticAnalysisInfo>();
        public Stylet.BindableCollection<StatisticAnalysisInfo> ResultList
        {
            get
            {
                return _ResultList;
            }
        }
        public void UpData()
        {
            ResultList.Clear();
            Dictionary<string, StatisticAnalysisTemInfo> dic_info = new Dictionary<string, StatisticAnalysisTemInfo>();
            foreach (var item in QueryResultList)
            {
                if(item.GelName!=null)
                {
                    if (!dic_info.ContainsKey(item.GelName))
                    {
                        dic_info.Add(item.GelName, new StatisticAnalysisTemInfo());
                    }
                    if (dic_info[item.GelName].GelBarcode.FindIndex(a => a == item.GelBarcode) == -1)
                        dic_info[item.GelName].GelBarcode.Add(item.GelBarcode);
                    if (dic_info[item.GelName].SampleBarcode.FindIndex(a => a == item.SmpBarcode) == -1)
                        dic_info[item.GelName].SampleBarcode.Add(item.SmpBarcode);
                }
            }
            int count = 0;
            foreach(var item in dic_info)
            {
                ResultList.Add(new StatisticAnalysisInfo(item.Key, item.Value.GelBarcode.Count,item.Value.SampleBarcode.Count, count+1));
                count++;
            }
            ResultList.Refresh();
        }
        /// <summary>
        /// 结果查询
        /// </summary>
        public void Query()
        {
            QueryResultList.Clear();
            QueryResultList.AddRange(resultService.QueryT_Result(resultParameter));
            UpData();
        }
        /// <summary>
        /// 重置查询条件
        /// </summary>
        public void ResetQuery()
        {
            resultParameter.Clear();
            resultParameter.StartTime = DateTime.Today;
            resultParameter.EndTime = DateTime.Today;
        }
    }
}
