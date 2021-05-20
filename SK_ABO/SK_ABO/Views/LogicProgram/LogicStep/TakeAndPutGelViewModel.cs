using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;
using SKABO.Common.Models.Logic;
using SKABO.Common.Models.BJ;
using SKABO.Common;

namespace SK_ABO.Views.LogicProgram.LogicStep
{
    public class TakeAndPutGelViewModel:LogicStepScreen
    {
        public VBJ TakeBJ { get; set; }
        public VBJ PutBJ { get; set; }
        public Stylet.BindableCollection<VBJ> BJList { get; set; }
        public GelAction TakeGel { get; set; }
        public GelAction PutGel { get; set; }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            TakeGel = TakeGel ?? new GelAction();
            PutGel = PutGel ?? new GelAction();
            if (BJList == null)
            {
                BJList = new Stylet.BindableCollection<VBJ>();
                String[] keys = new string[] { "T_BJ_GelSeat", "T_BJ_Centrifuge", "T_BJ_GelWarehouse", "T_BJ_Camera" };
                foreach (var Key in keys) {
                    if (Constants.BJDict.ContainsKey(Key))
                        BJList.AddRange(Constants.BJDict[Key]);
}
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
            if (PutBJ?.ID > 0)
            {
                PutGel.BJID = PutBJ.ID;
                PutGel.BJName = PutBJ.Name;
                PutGel.BJType = PutBJ.GetType().Name;
            }
            else
            {
                PutGel.BJID = 0;
                PutGel.BJName = null;
                PutGel.BJType = null;
            }
            this.Parameters = new GelAction[2] { TakeGel, PutGel }.ToJsonStr<GelAction[]>();
            return base.GetStep();
        }
        public override void ParseLogicStep(T_LogicStep Step)
        {
            base.ParseLogicStep(Step);
            var actions = this.Parameters.ToInstance<GelAction[]>();
            TakeGel = actions[0];
            PutGel = actions[1];
            if (!String.IsNullOrEmpty(TakeGel.BJType) && Constants.BJDict.ContainsKey(TakeGel.BJType))
            {
                TakeBJ = Constants.BJDict[TakeGel.BJType].Where(c => c.ID == TakeGel.BJID).FirstOrDefault();
            }
            if (!String.IsNullOrEmpty(PutGel.BJType) && Constants.BJDict.ContainsKey(PutGel.BJType))
            {
                PutBJ = Constants.BJDict[PutGel.BJType].Where(c => c.ID == PutGel.BJID).FirstOrDefault();
            }
        }
    }
}
