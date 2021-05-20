using SKABO.Ihardware.Core;
using System;
using System.IO.Ports;
using System.Threading;

namespace SKABO.Hardware.Scaner
{
    public class FX8090 : AbstractScaner
    {
        protected override int baudRate { get { return 115200; } }
        protected override int dataBits { get { return 8; } }
        protected override StopBits stopBits { get { return StopBits.One; } }
        protected override Parity parity { get { return Parity.None; } }
        protected override string NewLine
        {
            get { return "\r"; }
        }

        public FX8090():base()
        {

        }
        private byte[] GetDatas(byte parameter)
        {// 7e 80 08 04 00
            byte[] data = new byte[] { 0x7E, 0x80, 0x08, 0x04, 0x00, 0x00, 0x01, parameter, CalcLRC(parameter), 0x7E };
            return data;
        }
        private byte CalcLRC(byte parameter)
        {
            var datas = new byte[] { 0x80, 0x08, 0x04, 0x00, 0x00, 0x01, parameter };

            byte LRC = 0x00;
            for (int i = 0; i < datas.Length; i++)
            {
                LRC ^= datas[i];
            }
            return LRC;
        }

        public override bool Start(bool IsWorkingModel)
        {
            if (!IsOpen) return false;
            var data = (GetDatas(0x01));
            String ReturnVal;
            foreach (var d in data)
            {
                var s = new byte[] { d };
                SendData(s,out ReturnVal,false);
            }
            var result= ReadReturnInfo(out ReturnVal);
            return result;
        }

        public override bool Stop()
        {
            if (IsOpen)
            {
                var data = (GetDatas(0x00));
                CancelAllEvent();
                String ReturnVal;
                foreach (var d in data)
                {
                    var s = new byte[] { d };
                    SendData(s,out ReturnVal,false);
                }
                var restult=ReadReturnInfo(out ReturnVal);
                Close();
                return restult;
            }
            return true;
        }
        /// <summary>
        /// 读取发送命令后，条码的回应数据，并丢弃，7E 开始，并以7E结束，一般为9个
        /// 7E 0F 00 00 00 00 00 0F 7E
        /// </summary>
        /// <param name="ReturnVal">不关心返回值</param>
        /// <returns></returns>
        protected override bool ReadReturnInfo(out String ReturnVal)
        {
            ReturnVal = null;
            bool result = true;
            byte[] Success = new byte[] { 0x7E ,0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x7E };
            Thread.Sleep(200);//等200ms很重要，否则40%的机率出现读取不到返回信息，会导致一次扫码时数据我一些前导信息
            byte[] buffer = new byte[9];
            if (this.serialPort.IsOpen == false) return false;
            int count = this.serialPort.Read(buffer,0, buffer.Length);
            if (count != Success.Length)
                result = false;
            else
            {
                for (int j = 0; j < Success.Length; j++)
                {
                    result = result && Success[j] == buffer[j];
                }
            }
            /*
            for (int i = 0; i < 9; i++)
            {
                Console.WriteLine("i=" + i +" stats is "+this.IsOpen);
                try
                {
                    var db = this.serialPort.ReadByte();
                    Console.WriteLine("db=" + db.ToString("X2").ToUpper());
                    result = result && db == Success[i];

                    if (i != 0 && db == 0x7E)
                        break;
                }catch(TimeoutException ex)
                {
                    result = false;
                }
                if (!result)
                    break;
            }*/
            this.serialPort.DiscardInBuffer();
            return result;
        }
    }
}
