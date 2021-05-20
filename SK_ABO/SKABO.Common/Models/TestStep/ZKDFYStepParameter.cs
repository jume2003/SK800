using SKABO.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SKABO.Common.Models.TestStep
{
    
    public class ZKDFYStepParameter
    {
        public int YsTime { get; set; }
        public ZKDFYStepParameter(String paramStr)
        {
            if (String.IsNullOrEmpty(paramStr))
            {
                YsTime = 180;
                return;
            }
            Regex reg = new Regex(@"[\d]+");
            var m = reg.Match(paramStr);
            if (m != null)
            {
                if (!String.IsNullOrEmpty(m.Value))
                {
                    YsTime = int.Parse(m.Value);
                }
            }
        }
        public override string ToString()
        {
            return this.ToJsonStr();
        }
    }
}
