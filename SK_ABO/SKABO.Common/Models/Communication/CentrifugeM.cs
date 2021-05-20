using SKABO.Common.Models.Communication.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Communication
{
    /// <summary>
    /// 离心机
    /// </summary>
    public class CentrifugeM 
    {
        public CentrifugeM()
        {
            HightSpeed = new PLCParameter<Int32>();
            LowSpeed = new PLCParameter<Int32>();
            HightSpeedTime = new PLCParameter<float>();
            LowSpeedTime = new PLCParameter<float>();
            AddLSpeedTime = new PLCParameter<float>();
            AddHSpeedTime = new PLCParameter<float>();
            StopSpeedTime = new PLCParameter<float>();
            Code = new PLCParameter<string>();
        }
        public CentrifugeM(bool Init)
        {
            if (!Init) return;
            this.Motor = new Electromotor();
        }
        public void CheckNull()
        {

        }
        /// <summary>
        /// 电机
        /// </summary>
        public Electromotor Motor { get; set; }
        /// <summary>
        /// 高速度
        /// </summary>
        public PLCParameter<Int32> HightSpeed { get; set; }
        /// <summary>
        /// 低速度
        /// </summary>
        public PLCParameter<Int32> LowSpeed { get; set; }
        /// <summary>
        ///高速时间            
        /// </summary>
        public PLCParameter<float> HightSpeedTime { get; set; }
        /// <summary>
        ///低速时间            
        /// </summary>
        public PLCParameter<float> LowSpeedTime { get; set; }
        /// <summary>
        ///加高速时间            
        /// </summary>
        public PLCParameter<float> AddHSpeedTime { get; set; }
        /// <summary>
        ///加低速时间           
        /// </summary>
        public PLCParameter<float> AddLSpeedTime { get; set; }
        /// <summary>
        ///停止时间           
        /// </summary>
        public PLCParameter<float> StopSpeedTime { get; set; }
        /// <summary>
        ///离心机代号           
        /// </summary>
        public PLCParameter<string> Code { get; set; }

    }
}
