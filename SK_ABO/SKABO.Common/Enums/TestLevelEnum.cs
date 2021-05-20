using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Enums
{
    /// <summary>
    /// 测试样本的紧急程度，共分三级，Normal,Hight,SuperHight
    /// </summary>
    public enum TestLevelEnum
    {
        /// <summary>
        /// 一般
        /// </summary>
        Normal=0,
        /// <summary>
        /// 加急
        /// </summary>
        Hight=1,
        /// <summary>
        /// 特急
        /// </summary>
        SuperHight=2
    }
}
