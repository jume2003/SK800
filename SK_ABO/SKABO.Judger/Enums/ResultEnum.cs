using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Judger.Enums
{
    public enum ResultEnum
    {
        /// <summary>
        /// 阳性 4+ 表示RBC绝大部分凝聚于凝胶表层
        /// </summary>
        [Description("4+")]
        Positive4 = 4,
        /// <summary>
        /// 阳性 3+ 表示RBC多数凝聚于凝胶表层，少数RBC凝集颗粒悬浮于近胶表层
        /// </summary>
        [Description("3+")]
        Positive3 = 3,
        /// <summary>
        /// 阳性 2+ 表示部分RBC沉积于胶底，部分RBC凝集颗粒悬浮于凝胶中
        /// </summary>
        [Description("2+")]
        Positive2 = 2,
        /// <summary>
        /// 阳性 1+ 表示RBC多数沉积于胶底，可见少数RBC凝集颗粒悬浮于凝胶中
        /// </summary>
        [Description("1+")]
        Positive1 = 1,
        /// <summary>
        /// 弱阳性 ± 表示RBC绝大多数沉积于胶底，极少数RBC凝集颗粒悬浮于近胶底
        /// </summary>
        [Description("±")]
        Positive = 0,
        /// <summary>
        /// 阴性 - 表示RBC全部沉积于胶底
        /// </summary>
        [Description("-")]
        Negative = -1,
        /// <summary>
        /// H 表示完全溶血，凝胶管中液体呈清澈透明红色，凝胶中无红细胞
        /// </summary>
        [Description("H")]
        BadSample_H = -2,
        /// <summary>
        /// PH 表示部分溶血，凝胶管中液体呈清澈透明红色，凝胶中有残留红细胞
        /// </summary>
        [Description("PH")]
        BadSample_PH = -3,
        /// <summary>
        /// DCP 表示混合凝集外观，部分红细胞凝集颗粒集结于凝胶表层，部分红细胞沉于胶底
        /// </summary>
        [Description("DCP")]
        BadSample_DCP = -4,
        /// <summary>
        /// 结果不清，需人工待定
        /// </summary>
        [Description("?")]
        BadSample_Ambiguous =-5
    }
}
