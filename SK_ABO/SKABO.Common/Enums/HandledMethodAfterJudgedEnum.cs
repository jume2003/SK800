using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Enums
{
    public enum HandledMethodAfterJudgedEnum
    {
        /// <summary>
        /// 仅待定卡留置
        /// </summary>
        [Description("仅待定卡留置")]
        Only_Unknown_Stay =0,
        /// <summary>
        /// 所有卡留置
        /// </summary>
        [Description("所有卡留置")]
        All_Stay =1,
        /// <summary>
        /// 直接丢弃
        /// </summary>
        [Description("直接丢弃")]
        No_Stay =2
    }
}
