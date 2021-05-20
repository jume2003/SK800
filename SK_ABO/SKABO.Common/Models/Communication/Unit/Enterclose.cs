using System;

namespace SKABO.Common.Models.Communication.Unit
{
    /// <summary>
    /// 通道
    /// </summary>
    
    public class Enterclose
    {
        public Enterclose()
        {
        }
        public Enterclose(byte index):this()
        {
            this.Index = index;
            this.ZMotor = new DoubleSpeedMotor();
            this.YMotor = new DoubleSpeedMotor();
            this.PumpMotor = new DoubleSpeedMotor();
            this.Pressure = new PLCParameter<int>();
            this.ExistTipCoil = new PLCParameter<bool>();
            
            //this.ValidCoil = new PLCParameter<bool>();
        }
        /// <summary>
        /// 吸液速度与Z轴下降速度校正系统
        /// </summary>
        public decimal ZS_SpeedFactor { get; set; }
        /// <summary>
        /// 是否已稀释
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool XSed { get; set; }
        /// <summary>
        /// 是否已装针
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool TakedTip { get; set; }
        /// <summary>
        /// 通道Z轴的程序零点
        /// </summary>
        public decimal Zero { get; set; }
        /// <summary>
        /// 齿轮间隙
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public decimal GearGap { get; set; }
        /// <summary>
        /// 分针距离
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public decimal SplitDistance { get; set; }
        /// <summary>
        /// 是否选中，只在部件控制台中使用
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool Selected { get; set; }
        /// <summary>
        /// 零点
        /// </summary>
        public decimal YZero { get; set; }
        /// <summary>
        /// 针头的偏移
        /// </summary>
        public decimal TipDis { get; set; }
        /// <summary>
        /// 通道宽度
        /// </summary>
        public decimal InjWidth { get; set; }
        /// <summary>
        /// 通道宽度
        /// </summary>
        public bool InjEnable { get; set; }

        /// <summary>
        /// 逻辑通道索引0为最里面的一个通道，往外依次加一
        /// </summary>
        public byte Index { get; set; }
        
        public bool Valid { get;set; }
        /*
        [Newtonsoft.Json.JsonIgnore]
        public PLCParameter<bool> ValidCoil { get; set; }
        */
        /// <summary>
        /// Z轴马达
        /// </summary>
        public DoubleSpeedMotor ZMotor { get; set; }
        /// <summary>
        /// Y轴马达
        /// </summary>
        public DoubleSpeedMotor YMotor { get; set; }
        /// <summary>
        /// 吸液泵马达
        /// </summary>
        public DoubleSpeedMotor PumpMotor { get; set; }
        /// <summary>
        /// 是否存在Tip头在通道上
        /// </summary>
        public PLCParameter<Boolean> ExistTipCoil { get; set; }
        
        /// <summary>
        /// 压力信号值
        /// </summary>
        public PLCParameter<int> Pressure { get; set; }
        /// <summary>
        /// 基础压力值，每次试验时会在打孔的同时更新
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public int BasePressure
        {
            get;set;
        }
    }
}
