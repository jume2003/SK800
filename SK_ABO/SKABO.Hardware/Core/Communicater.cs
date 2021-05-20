using SKABO.Hardware.Enums;
using SKABO.Ihardware.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SKABO.Common.Utils;
using SKABO.Common;
using SKABO.Common.Models.Communication;

namespace SKABO.Hardware.Core
{

    public class Communicater : AbstractComm
    {
        private Communicater() { }
        /// <summary>
        /// PLC通信站号
        /// </summary>
        public override byte PLC_STATION_ID => 0x01;
        public override String Key => "PLC";
        protected override string DeviceName => "血型分析仪";

        public Communicater(String ipAddr, int port)
        {
            this.ipAddr = ipAddr;
            this.port = port;
            this.scanFlag = true;
        }
        public override byte[] ConvertFatekAddrToModbusAddr(String FatekAddr)
        {
            if (String.IsNullOrWhiteSpace(FatekAddr))
            {
                return null;
            }
            FatekAddr = FatekAddr.Trim();
            String type = FatekAddr.Substring(0, 1).ToUpper();
            Int16 baseAddr = 0;
            switch (type)
            {
                case "Y"://000001-000256  YO-Y255 
                    {
                        baseAddr = 0;
                        break;
                    }
                case "X"://001001-001256   X0-X255
                    {
                        baseAddr = 1000;
                        break;
                    }
                case "M"://002001-004002  M0-M2001
                    {
                        baseAddr = 2000;
                        break;
                    }
                case "S"://006001-007000 S0-S999
                    {
                        baseAddr = 6000;
                        break;
                    }
                case "T"://09001-09256 T0-T255
                    {
                        baseAddr = 9000;
                        break;
                    }
                case "C"://09501-0956 C0-C255
                    {
                        baseAddr = 9500;
                        break;
                    }
                case "R"://40001 - 44168 R0-R4167
                    {
                        baseAddr = 0;
                        break;
                    }
                case "D"://46001 -48999 D0-D2998
                    {
                        baseAddr = 6000;
                        break;
                    }
            }
            String dstr = FatekAddr.Substring(1);
            Int16 addr = (Int16)(baseAddr + Int16.Parse(dstr));
            var result = ByteUtil.Int2ToBytes(addr);
            if (result.Count() == 1)
            {

                return new byte[] { 0X00, result[0] };
            }
            return result;

        }
    }
}
