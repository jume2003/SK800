using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Enums
{
    /// <summary>
    /// 用户处理错误意见
    /// </summary>
    public enum UserResultEnum
    {
        /// <summary>
        /// 终止
        /// </summary>
        ABORT=0,
        /// <summary>
        /// 重试
        /// </summary>
        RETRY=1,
        /// <summary>
        /// 忽略
        /// </summary>
        IGNORE=2
    }
}
