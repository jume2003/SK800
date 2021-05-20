using SKABO.Common.Models.Communication.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Communication
{
    /// <summary>
    /// 卡仓参数
    /// </summary>
    public class GelWarehouse
    {
        public GelWarehouse()
        {

        }
        public GelWarehouse(bool Init)
        {
            if (Init)
            {
                this.XMotor = new Electromotor();
                this.FirstCoil = new PLCParameter<short>();
                this.DoorCoil = new PLCParameter<bool>();
            }
        }
        /// <summary>
        /// 运动电机
        /// </summary>
        public Electromotor XMotor { get; set; }
        /// <summary>
        /// 检测光栅起始位置，及个数
        /// </summary>
        public PLCParameter<short> FirstCoil { get; set; }
        /// <summary>
        /// 卡仓门
        /// </summary>
        public PLCParameter<bool> DoorCoil { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public bool[] TestResults { get; set; }
        public bool this[byte index]
        {
            get
            {
                return TestResults[index];
            }
        }
    }
}
