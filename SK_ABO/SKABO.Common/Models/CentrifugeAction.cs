using SKABO.Common.Models.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models
{
    public class CentrifugeAction
    {
        public CentrifugeAction()
        {
            StartCoil = new PLCParameter<bool>();
            ToSpeedTime = new PLCParameter<short>();
            KeepSpeedTime = new PLCParameter<int>();
            Speed = new PLCParameter<float>();
        }
        /// <summary>
        /// 起动线圈
        /// </summary>
        public PLCParameter<Boolean> StartCoil { get; set; }
        /// <summary>
        /// 低速加速时间
        /// </summary>
        public PLCParameter<short> ToSpeedTime { get; set; }
        /// <summary>
        /// 低速保持时间
        /// </summary>
        public PLCParameter<int> KeepSpeedTime { get; set; }
        public PLCParameter<float> Speed { get; set; }
    }
}
