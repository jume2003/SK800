using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Judger.Enums
{
    public enum GelDisposeEnum
    {
        /// <summary>
        /// 所有卡留置
        /// </summary>
        [Description("所有卡留置")]
        ALL_GEL_PERSIST =0,
        /// <summary>
        /// 所有卡丢弃
        /// </summary>
        [Description("所有卡丢弃")]
        ALL_GEL_NO_PERSIST =1,
        /// <summary>
        /// 仅待定卡留置
        /// </summary>
        [Description("仅待定卡留置")]
        ONLY_UNCONFIRMED_GEL_PERSIST =2
    }
}
