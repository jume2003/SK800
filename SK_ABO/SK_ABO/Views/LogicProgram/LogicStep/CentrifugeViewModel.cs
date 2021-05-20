using SKABO.Common;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.Logic;
using SKABO.Hardware.RunBJ;
using System;
using System.Collections.Generic;
using System.Linq;
using SKABO.Common.Utils;
using System.Threading.Tasks;

namespace SK_ABO.Views.LogicProgram.LogicStep
{
    class CentrifugeViewModel:LogicStepScreen
    {
        [StyletIoC.Inject]
        private CentrifugeDevice centrifugeDevice;
        private IList<VBJ> _CentrifugeList;
        public IList<VBJ> CentrifugeList { get {
               if(_CentrifugeList == null)
                {
                    String key = typeof(T_BJ_Centrifuge).Name;
                    if (Constants.BJDict.ContainsKey(key))
                    {
                        _CentrifugeList = Constants.BJDict[key].Where(item=>(item as T_BJ_Centrifuge).Status==1).ToList();
                    }
                    else
                    {
                        _CentrifugeList = new List<VBJ>();
                    }
                   
                }
                return _CentrifugeList;
            } }
        public VBJ SeletedBJ { get; set; }
        public CommonAction CentAction { get; set; }
        private IDictionary<byte, String> _ActionList;
        public IDictionary<byte, String> ActionList
        {
            get { if (_ActionList == null)
                {
                    this._ActionList = new Dictionary<byte, String>();
                    _ActionList.Add(0, "离心");
                    _ActionList.Add(1, "移动卡位");
                    _ActionList.Add(2, "开舱门");
                    _ActionList.Add(3, "关舱门");
                    _ActionList.Add(4, "初始化");
                }
                return _ActionList;
            }
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            CentAction = CentAction ?? new CommonAction() ;
            
        }
        private String GetActionName()
        {
            if (CentAction.DoAction == 0) return "离心";
            if (CentAction.DoAction == 1) return "移卡";
            if (CentAction.DoAction == 2) return "开舱门";
            if (CentAction.DoAction == 3) return "关舱门";
            return "";
        }
        public override T_LogicStep GetStep()
        {
            this.Name = $"离心机-{GetActionName()}";
            this.Parameters = CentAction.ToJsonStr<CommonAction>();
            return base.GetStep();
        }
        public override void ParseLogicStep(T_LogicStep Step)
        {
            base.ParseLogicStep(Step);
            CentAction = Parameters.ToInstance<SKABO.Common.Models.Logic.CommonAction>();
        }
    }
    
}
