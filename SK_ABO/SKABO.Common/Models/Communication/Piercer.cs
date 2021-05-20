using SKABO.Common.Models.Communication.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Communication
{
    /// <summary>
    /// 打孔器参数
    /// </summary>
    public class Piercer
    {
        public Piercer()
        { }
            /// <summary>
            /// 破孔器
            /// </summary>
            public Piercer(bool Init)
        {
            if (Init)
            {
                YMotor = new Electromotor();
                ZMotor = new Electromotor();
            }
        }
        /// <summary>
        /// 运动电机
        /// </summary>
        public Electromotor YMotor { get; set; }
        /// <summary>
        /// 打孔电机
        /// </summary>
        public Electromotor ZMotor { get; set; }
        
    }
}
