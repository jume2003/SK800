using SKABO.Common.Enums;
using SKABO.Common.Models.TestStep;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;

namespace SK_ABO.Pages.SetStep
{
    public class LXJDZViewModel:Screen
    {
        public LXJDZStepParameter Param { get; set; }
        public String StepParameter
        {
            get
            {
                return Param == null ? "" : Param.ToString();
            }
            set
            {
                Param = new LXJDZStepParameter(value);
            }
        }
        public IDictionary<CentrifugeActionEnum, String> Actions
        {
            get
            {
                var r = new Dictionary<CentrifugeActionEnum, String>();
                foreach(CentrifugeActionEnum item in Enum.GetValues(typeof(CentrifugeActionEnum)))
                {
                    if(item== CentrifugeActionEnum.Start)
                        r.Add(item, item.GetDescription());
                }
                return r;
            }
        }
    }
}
