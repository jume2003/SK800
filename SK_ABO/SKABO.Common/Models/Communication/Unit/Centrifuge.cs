using SKABO.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using SKABO.Common.Utils;
using System.Threading;
using System.Threading.Tasks;
using SKABO.Common.Models.BJ;

namespace SKABO.Common.Models.Communication.Unit
{
    /// <summary>
    /// 离心机参数
    /// </summary>
    public class Centrifuge
    {
        private T_BJ_Centrifuge _BJParam;
        [Newtonsoft.Json.JsonIgnore]
        public T_BJ_Centrifuge BJParam{
            get
            {
                if (_BJParam == null)
                {
                    _BJParam = Constants.BJDict[typeof(T_BJ_Centrifuge).Name].Where(c => (c as T_BJ_Centrifuge).Code == this.Code && this.Code!=null).FirstOrDefault() as T_BJ_Centrifuge;
                }
                return _BJParam;
            }
            }
        private CentrifugeStatusEnum _StatusEnum;
            [Newtonsoft.Json.JsonIgnore]
        public int Time { get; set; }
        /// <summary>
        /// 离心机状态信息
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public CentrifugeStatusEnum StatusEnum
        {
            get
            {
                return _StatusEnum;
            }
            set
            {
                _StatusEnum=value;
                ChangeStatusEvent?.Invoke(this, new CentrifugeStatusChangeEventArg() { Code = this.Code, StatusEnum = this.StatusEnum, Time = this.Time });
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        public Semaphore TimerSemaphore;
        private System.Threading.Timer _RunTimer;
        [Newtonsoft.Json.JsonIgnore]
        public System.Threading.Timer RunTimer{get
            {
                if (_RunTimer == null)
                {
                    _RunTimer = new System.Threading.Timer(Tick_tick, null,System.Threading.Timeout.Infinite, Timeout.Infinite);
                }
                return _RunTimer;
            }
        }
        public delegate void ChangeStautsHandler(Centrifuge centrifuge, CentrifugeStatusChangeEventArg e);
        public event ChangeStautsHandler ChangeStatusEvent;
        private void Tick_tick(Object obj)
        {
            this.Time--;
            ChangeStatusEvent?.Invoke(this, new CentrifugeStatusChangeEventArg() {Code=this.Code,StatusEnum=this.StatusEnum,Time=this.Time });
            if (this.Time <= 0)
            {
                this.RunTimer.Change(Timeout.Infinite, Timeout.Infinite);
                Console.WriteLine("停止计时");
                TimerSemaphore.Release(1);
            }
        }
        public Centrifuge()
        {
            TimerSemaphore = new Semaphore(0,1);
            InitCoil =new PLCParameter<bool>();
            Zero = new PLCParameter<bool>();
            Angle = new PLCParameter<float>();
            AngleFactor = new PLCParameter<float>();
            CurrentSpeed = new PLCParameter<int>();
            SpeedFactor = new PLCParameter<float>();
            AngleStartCoil = new PLCParameter<bool>();
            DoneCoil = new PLCParameter<bool>();
            LowAction = new CentrifugeAction();
            HighAction = new CentrifugeAction();
            EndAction = new CentrifugeAction();
            Speed = new PLCParameter<float>();
        }
        /// <summary>
        /// 离心机代号，与部件参数中代号相对应
        /// </summary>
        public String Code { get; set; }
        /// <summary>
        /// 初始化线圈
        /// </summary>
        public PLCParameter<Boolean> InitCoil { get; set; }
        /// <summary>
        /// 零点
        /// </summary>
        public PLCParameter<Boolean> Zero { get; set; }
        /// <summary>
        /// 旋转角度
        /// </summary>
        public PLCParameter<float> Angle { get; set; }
        /// <summary>
        /// 角度校正因子
        /// </summary>
        public PLCParameter<float> AngleFactor { get; set; }
        
        /// <summary>
        /// 速度校正因子
        /// </summary>
        public PLCParameter<float> SpeedFactor { get; set; }
        /// <summary>
        /// 定位速度(转/分)
        /// </summary>
        public PLCParameter<float> Speed { get; set; }
        /// <summary>
        /// 当前转速(转/分)
        /// </summary>
        public PLCParameter<int> CurrentSpeed { get; set; }
        
        /// <summary>
        /// 完成线圈
        /// </summary>
        public PLCParameter<Boolean> DoneCoil { get; set; }
        
        
        /// <summary>
        /// 按角度旋转起动线圈
        /// </summary>
        public PLCParameter<Boolean> AngleStartCoil { get; set; }
        /// <summary>
        /// 低速离心
        /// </summary>
        public CentrifugeAction LowAction { get; set; }
        /// <summary>
        /// 高速离心参数
        /// </summary>
        public CentrifugeAction HighAction { get; set; }
        /// <summary>
        /// 降速参数
        /// </summary>
        public CentrifugeAction EndAction { get; set; }
        /// <summary>
        /// 开门速度
        /// </summary>
        public int OpenDoorSpeed { get; set; }
    }
}
