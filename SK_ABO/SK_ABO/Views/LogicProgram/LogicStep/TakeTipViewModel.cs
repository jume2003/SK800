using SKABO.Common;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.Communication;
using SKABO.Common.Models.Logic;
using SKABO.Hardware.RunBJ;
using System;
using System.Collections.Generic;
using System.Linq;
using SKABO.Common.Utils;
using System.Threading.Tasks;

namespace SK_ABO.Views.LogicProgram.LogicStep
{
    /// <summary>
    /// 装吸头
    /// </summary>
    class TakeTipViewModel:LogicStepScreen
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
        public VBJ TakeBJ { get; set; }
        public TakeTipAction TakeTip { get; set; }
        public Stylet.BindableCollection<VBJ> BJList { get; set; }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            TakeTip = TakeTip ?? new TakeTipAction();
            if (BJList == null)
            {
                BJList = new Stylet.BindableCollection<VBJ>();
                var Key = typeof(T_BJ_Tip).Name;
                
                    if (Constants.BJDict.ContainsKey(Key))
                        BJList.AddRange(Constants.BJDict[Key]);
            }
        }
        public override T_LogicStep GetStep()
        {
            if (TakeBJ?.ID > 0)
            {
                TakeTip.Indexs = injectorDevice.GetSeleteced().Select(et => et.Index).ToArray();
                TakeTip.TipBoxName = TakeBJ.Name;
            }
            else
            {
                TakeTip.R = 0;
                TakeTip.C = 0;
                TakeTip.TipBoxName = null;
            }
            this.Parameters = TakeTip.ToJsonStr<TakeTipAction>();
            return base.GetStep();
        }
        public override void ParseLogicStep(T_LogicStep Step)
        {
            base.ParseLogicStep(Step);
            TakeTip = this.Parameters.ToInstance<TakeTipAction>();
            foreach (var ent in injectorDevice.Injector.Entercloses)
            {
                ent.Selected = false;
            }
            foreach (byte i in TakeTip.Indexs)
            {
                if(i<Constants.EntercloseCount)
                    injectorDevice.Injector.Entercloses[i].Selected = true;
            }
            if (!String.IsNullOrEmpty(TakeTip.TipBoxName) && Constants.BJDict.ContainsKey(typeof(T_BJ_Tip).Name))
            {
                TakeBJ = Constants.BJDict[typeof(T_BJ_Tip).Name].Where(c => c.Name == TakeTip.TipBoxName).FirstOrDefault();
            }
        }
    }
}
