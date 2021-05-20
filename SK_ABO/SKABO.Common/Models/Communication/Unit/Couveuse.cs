using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Communication.Unit
{
    /// <summary>
    /// 孵育器
    /// </summary>
    public class Couveuse
    {
        public Couveuse()
        {
            HotSwitchCoil = new PLCParameter<bool>();
            CurentTemp = new PLCParameter<float>();
            SetupTemp = new PLCParameter<float>();
            TempCompensate = new PLCParameter<float>();
        }
        /// <summary>
        /// 加热开关线圈
        /// </summary>
        public PLCParameter<bool> HotSwitchCoil { get; set; }
        /// <summary>
        /// 当前温度
        /// </summary>
        public PLCParameter<float> CurentTemp { get; set; }
        /// <summary>
        /// 设定温度
        /// </summary>
        public PLCParameter<float> SetupTemp { get; set; }
        /// <summary>
        /// 温度补偿
        /// </summary>
        public PLCParameter<float> TempCompensate { get; set; }
    }
}
