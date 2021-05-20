using System;
using System.Collections.Generic;

namespace SKABO.Common.Models.Communication.Unit
{
    public class ForecastMoveData
    {
        public double setp_time = 0;//一步用时
        public long beg_time = 0;//开始时间
        public bool is_bin = false;//是否已生成
        public double distance = 0;//移动距离
        public double tager = 0;//目标点

    }
    /// <summary>
    /// 马达
    /// </summary>
    public class Electromotor
    {
        public Electromotor()
        {
            this.InitCoil = new PLCParameter<bool>();
            Zero = new PLCParameter<Boolean>();
            Distance = new PLCParameter<float>() ;
            Speed = new PLCParameter<Int32>();
            Factor = new PLCParameter<float>();
            Maximum = new PLCParameter<float>();
            DoneCoil = new PLCParameter<Boolean>();
            StartCoil = new PLCParameter<Boolean>();
            InitSpeed = new PLCParameter<int>();
            InitDistance = new PLCParameter<float>();
            RealDistance = new PLCParameter<float>();
            InitTime  = new PLCParameter<float>();
            DistanceTime = new PLCParameter<float>();
            PressureTime = new PLCParameter<Int32>();
            PressureSwitch = new PLCParameter<Boolean>();
            Pressure = new PLCParameter<Int32>();
            PickTMotor = new PLCParameter<Int32>();
            StopCoil = new PLCParameter<Boolean>();
            StartAfter = new PLCParameter<Int32>();
        }
        /// <summary>
        /// 预测移动
        /// </summary>
        public Dictionary<int, ForecastMoveData> forecast_move = new Dictionary<int, ForecastMoveData>();
        /// <summary>
        /// 当前设置的速度
        /// </summary>
        public int device_speed { get; set; } = -1;
        /// <summary>
        /// 初始化速度
        /// </summary>
        public PLCParameter<Int32> InitSpeed { get; set; }
        /// <summary>
        /// 初始化移动距离
        /// </summary>
        public PLCParameter<float> InitDistance { get; set; }
        /// <summary>
        /// 程序零点
        /// </summary>
        public decimal ProgramZero { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public bool IsStarted { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public bool IsBack { get; set; }
        /// <summary>
        /// 当前移动到的位置距离
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public decimal CurrentDistance { get {
                return _CurrentDistance;
            } set {
                if (_CurrentDistance > value)
                {
                    IsBack = true;
                }else if (_CurrentDistance < value)
                {
                    IsBack = false;
                }
                _CurrentDistance = value;
            } }
        private decimal _CurrentDistance;
        public string GetErrorAdress()
        {
            var error_adress = InitCoil.Addr.Substring(0, 2) + "-B001";
            return error_adress;
        }
        public int GetMotoIndex()
        {
            var motor_index = int.Parse(Distance.Addr.Substring(3, 2));
            return motor_index;
        }
        public PLCParameter<bool> InitCoil { get; set; }
        /// <summary>
        /// 零点
        /// </summary>
        public PLCParameter<Boolean> Zero { get; set; }
        /// <summary>
        /// 位移量，距离
        /// </summary>
        public PLCParameter<float> Distance { get; set; }
        /// <summary>
        /// 实际位移量，距离
        /// </summary>
        public PLCParameter<float> RealDistance { get; set; }
        /// <summary>
        /// 速度
        /// </summary>
        public PLCParameter<Int32> Speed { get; set; }
        /// <summary>
        /// 校正因子
        /// </summary>
        public PLCParameter<float> Factor { get; set; }
        /// <summary>
        /// 完成线圈
        /// </summary>
        public PLCParameter<Boolean> DoneCoil { get; set; }
        /// <summary>
        /// 起动线圈
        /// </summary>
        public PLCParameter<Boolean> StartCoil { get; set; }
        /// <summary>
        /// 最大极限值
        /// </summary>
        public PLCParameter<float> Maximum { get; set; }
        /// <summary>
        /// 零点超时
        /// </summary>
        public PLCParameter<float> InitTime { get; set; }
        /// <summary>
        /// 移动超时
        /// </summary>
        public PLCParameter<float> DistanceTime { get; set; }
        /// <summary>
        /// 气压上传时间
        /// </summary>
        public PLCParameter<Int32> PressureTime { get; set; }
        /// <summary>
        /// 气压上传开关
        /// </summary>
        public PLCParameter<Boolean> PressureSwitch { get; set; }
        /// <summary>
        /// 气压
        /// </summary>
        public PLCParameter<Int32> Pressure { get; set; }
        /// <summary>
        /// T轴选位
        /// </summary>
        public PLCParameter<Int32> PickTMotor { get; set; }
        /// <summary>
        /// 运动停止
        /// </summary>
        public PLCParameter<Boolean> StopCoil { get; set; }
        /// <summary>
        /// 始停步长
        /// </summary>
        public PLCParameter<Int32> StartAfter { get; set; }
    }
}
