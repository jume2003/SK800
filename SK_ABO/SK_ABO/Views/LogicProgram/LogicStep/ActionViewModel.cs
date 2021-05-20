using SKABO.Common.Models.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;

namespace SK_ABO.Views.LogicProgram.LogicStep
{
    public class ActionViewModel:LogicStepScreen
    {
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            Action = Action ?? new SimpleAction();
        }
        public SimpleAction Action { get; set; }
        public override T_LogicStep GetStep()
        {
            this.Parameters = Action.ToJsonStr<SimpleAction>();
            return base.GetStep();
        }
        public override void ParseLogicStep(T_LogicStep Step)
        {
            base.ParseLogicStep(Step);
            Action = Parameters.ToInstance<SimpleAction>();
        }
    }
}
