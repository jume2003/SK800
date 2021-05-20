using System;
using System.Collections.Generic;
using SKABO.Common.Utils;

namespace SKABO.Common.Models.TestStep
{
    public class FPYTUnit
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
        /// 吸液速度
        /// </summary>
        public decimal AbsSpeed { get; set; }
        /// <summary>
        /// 分液速度
        /// </summary>
        public decimal SpuSpeed { get; set; }
        /// <summary>
        /// 加深速度
        /// </summary>
        public decimal DeeSpeed { get; set; }
        /// <summary>
        /// 是否探液
        /// </summary>
        public bool IsDetector { get; set; }
        /// <summary>
        /// 是否试剂
        /// </summary>
        public bool IsAgentia { get; set; }
        /// <summary>
        /// 是否生理盐水
        /// </summary>
        public bool IsSlys { get; set; }
        /// <summary>
        /// 回吸
        /// </summary>
        public decimal BackCapacity { get; set; }
        /// <summary>
        /// 吸液回吸速度
        /// </summary>
        public decimal BackAbsSpeed { get; set; }
        /// <summary>
        /// 吸液回吸间隔
        /// </summary>
        public decimal BackAbsTime { get; set; }
        /// <summary>
        /// 分液回吸速度
        /// </summary>
        public decimal BackSpuSpeed { get; set; }
        /// <summary>
        /// 分液回吸间隔
        /// </summary>
        public decimal BackSpuTime { get; set; }
        /// <summary>
        /// 吸液压力
        /// </summary>
        public decimal AbsPressure { get; set; }
    }
    public class FPYTStepParameter
    {
        /// <summary>
        /// 每格容量
        /// </summary>
        public decimal Vol { get; set; }
        /// <summary>
        /// 微胶柱
        /// </summary>
        public int TubeValue { get; set; }
        /// <summary>
        /// 混合次数
        /// </summary>
        public decimal MixTimes { get; set; }
        /// <summary>
        /// 吸液速度
        /// </summary>
        public decimal AbsSpeed { get; set; }
        /// <summary>
        /// 分液速度
        /// </summary>
        public decimal SpuSpeed { get; set; }
        /// <summary>
        /// 混合深度
        /// </summary>
        public decimal MixDeep { get; set; }
        /// <summary>
        /// 混合容量
        /// </summary>
        public decimal MixCapacity { get; set; }
        /// <summary>
        /// 液体列表
        /// </summary>
        public List<FPYTUnit> LiquidList { get; set; } = new List<FPYTUnit>();
        public override string ToString()
        {
            return this.ToJsonStr();
        }
        //得到指定液体类型
        public string GetLiquidType(int index)
        {
            if(index< LiquidList.Count)
            return LiquidList[index].LiquidType;
            return "";
        }
        //是否有指定液体
        public bool FindLiquidType(string liquidtype)
        {
            foreach (var code_tem in LiquidList)
            {
                if (liquidtype.IndexOf("*") == -1)
                {
                    if (code_tem.LiquidType == liquidtype)
                        return true;
                }
                else
                {
                    var ncode = liquidtype.Replace("*", "");
                    if (code_tem.LiquidType.IndexOf(ncode) != -1)
                        return true;
                }
            }
            return false;
        }
    }
}
