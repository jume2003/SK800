using SKABO.Judger.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SKABO.Judger.Core.Models
{
    /// <summary>
    /// 微胶柱问号先决
    /// </summary>
    public class UnknownResult
    {
        private String _ToUnknownStr;
        public UnknownResult(String ToUnknownStr)
        {
            this._ToUnknownStr = ToUnknownStr;
            if (String.IsNullOrEmpty(ToUnknownStr))
                return;
            Regex reg = new Regex(@"([\-]?\d{1})");
            var mats=reg.Matches(ToUnknownStr);
            for(int i = 0; i < mats.Count; i++)
            {
                String strVal = mats[i].Value;
                switch (strVal)
                {
                    case "4":
                        {
                            Positive4 = true;
                            break;
                        }
                    case "3":
                        {
                            Positive3 = true;
                            break;
                        }
                    case "2":
                        {
                            Positive2 = true;
                            break;
                        }
                    case "1":
                        {
                            Positive1 = true;
                            break;
                        }
                    case "0":
                        {
                            Positive = true;
                            break;
                        }
                    case "-2":
                        {
                            BadSample_H = true;
                            break;
                        }
                    case "-3":
                        {
                            BadSample_PH = true;
                            break;
                        }
                    case "-4":
                        {
                            BadSample_DCP = true;
                            break;
                        }
                }
            }
        }
        public bool Positive4 { get; set; }
        public bool Positive3 { get; set; }
        public bool Positive2 { get; set; }
        public bool Positive1 { get; set; }
        public bool Positive { get; set; }
        public bool BadSample_H { get; set; }
        public bool BadSample_PH { get; set; }
        public bool BadSample_DCP { get; set; }
        public override string ToString()
        {
            return _ToUnknownStr;
        }
        public void Refresh()
        {
            String val = "";
            val += Positive4 ? "4" : "";
            val += Positive3 ? "3" : "";
            val += Positive2 ? "2" : "";
            val += Positive1 ? "1" : "";
            val += Positive ? "0" : "";
            val += BadSample_H ? "-2" : "";
            val += BadSample_PH ? "-3" : "";
            val += BadSample_DCP ? "-4" : "";
            _ToUnknownStr = val;
        }
        public ResultEnum FilterResult(ResultEnum result)
        {
            if (String.IsNullOrEmpty(_ToUnknownStr))
                return result;
            String val = ((int)result).ToString();
            if (val.Length == 2)
                return _ToUnknownStr.Contains(val) ? ResultEnum.BadSample_Ambiguous : result;
            int postion = _ToUnknownStr.IndexOf(val);
            if (postion > 0)
            {
                if ("-" == _ToUnknownStr.ElementAt(postion - 1).ToString())
                {
                    return result;
                }
                else
                {
                    return ResultEnum.BadSample_Ambiguous;
                }
            }
            else
            {
                return result;
            }

        }
    }
}
