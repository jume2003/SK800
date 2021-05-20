using System;
using SKABO.Common.Utils;

namespace SKABO.Common.Models.TestStep
{
    public class FPBRXSHXBStepParameter
    {
        /// <summary>
        /// 分配容量
        /// </summary>
        public decimal Vol { get; set; }
        /// <summary>
        /// 液体类别
        /// </summary>
        public String LiquidType { get; set; }
        /// <summary>
        /// 加样深度
        /// </summary>
        public decimal Deep { get; set; }
        /// <summary>
        /// 液面探测加深
        /// </summary>
        public decimal DetectorDeep { get; set; }
        /// <summary>
        /// 微胶柱
        /// </summary>
        public int TubeValue { get; set; }
        /// <summary>
        /// 稀释液代码
        /// </summary>
        public String XSYCode { get; set; }
        /// <summary>
        /// 红细胞的起始量(ul)
        /// </summary>
        public decimal RedCellVol { get; set; }
        /// <summary>
        /// 吸红细胞等待时间(ms)
        /// </summary>
        public int WaitTimeForAbsorbRedCell { get; set; }
        /// <summary>
        /// 稀释液量(ul)
        /// </summary>
        public decimal XSYVol { get; set; }
        /// <summary>
        /// 稀释液吸液速度
        /// </summary>
        public int AbsorbSpeedOfXSY { get; set; }
        /// <summary>
        /// 红细胞吸液速度
        /// </summary>
        public int AbsorbSpeedOfRedCell { get; set; }
        /// <summary>
        /// 第一阶段混匀次数
        /// </summary>
        public int MixedCountFirst { get; set; }
        /// <summary>
        /// 第一阶段混匀容量
        /// </summary>
        public int MixedVolFirst { get; set; }
        /// <summary>
        /// 第二阶段混匀次数
        /// </summary>
        public int MixedCountSecond { get; set; }
        /// <summary>
        /// 第二阶段混匀容量
        /// </summary>
        public int MixedVolSecond { get; set; }
        /// <summary>
        /// 混匀流速
        /// </summary>
        public int MixedSpeed { get; set; }
        /// <summary>
        /// 手工配置深孔板稀释液
        /// </summary>
        public bool IsXSYByMan { get; set; }
        public override string ToString()
        {
            return this.ToJsonStr();
        }
    }
}
