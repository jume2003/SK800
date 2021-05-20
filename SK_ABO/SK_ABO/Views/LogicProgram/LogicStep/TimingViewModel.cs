using SKABO.Common.Models.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;
using SKABO.Common.UserCtrls;
using System.Windows.Data;

namespace SK_ABO.Views.LogicProgram.LogicStep
{
    public class TimingViewModel:LogicStepScreen
    {
        protected override void OnViewLoaded()
        {
            Timer=Timer ?? new LogicTimer();
        }
        public LogicTimer Timer { get; set; }
        public override T_LogicStep GetStep()
        {
            Parameters = Timer.ToJsonStr<LogicTimer>();
            return base.GetStep();
        }
        public override void ParseLogicStep(T_LogicStep Step)
        {
            base.ParseLogicStep(Step);
            Timer = this.Parameters.ToInstance<LogicTimer>();
        }
    }
}
