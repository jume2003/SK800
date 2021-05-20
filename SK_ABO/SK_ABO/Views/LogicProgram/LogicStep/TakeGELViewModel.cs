using SKABO.Common.Models.BJ;
using SKABO.Common.Models.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;
using SKABO.Common;

namespace SK_ABO.Views.LogicProgram.LogicStep
{
    class TakeGELViewModel:LogicStepScreen
    {
        public VBJ TakeBJ { get; set; }
        public GelAction TakeGel { get; set; }
        public Stylet.BindableCollection<VBJ> BJList { get; set; }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            TakeGel = TakeGel ?? new GelAction();
            if (BJList == null)
            {
                BJList = new Stylet.BindableCollection<VBJ>();
                String[] keys = new string[] { "T_BJ_GelSeat", "T_BJ_Centrifuge", "T_BJ_GelWarehouse" };
                foreach (var Key in keys)
                {
                    if (Constants.BJDict.ContainsKey(Key))
                        BJList.AddRange(Constants.BJDict[Key]);
                }
                if (Constants.BJDict.ContainsKey(typeof(T_BJ_WastedSeat).Name))
                    BJList.AddRange(Constants.BJDict[typeof(T_BJ_WastedSeat).Name].Where(item=>(item as T_BJ_WastedSeat).Purpose==1));
            }
        }
        public override T_LogicStep GetStep()
        {
            if (TakeBJ?.ID > 0)
            {
                TakeGel.BJID = TakeBJ.ID;
                TakeGel.BJName = TakeBJ.Name;
                TakeGel.BJType = TakeBJ.GetType().Name;
            }
            else
            {
                TakeGel.BJID = 0;
                TakeGel.BJName = null;
                TakeGel.BJType = null;
            }
            this.Parameters = TakeGel.ToJsonStr<GelAction>();
            return base.GetStep();
        }
        public override void ParseLogicStep(T_LogicStep Step)
        {
            base.ParseLogicStep(Step);
            TakeGel = this.Parameters.ToInstance<GelAction>();
            if (!String.IsNullOrEmpty(TakeGel.BJType) && Constants.BJDict.ContainsKey(TakeGel.BJType))
            {
                TakeBJ = Constants.BJDict[TakeGel.BJType].Where(c => c.ID == TakeGel.BJID).FirstOrDefault();
            }
        }
    }
}
