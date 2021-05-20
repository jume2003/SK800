using SKABO.Common.Models.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;

namespace SK_ABO.Views.LogicProgram.LogicStep
{
    public class HexViewModel:LogicStepScreen
    {
        public HexCommand Command { get; set; }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            Command = Command ?? new HexCommand();
        }
        public override T_LogicStep GetStep()
        {
            this.Parameters = this.Command.ToJsonStr<HexCommand>();
            return base.GetStep();
        }
        public override void ParseLogicStep(T_LogicStep Step)
        {
            base.ParseLogicStep(Step);
            Command = Parameters.ToInstance<HexCommand>();
        }
    }
}
