using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Logic
{
    /// <summary>
    /// 探测液面
    /// </summary>
    public class DetectAction
    {
        public byte[] Indexs { get; set; }
        public decimal Start { get; set; }
        public decimal ZLimit { get; set; }
    }
}
