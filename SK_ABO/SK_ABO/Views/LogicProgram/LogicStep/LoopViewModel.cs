using SKABO.Common.Models.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;

namespace SK_ABO.Views.LogicProgram.LogicStep
{
    public class LoopViewModel:LogicStepScreen
    {
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            Loop = Loop??new LogicLoop();
        }
        public LogicLoop Loop { get; set; }
        public override T_LogicStep GetStep()
        {
            Parameters = Loop.ToJsonStr<LogicLoop>();
            return base.GetStep();
        }
        public override void ParseLogicStep(T_LogicStep Step)
        {
            base.ParseLogicStep(Step);
            Loop = this.Parameters.ToInstance<LogicLoop>();
        }
    }
}
