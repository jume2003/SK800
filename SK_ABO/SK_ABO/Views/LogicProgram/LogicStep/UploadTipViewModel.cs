using SKABO.Common;
using SKABO.Common.Models.Communication;
using SKABO.Common.Models.Logic;
using SKABO.Common.Utils;
using SKABO.Hardware.RunBJ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK_ABO.Views.LogicProgram.LogicStep
{
    public class UploadTipViewModel:LogicStepScreen
    {
        [StyletIoC.Inject]
        private InjectorDevice injectorDevice;
        public Injector injector
        {
            get
            {
                return injectorDevice.Injector;
            }
        }
        public OutTipAction OutTip { get; set; }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            OutTip = OutTip ?? new OutTipAction();
            
        }
        public override T_LogicStep GetStep()
        {

            OutTip.Indexs = injectorDevice.GetSeleteced().Select(et => et.Index).ToArray();
            this.Parameters = OutTip.ToJsonStr<OutTipAction>();
            return base.GetStep();
        }
        public override void ParseLogicStep(T_LogicStep Step)
        {
            base.ParseLogicStep(Step);
            OutTip = this.Parameters.ToInstance<OutTipAction>();
            foreach (var et in this.injector.Entercloses)
            {
                et.Selected = OutTip.Indexs.Contains(et.Index);
            }
        }
    }
}
