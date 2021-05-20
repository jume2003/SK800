using SKABO.Common.Models.Communication.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Communication
{
    /// <summary>
    /// 机械手参数
    /// </summary>
    public class MachineHand 
    {
        public MachineHand()
        { }
            public MachineHand(bool Init) {
            if (!Init) return;
            this.XMotor = new Electromotor();
            this.YMotor = new Electromotor();
            this.ZMotor = new DoubleSpeedMotor();
            this.HandCoil = new PLCParameter<bool>();
            ExistCoil = new PLCParameter<bool>();
            HandDonedCoil = new PLCParameter<bool>();
            HandInitCoil = new PLCParameter<bool>();
            HandStartCoil = new PLCParameter<bool>();
            HandStartAfter = new PLCParameter<Int32>();
            InitTime = new PLCParameter<float>();
            DistanceTime = new PLCParameter<float>();
        }
        public void CheckNull()
        {
            this.HandCoil = this.HandCoil ?? new PLCParameter<bool>();
            this.HandDonedCoil = this.HandDonedCoil ?? new PLCParameter<bool>();
            this.HandInitCoil = this.HandInitCoil ?? new PLCParameter<bool>();
            this.HandStartCoil = this.HandStartCoil ?? new PLCParameter<bool>();
            this.HandStartAfter = this.HandStartAfter ?? new PLCParameter<Int32>();
            this.InitTime = this.InitTime??new PLCParameter<float>();
            this.DistanceTime = this.DistanceTime??new PLCParameter<float>();
        }
        /// <summary>
        /// 夹手状态
        /// </summary>
        public bool IsOpen = false;
        /// <summary>
        /// X轴电机
        /// </summary>
        public Electromotor XMotor { get; set; }
        /// <summary>
        /// y轴电机
        /// </summary>
        public Electromotor YMotor { get; set; }
        /// <summary>
        /// Z轴电机
        /// </summary>
        public DoubleSpeedMotor ZMotor {get;set;}
        /// <summary>
        /// 夹手动作线圈
        /// </summary>
        public PLCParameter<Boolean> HandCoil { get; set; }
        /// <summary>
        /// 夹手动完成线圈
        /// </summary>
        public PLCParameter<Boolean> HandDonedCoil { get; set; }
        /// <summary>
        /// 夹手初始化线圈
        /// </summary>
        public PLCParameter<Boolean> HandInitCoil { get; set; }
        /// <summary>
        /// 夹手动启动线圈
        /// </summary>
        public PLCParameter<Boolean> HandStartCoil { get; set; }
        /// <summary>
        /// 是否夹到卡
        /// </summary>
        public PLCParameter<bool> ExistCoil { get; set; }
        /// <summary>
        /// 零点超时
        /// </summary>
        public PLCParameter<float> InitTime { get; set; }
        /// <summary>
        /// 移动超时
        /// </summary>
        public PLCParameter<float> DistanceTime { get; set; }
        /// <summary>
        /// 夹手始停步长
        /// </summary>
        public PLCParameter<Int32> HandStartAfter { get; set; }

    }
}
