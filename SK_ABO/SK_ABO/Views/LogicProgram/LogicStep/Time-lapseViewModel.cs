using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKABO.Common.Models.Logic;

namespace SK_ABO.Views.LogicProgram.LogicStep
{
    public class Time_lapseViewModel : LogicStepScreen
    {
        public int DelayTime { get; set; }
        public override T_LogicStep GetStep()
        {
            Parameters = DelayTime.ToString();
            return base.GetStep();
        }
        public override void ParseLogicStep(T_LogicStep Step)
        {
            base.ParseLogicStep(Step);
            int val = 0;
            int.TryParse(this.Parameters, out val);
            DelayTime = val;
        }
    }
}
