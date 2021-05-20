using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Communication.Unit
{
    public class DoubleSpeedMotor:Electromotor
    {
        public DoubleSpeedMotor() : base()
        {
            SecondSpeed = new PLCParameter<Int32>();
            DownSpeed = new PLCParameter<Int32>();
        }
        /// <summary>
        /// 第二速度
        /// </summary>
        public PLCParameter<Int32> SecondSpeed { get; set; }
        /// <summary>
        /// 下降速度
        /// </summary>
        public PLCParameter<Int32> DownSpeed { get; set; }

    }
}
