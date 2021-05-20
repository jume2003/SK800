using SKABO.Common;
using SKABO.Hardware.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SKABO.Hardware.Core
{
    public class PcbComm : AbstractComm
    {
        public override byte PLC_STATION_ID => 0X01;
        public override String Key => "PCB";

        protected override string DeviceName => "辅助设备";
        public PcbComm(String ipAddr, int port)
        {
            this.ipAddr = ipAddr;
            this.port = port;
            this.scanFlag = true;
        }

        public override byte[] ConvertFatekAddrToModbusAddr(String FatekAddr)
        {
            return new Byte[] { 0x00, Byte.Parse(FatekAddr) };
        }
        public override void ScanRunResult()
        {
            //int i = 0;
            while (scanFlag)
            {
                this.ScanRestEvent.WaitOne();
                bool[] result = null;
                if (Key == "PCB")
                    result = this.ReadCoil(16, 1, "0",true);
                if (RunResult != null && result!=null)
                {
                    IList<byte> idList = new List<byte>();
                    for (byte i=0;i< 16;i++)
                    {
                        if (RunResult[i] ^ result[i])
                        {
                            idList.Add(i);
                        }
                    }
                    if (idList.Count > 0)
                    {
                        OnChangeSampleRackStatus?.Invoke(idList.ToArray(), (byte)(result[idList[0]] ? 1 : 0));
                    }
                }
                RunResult = result;
                Thread.Sleep(Constants.Timespan);//间隔时间
            }
        }
        /// <summary>
        /// 样本载架脱离或复位委托
        /// </summary>
        /// <param name="indexs"></param>
        /// <param name="eventType">0:脱离事件，1：复位事件</param>
        public delegate void SampleRackExistedEventHandler(byte[] indexs, byte eventType);
        /// <summary>
        /// 样本载架脱离或复位事件
        /// </summary>
        public event SampleRackExistedEventHandler OnChangeSampleRackStatus;
        /// <summary>
        /// 光藕感应，检测Gel卡是否存在
        /// </summary>
        /// <param name="coilCount"></param>
        /// <param name="regCount"></param>
        /// <param name="addr"></param>
        /// <returns></returns>
        public bool[] TestGelCard(short coilCount,short regCount,String addr)
        {
            var data = ReadAddr(ModbusFunCode.READ_REGISTER, regCount, ConvertFatekAddrToModbusAddr(addr));
            if (data != null)
            {
                var target = new byte[] { 0x03, (byte)~data[2], (byte)~data[1], (byte)~data[4] };
                return RecDataToBools(coilCount, target);
            }
            return null;
        }
    }
}
