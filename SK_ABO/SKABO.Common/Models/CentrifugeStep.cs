using SKABO.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models
{
    public class CentrifugeStep
    {
        public byte ID { get; set; }
        /// <summary>
        /// 时长 单位 秒
        /// </summary>
        public short? Time { get; set; }
        public float? TargetSpeed { get; set; }
        public bool IsKeep { get; set; }
        public CentrifugeActionEnum Action { get; set; }
    }
}
