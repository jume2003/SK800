using System;
using SKABO.Common.Utils;

namespace SKABO.Common.Models.TestStep
{
    /// <summary>
    /// 分配试剂参数
    /// </summary>
    public class FPSJStepParameter
    {
        public FPSJStepParameter()
        {
            
        }
        /// <summary>
        /// 容量
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
        /// 试剂识别码
        /// </summary>
        public String AgentCode { get; set; }
        /// <summary>
        /// 试剂混匀次数
        /// </summary>
        public int AgentMixCount { get; set; }
        /// <summary>
        /// 吸的流速
        /// </summary>
        public int AbsorbSpeed { get; set; }
        /// <summary>
        /// 分配的流速
        /// </summary>
        public int AllotSpeed { get; set; }
        /// <summary>
        /// 回吸量
        /// </summary>
        public decimal BackAbsVol { get; set; }
        /// <summary>
        /// 回吸速度
        /// </summary>
        public int BackSpeed { get; set; }
        /// <summary>
        /// 微胶柱
        /// </summary>
        public int TubeValue { get; set; }
        public override string ToString()
        {
           return this.ToJsonStr();
        }

    }
}
