using SKABO.Common.Models.Communication.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Communication
{
    /// <summary>
    /// 系统其它部分参数
    /// </summary>
    public class OtherPart
    {
        public OtherPart()
        {
            IpAddress = "10.139.1.2";
            Port = 502;
            ScanMotor = new Electromotor();
            SampleRackCoils = new PLCParameter<bool>[Constants.SampleRackCount];
            for (byte i = 0; i < Constants.SampleRackCount; i++)
            {
                SampleRackCoils[i] = new PLCParameter<bool>();
            }

            DoorCoil = new PLCParameter<bool>();
            this.CameraLightCoil = new PLCParameter<bool>();
            this.CameraFLightCoil = new PLCParameter<bool>();
            this.HandStopCoil = new PLCParameter<bool>();
            this.EmergencyStopCoil = new PLCParameter<bool>();
            this.ErrorCode = new PLCParameter<short>();
            this.LightAlarmCoil = new PLCParameter<bool>();
            this.LightCoil = new PLCParameter<bool>();
            this.PauseCoil = new PLCParameter<bool>();
            this.PLCLightAlarmCoil = new PLCParameter<bool>();
            this.PLCVoiceAlarmCoil = new PLCParameter<bool>();
            this.StartSwitchCoil = new PLCParameter<bool>();
            this.StopCode = new PLCParameter<short>();
            this.TestSelfSwitchCoil = new PLCParameter<bool>();
            this.VoiceAlarmCoil = new PLCParameter<bool>();
            this.AvoidanceTotal = new PLCParameter<int>();
            this.AvoidanceSpace = new PLCParameter<int>();
        }
        //public OtherPart(bool Init)
        //{


        //}
        /// <summary>
        /// PLC地址
        /// </summary>
        public String IpAddress { get; set; }
        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 辅IP,PCB 板IP
        /// </summary>
        public String SencondIpAddress { get; set; }
        /// <summary>
        /// 辅port
        /// </summary>
        public int SecondPort { get; set; }
        /// <summary>
        /// 样本扫码器载架电机
        /// </summary>
        public Electromotor ScanMotor { get; set; }

        /// <summary>
        /// 样本载架是否在位
        /// </summary>
        public PLCParameter<bool>[] SampleRackCoils { get; set; }
        
       
        public PLCParameter<bool> DoorCoil { get; set; }
        /// <summary>
        /// 声音报警
        /// </summary>
        public PLCParameter<bool> VoiceAlarmCoil { get; set; }
        /// <summary>
        /// 灯光报警
        /// </summary>
        public PLCParameter<bool> LightAlarmCoil { get; set; }
        /// <summary>
        /// PLC触发声音报警
        /// </summary>
        public PLCParameter<bool> PLCVoiceAlarmCoil { get; set; }
        /// <summary>
        /// PLC触发灯光报警
        /// </summary>
        public PLCParameter<bool> PLCLightAlarmCoil { get; set; }
        /// <summary>
        /// 照明灯光
        /// </summary>
        public PLCParameter<bool> LightCoil { get; set; }
        /// <summary>
        /// 相机灯光
        /// </summary>
        public PLCParameter<bool> CameraLightCoil { get; set; }
        /// <summary>
        /// 相机前灯光
        /// </summary>
        public PLCParameter<bool> CameraFLightCoil { get; set; }
        /// <summary>
        /// 暂停线圈
        /// </summary>
        public PLCParameter<bool> PauseCoil { get; set; }
        /// <summary>
        /// 紧急停止线圈
        /// </summary>
        public PLCParameter<bool> EmergencyStopCoil { get; set; }
        /// <summary>
        /// 系统停止原因代码
        /// </summary>
        public PLCParameter<short> StopCode { get; set; }
        /// <summary>
        /// 开关机线圈
        /// </summary>
        public PLCParameter<bool> StartSwitchCoil { get; set; }
        /// <summary>
        /// 自检开关
        /// </summary>
        public PLCParameter<bool> TestSelfSwitchCoil { get; set; }
        /// <summary>
        /// 系统错误代码
        /// </summary>
        public PLCParameter<short> ErrorCode { get; set; }
        /// <summary>
        /// 抓手刹车位
        /// </summary>
        public PLCParameter<bool> HandStopCoil { get; set; }
        /// <summary>
        /// 防撞总行程
        /// </summary>
        public PLCParameter<int> AvoidanceTotal { get; set; }
        /// <summary>
        /// 防撞间隔
        /// </summary>
        public PLCParameter<int> AvoidanceSpace { get; set; }

    }
}
