using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Logic
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class CommonAction
    {
        public int? ID { get; set; }
        /// <summary>
        /// 离心机代号
        /// </summary>
        public String Code{get;set;}
        /// <summary>
        /// 卡位号
        /// </summary>
        public byte SeatIndex { get; set; }
        /// <summary>
        /// 离心机动作
        /// 0：正常离心程序
        /// 1：移动指定卡位
        /// 2：开舱门
        /// 3：关舱门
        /// 打孔器，0：开孔
        /// </summary>
        public byte DoAction { get; set; }
    }
}
