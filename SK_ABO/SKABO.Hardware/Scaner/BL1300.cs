using SKABO.Ihardware.Core;
using System;
using System.IO.Ports;

namespace SKABO.Hardware.Scaner
{
    public class BL1300 : AbstractScaner
    {
        protected override int baudRate { get { return 9600; } }
        protected override int dataBits { get { return 7; } }
        protected override StopBits stopBits { get { return StopBits.One; } }
        protected override Parity parity { get { return Parity.Even; } }
        protected override string NewLine
        {
            get { return "\r"; }
        }
        public override bool Start(bool IsWorkingModel)
        {
            if (!IsOpen) return false;

            var result=SendDataLine("UNLOCK",out String ReturnVal);//复位激光关闭
            result = result && SendDataLine("SSET",out ReturnVal);//切换到设置模式
            result = result && SendDataLine("WP121", out ReturnVal);//多标签读取模式 1 
            //SendDataLine("RP12", out ReturnVal);
            //SendDataLine("WP130",out ReturnVal);
            //SendDataLine("RP13",out ReturnVal);
            result = result && SendDataLine("SEND", out ReturnVal);//退出设置模式
            result = result && SendDataLine("MOTORON", out ReturnVal);//取消马达停止
            result = result && SendDataLine("LON", out ReturnVal, false);//触发输入接通
            return result;
        }

        public override bool Stop()
        {
            if (IsOpen)
            {
                CancelAllEvent();
                bool result = SendDataLine("LOFF",out String ReturnVal, false);//触发输入关闭
                //result= result && SendDataLine("LOCK",out ReturnVal, true);//强制激光关闭
                Close();
                return result;
            }
            return true;
        }
        /// <summary>
        /// 读取发送命令后，条码的回应数据，并丢弃，一般为OK ERROR
        /// OK+\r
        /// </summary>
        protected override bool ReadReturnInfo( out String ReturnVal)
        {
            ReturnVal = this.Read();
            return "OK"== ReturnVal;
        }
    }
}
