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

namespace SK_ABO.Views.QueryStaInfo
{
    public class StaInfoResult
    {
        public string GelName { get; set; }
        public string Result { get; set; }
        public int Count { get; set; }
        public StaInfoResult(string gelname,string result,int count)
        {
            GelName = gelname;
            Result = result;
            Count = count;
        }
    }

    public class QueryStaInfoViewModel:Screen
    {
        private static Stylet.BindableCollection<StaInfoResult> _ResultList;
        public static Stylet.BindableCollection<StaInfoResult> ResultList
        {
            get
            {
                if (_ResultList == null)
                {
                    _ResultList = new BindableCollection<StaInfoResult>();
                }
                return _ResultList;
            }
        }
        public void SetStaInfo(Dictionary<string, int> statistcount)
        {
            ResultList.Clear();
            foreach(var item in statistcount)
            {
                string []values = item.Key.Split('\t');
                ResultList.Add(new StaInfoResult(values[0], values[1], item.Value));
            }
            ResultList.Refresh();
        }
    }
}
