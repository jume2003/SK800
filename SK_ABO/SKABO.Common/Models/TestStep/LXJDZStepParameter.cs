using SKABO.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SKABO.Common.Utils;

namespace SKABO.Common.Models.TestStep
{
    public class LXJDZStepParameter
    {
        public CentrifugeActionEnum CentrifugeAction { get; set; }
        public LXJDZStepParameter(String paramStr)
        {
            if (String.IsNullOrEmpty(paramStr))
            {
                return;
            }
            Regex reg = new Regex(@"[\d]+");
            var m = reg.Match(paramStr);
            if (m != null)
            {
                if (!String.IsNullOrEmpty(m.Value))
                {
                    CentrifugeAction = (CentrifugeActionEnum)int.Parse(m.Value);
                }
            }
        }
        public override String ToString()
        {
            return String.Format("{0}({1})", CentrifugeAction.GetDescription(), (int)CentrifugeAction);
        }
    }
}
