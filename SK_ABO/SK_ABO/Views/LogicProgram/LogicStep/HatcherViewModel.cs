using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SKABO.Common;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.Logic;
using SKABO.Common.Utils;

namespace SK_ABO.Views.LogicProgram.LogicStep
{
    class HatcherViewModel:LogicStepScreen
    {
        private IList<VBJ> _BjList;
        public IList<VBJ> BjList
        {
            get
            {
                if (_BjList == null)
                {
                    String key = typeof(T_BJ_GelSeat).Name;
                    if (Constants.BJDict.ContainsKey(key))
                    {
                        _BjList = Constants.BJDict[key].Where(item => (item as T_BJ_GelSeat).Purpose == 1).ToList();
                    }
                    else
                    {
                        _BjList = new List<VBJ>();
                    }

                }
                return _BjList;
            }
        }
        public VBJ SeletedBJ { get; set; }
        public CommonAction PieAction { get; set; }
        private IList<KeyValuePair<Byte, String>> _ActionList;
        public IList<KeyValuePair<Byte, String>> ActionList
        {
            get
            {
                if (_ActionList == null)
                {
                    _ActionList = new List<KeyValuePair<Byte, String>>();
                    _ActionList.Add(new KeyValuePair<byte, string>(Convert.ToByte(0), "启动"));
                    _ActionList.Add(new KeyValuePair<byte, string>(Convert.ToByte(0), "关闭"));
                }
                return _ActionList;
            }
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            PieAction = PieAction ?? new CommonAction();
        }
        public override T_LogicStep GetStep()
        {
            this.Parameters = PieAction.ToJsonStr<CommonAction>();
            return base.GetStep();
        }
        public override void ParseLogicStep(T_LogicStep Step)
        {
            base.ParseLogicStep(Step);
            PieAction = Parameters.ToInstance<SKABO.Common.Models.Logic.CommonAction>();
        }
    }
}
