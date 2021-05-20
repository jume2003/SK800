using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Communication.Unit
{
    /// <summary>
    /// 混匀器
    /// </summary>
    public class Mixer:Electromotor
    {
        public Mixer() : base()
        {
            TimeSpan = new PLCParameter<short>();
        }
        /// <summary>
        /// 定时器
        /// </summary>
        public PLCParameter<short> TimeSpan { get; set; }
    }
}
