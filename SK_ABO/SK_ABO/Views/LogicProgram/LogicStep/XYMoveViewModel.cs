using SKABO.Common;
using SKABO.Common.Models.BJ;
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
    public class XYMoveViewModel:LogicStepScreen
    {
        [StyletIoC.Inject]
        private InjectorDevice injectorDevice;
        private IList<VBJ> _BjList;
        public TakeTipAction TakeTip { get; set; }
        public VBJ TakeBJ { get; set; }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            TakeTip = TakeTip ?? new TakeTipAction();
        }
        public IList<VBJ> BjList
        {
            get
            {
                if (_BjList == null)
                {
                    _BjList = new List<VBJ>();
                    String[] keys = new string[] { "T_BJ_Tip", "T_BJ_SampleRack", "T_BJ_AgentiaWarehouse" };
                    foreach (var Key in keys)
                    {
                        if (Constants.BJDict.ContainsKey(Key))
                            foreach(var bj in Constants.BJDict[Key])
                            {
                                _BjList.Add(bj);
                            }
                    }
                    if (Constants.BJDict.ContainsKey(typeof(T_BJ_GelSeat).Name))
                        foreach(var bj in Constants.BJDict[typeof(T_BJ_GelSeat).Name].Where(item => (item as T_BJ_GelSeat).Purpose == 0).ToArray())
                        {
                            _BjList.Add(bj);
                        }
                }
                return _BjList;
            }
           
        }
        public override T_LogicStep GetStep()
        {
            if (TakeBJ?.ID > 0)
            {
                //TakeTip.Indexs = injectorDevice.GetSeleteced().Select(et => et.Index).ToArray();
                TakeTip.TipBoxName = TakeBJ.Name;
                TakeTip.Key = TakeBJ.GetType().Name;
            }
            else
            {
                TakeTip.R = 0;
                TakeTip.C = 0;
                TakeTip.TipBoxName = null;
                TakeTip.Key = null;
            }
            this.Parameters = TakeTip.ToJsonStr<TakeTipAction>();
            return base.GetStep();
        }
        public override void ParseLogicStep(T_LogicStep Step)
        {
            base.ParseLogicStep(Step);
            TakeTip = this.Parameters.ToInstance<TakeTipAction>();
            
            if (!String.IsNullOrEmpty(TakeTip.TipBoxName) && Constants.BJDict.ContainsKey(TakeTip.Key))
            {
                TakeBJ = Constants.BJDict[TakeTip.Key].Where(c => c.Name == TakeTip.TipBoxName).FirstOrDefault();
            }
        }
    }
}
