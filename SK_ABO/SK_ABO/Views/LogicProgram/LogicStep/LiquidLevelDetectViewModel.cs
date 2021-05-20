using SKABO.Common.Models.Logic;
using System;
using SKABO.Common.Utils;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKABO.Common.Models.Communication;
using SKABO.Hardware.RunBJ;

namespace SK_ABO.Views.LogicProgram.LogicStep
{
    public class LiquidLevelDetectViewModel:LogicStepScreen
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
        public DetectAction Detect { get; set; }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            Detect = Detect ?? new DetectAction();
            
        }
        public override T_LogicStep GetStep()
        {
            var ents=this.injectorDevice.GetSeleteced();
            if (ents.Length > 0)
            {
                Detect.Indexs = ents.Select(et => et.Index).ToArray();
            }
            
            this.Parameters = Detect.ToJsonStr<DetectAction>();
            return base.GetStep();
        }
        public override void ParseLogicStep(T_LogicStep Step)
        {
            base.ParseLogicStep(Step);
            Detect = this.Parameters.ToInstance<DetectAction>();
            if (Detect.Indexs != null)
            {
                injector.Entercloses.AsParallel().ForAll(et => et.Selected = Detect.Indexs.Contains(et.Index));
                
            }
        }
    }
}
