using SKABO.Common.Models.BJ;
using SKABO.Common.Utils;
using System;
using System.IO.Ports;
using System.Text;

namespace SKABO.Ihardware.Core
{
    public abstract class AbstractScaner
    { 
        public object Tag { get; set; }
        protected abstract int baudRate { get; }
        protected abstract int dataBits { get; }
        protected abstract StopBits stopBits { get; }
        protected abstract Parity parity { get; }
        protected abstract String NewLine { get; }
        public AbstractScaner()
        {
            serialPort = new SerialPort();
        }
        protected SerialPort serialPort;
        public delegate void DataReceivedEventHandler(AbstractScaner sender);
        public event DataReceivedEventHandler DataReceived;
        public void CancelAllEvent()
        {
            this.serialPort.DataReceived -= SerialPort_DataReceived;
            this.DataReceived =null;
        }
        public void DiscardInBuffer()
        {
            if(IsOpen)
                this.serialPort.DiscardInBuffer();
        }
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="portName"></param>
        /// <param name="baudRate"></param>
        /// <returns></returns>
        public bool Open(String portName)
        {
            if (serialPort.IsOpen) return true;
                //串口名
                serialPort.PortName = portName;
                //波特率
                serialPort.BaudRate = baudRate;
                //数据位
                serialPort.DataBits = dataBits;//8
                //1个停止位
                serialPort.StopBits = stopBits;
                //无奇偶校验位
                serialPort.Parity = parity;//None
            serialPort.ReadTimeout = 30000;

                this.serialPort.NewLine = NewLine;

                if (serialPort.IsOpen)
                {
                    Close();
                }
            try
            {
                serialPort.Open();
                if (serialPort.IsOpen)
                {
                    serialPort.DataReceived += SerialPort_DataReceived;
                    return true;
                }
                else
                {
                    return false;
                }
            }catch(Exception ex)
            {
                Tool.AppLogError(ex);
                return false;
            }
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataReceived?.Invoke(this);
        }
        /// <summary>
        /// 触发条码器
        /// </summary>
        public abstract bool Start(bool IsWorkingModel);
        /// <summary>
        /// 关闭条码器，同时关闭串口
        /// </summary>
        /// <returns></returns>
        public abstract bool Stop();
        /// <summary>
        /// 读取串口数据
        /// </summary>
        /// <returns></returns>
        public virtual String Read()
        {
            try
            {
                if (this.IsOpen)
                {
                    //while (true) { 
                    var result = this.serialPort.ReadLine();
                    this.serialPort.DiscardInBuffer();
                    return result;
                }
                else
                {
                    Console.WriteLine(this.serialPort + " 已关闭");
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// 发送字符串 
        /// </summary>
        /// <param name="str"></param>
        public bool SendData(String str, out String ReturnVal, bool hasResponse=true)
        {
            ReturnVal = null;
            if (String.IsNullOrEmpty(str))
            {
                return true;
            }
            var bytes = Encoding.Default.GetBytes(str);
            WritePort(bytes, 0, bytes.Length);
            if (hasResponse)
                return ReadReturnInfo(out ReturnVal);
            else
            {
                return true;
            }
        }
        public bool SendDataLine(String str, out String ReturnVal, bool hasResponse=true)
        {
            ReturnVal = null;
            if (String.IsNullOrEmpty(str))
            {
                return true;
            }
            this.serialPort.WriteLine(str);
            this.serialPort.DiscardOutBuffer();
            if (hasResponse)
                return ReadReturnInfo(out ReturnVal);
            else
                return true;
        }
        /// <summary>
        /// 发送字节码
        /// </summary>
        /// <param name="data"></param>
        public bool SendData(Byte[] data, out String ReturnVal, bool hasResponse = true)
        {
            ReturnVal = null;
            WritePort(data, 0, data.Length);
            if (hasResponse)
                return ReadReturnInfo(out ReturnVal);
            else
                return true;
        }
        /// <summary>
        /// 关闭串口
        /// </summary>
        public virtual void Close()
        {
            if(this.serialPort!=null && IsOpen)
            {
                serialPort.Close();
            }
        }
        private void WritePort(byte[] send, int offSet, int count)
        {
            if (IsOpen)
            {
                serialPort.Write(send, offSet, count);
                //System.Diagnostics.Debug.Assert(this.serialPort.IsOpen);
                if (this.serialPort.IsOpen)
                this.serialPort.DiscardOutBuffer();
              
            }
        }
        //串口状态
        public bool IsOpen
        {
            get
            {
                return serialPort.IsOpen;
            }
        }
        /// <summary>
        /// 是否成功执行
        /// </summary>
        /// <param name="ReturnVal">返回信息</param>
        /// <returns></returns>
        protected abstract bool ReadReturnInfo(out String ReturnVal);
        //获取可用串口
        public static string[] GetComName()
        {
            string[] names = null;
            try
            {
                names = SerialPort.GetPortNames(); // 获取所有可用串口的名字
            }
            catch (Exception)
            {
                //MessageBox.Show("找不到串口");
            }
            return names;
        }
    }
}
