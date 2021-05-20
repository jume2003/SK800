using Newtonsoft.Json;
using System;
using SKABO.Common.Utils;

namespace SKABO.Common.Models.TestStep
{
    /// <summary>
    /// 分配病人血清参数
    /// </summary>
    public class FPBRXQStepParameter
    {
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
