using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Enums
{
    public enum TraceLevelEnum
    {
        /// <summary>
        /// 一般操作
        /// </summary>
        Info=5,
        /// <summary>
        /// 低重要
        /// </summary>
        LowImportant = 4,
        /// <summary>
        /// 重要
        /// </summary>
        Important=3,
        /// <summary>
        /// 非常重要
        /// </summary>
        VeryImportant=2,
        /// <summary>
        /// 非常非常重要
        /// </summary>
        HightImportant=1
    }
}
