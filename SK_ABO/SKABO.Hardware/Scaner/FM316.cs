using SKABO.Ihardware.Core;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SKABO.Hardware.Scaner
{
    public class FM316 : AbstractScaner
    {
        protected override int baudRate { get { return 9600; } }
        protected override int dataBits { get { return 8; } }
        protected override StopBits stopBits { get { return StopBits.One; } }
        protected override Parity parity { get { return Parity.None; } }
        protected override string NewLine
        {
            get { return "\n"; }
        }
        public FM316() : base()
        {

        }

        public override bool Start(bool IsWorkingModel)
        {
            if (!IsOpen) return false;
            var data_open = new byte[] {0x02,0x01,0x01,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x03,0xF8};
            var data_add_tab = new byte[] {0x02,0x02,0x07,0x82,0x30,0x32,0x31,0x32,0x40,0x0A,0x00,0x00,0x00,0x00,0x03,0x61};
            var data_close_sound = new byte[] {0x02,0x02,0x07,0x82,0x30,0x31,0x34,0x32,0x30,0x30,0x00,0x00,0x00,0x00,0x03,0x49};
            
            var data_version = new byte[] {0x02,0x02,0x06,0x82,0x30,0x30,0x30,0x41,0x30,0x00,0x00,0x00,0x00,0x00,0x03,0x70};
            String ReturnVal;
            SendData(data_open, out ReturnVal, false);
            Thread.Sleep(200);
            SendData(data_add_tab, out ReturnVal, false);
            Thread.Sleep(200);
            SendData(data_close_sound, out ReturnVal, false);
            Thread.Sleep(200);
            SendData(data_version, out ReturnVal, true);
            Thread.Sleep(200);
            var result = ReturnVal.IndexOf("Decoder R2 V7.11")!=-1;
            return result;
        }

        public override bool Stop()
        {
            if (IsOpen)
            {
                var data = new byte[] {0x02,0x01,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x03,0xF9};
                CancelAllEvent();
                String ReturnVal;
                SendData(data, out ReturnVal, false);
                //foreach (var d in data)
                //{
                //    var s = new byte[] { d };
                //    SendData(s, out ReturnVal, false);
                //}
                var restult = true;// ReadReturnInfo(out ReturnVal);
                Close();
                return restult;
            }
            return true;
        }

        protected override bool ReadReturnInfo(out string ReturnVal)
        {
            ReturnVal = null;
            int maxbyte = 1024;
            bool result = true;
            byte[] buffer = new byte[maxbyte];
            Thread.Sleep(200);
            int count = this.serialPort.Read(buffer, 0, buffer.Length);
            byte[] strbuffer = new byte[count];
            Buffer.BlockCopy(buffer, 0, strbuffer, 0, count);
            ReturnVal = System.Text.Encoding.UTF8.GetString(strbuffer);
            this.serialPort.DiscardInBuffer();
            return result;
        }
        
    }
}
