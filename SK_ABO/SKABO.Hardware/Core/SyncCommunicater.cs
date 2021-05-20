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
using System.Threading.Tasks;

namespace SKABO.Hardware.Core
{

    public class SyncCommunicater
    {
        private SyncCommunicater() { }
        /// <summary>
        /// PLC通信站号
        /// </summary>
        public static byte PLC_STATION_ID = 0X01;
        private object monitor = new object();
        public SyncCommunicater(String ipAddr, int port)
        {
            this.ipAddr = ipAddr;
            this.port = port;
        }
        private String _ErrorMessage;
        public String ErrorMessage
        {
            get
            {
                return _ErrorMessage;
            }
            set
            {
                _ErrorMessage = value;
                if (OnErrorHandler != null)
                {
                    OnErrorHandler.Invoke(value);
                }
            }
        }
        private String ipAddr;
        private int port;
        private Socket socket;
        private bool Connected;
        public void SetCoilOn(String fatekAddr)
        {
            byte[] addr = ConvertFatekAddrToModbusAddr(fatekAddr);
            SetCoilOn(addr[0], addr[1]);
        }
        /// <summary>
        /// addrs[0]为地址高位 addrs[1]为地址低位
        /// </summary>
        /// <param name="addr"></param>
        public void SetCoilOn(params byte[] addr)
        {
            SetCoil(true, addr);
        }
        public void SetCoilOff(String fatekAddr)
        {
            byte[] addr = ConvertFatekAddrToModbusAddr(fatekAddr);
            SetCoilOff(addr);
        }
        public void SetCoil(bool on, params byte[] addr)
        {
            SetSingleVal(ModbusFunCode.WRITE_SINGLE_COIL, addr, (byte)(on ? 0xFF : 0x00), 0x00);
        }
        private void SetSingleVal(byte funCode, byte[] addr, params byte[] vals)
        {
            send(new byte[] { PLC_STATION_ID, funCode, addr[0], addr[1], vals[0], vals[1] });
        }
        /// <summary>
        /// addrs[0]为地址高位 addrs[1]为地址低位
        /// </summary>
        /// <param name="addr"></param>
        public void SetCoilOff(params byte[] addr)
        {
            SetCoil(false, addr);
        }
        public void SetBatchCoil(String startFatekAddr, short len, params byte[] bitValsk)
        {
            SetBatchCoil(ConvertFatekAddrToModbusAddr(startFatekAddr), len, bitValsk);
        }
        public void SetBatchCoil(byte[] startAddr, short len, params byte[] bitValsk)
        {
            SetBatchVal(ModbusFunCode.WRITE_MULTI_COIL, startAddr, len, bitValsk);
        }
        public void ReadCoil(short coilCount, String startFatekAddr)
        {
            ReadCoil(coilCount, ConvertFatekAddrToModbusAddr(startFatekAddr));
        }
        public void ReadCoil(short coilCount, params byte[] startAddr)
        {
            ReadAddr(ModbusFunCode.READ_COIL, coilCount, startAddr);
        }
        public void ReadDiscrete(short coilCount, String startFatekAddr)
        {
            ReadDiscrete(coilCount, ConvertFatekAddrToModbusAddr(startFatekAddr));
        }
        public void ReadDiscrete(short coilCount, params byte[] startAddr)
        {
            ReadAddr(ModbusFunCode.READ_DISCRETE, coilCount, startAddr);
        }
        public void ReadRegister(short count, String startAddr)
        {
            ReadRegister(count, ConvertFatekAddrToModbusAddr(startAddr));
        }
        public void ReadRegister(short count, params byte[] startAddr)
        {
            ReadAddr(ModbusFunCode.READ_REGISTER, count, startAddr);
        }
        public void SetRegister(String addr, short val)
        {
            SetRegister(ConvertFatekAddrToModbusAddr(addr), val);
        }

        public void SetRegister(byte[] addr, short val)
        {
            SetSingleVal(ModbusFunCode.WRITE_SINGLE_REGISTER, addr, int2ToBytes(val));
        }
        private Regex _DoubleAddrReg;
        private Regex DoubleAddrReg
        {
            get
            {
                if (_DoubleAddrReg == null)
                {
                    _DoubleAddrReg = new Regex(@"^D\w{1}\d+$");
                }
                return _DoubleAddrReg;
            }
        }

        private bool IsDoubleAddr(String addr)
        {
            return DoubleAddrReg.IsMatch(addr);
        }
        public void SetRegister(String addr, int val)
        {
            if (IsDoubleAddr(addr))
                SetRegister(ConvertFatekAddrToModbusAddr(addr.Substring(1)), val);
            else
            {
                ErrorMessage = "地址格式不正确";
            }

        }

        public void SetRegister(byte[] addr, int val)
        {

            SetBatchRegister(addr, 2, int4ToBytes(val));
        }
        public void SetRegister(String addr, float val)
        {
            if (IsDoubleAddr(addr))
                SetRegister(ConvertFatekAddrToModbusAddr(addr.Substring(1)), val);
            else
            {
                ErrorMessage = "地址格式不正确";
            }
        }

        public void SetRegister(byte[] addr, float val)
        {
            SetBatchRegister(addr, 2, singleToBytes(val));
        }
        /*
        public void SetBatchRegister(String startAddr, short len, params byte[] vals)
        {
            SetBatchRegister(ConvertFatekAddrToModbusAddr(startAddr), len, vals);
        }*/
        public void SetBatchRegister(String startAddr, params short[] vals)
        {
            List<Byte> blist = new List<byte>();
            for (short i = 0; i < vals.Length; i++)
            {
                blist.AddRange(int2ToBytes(vals[i]));
            }
            Console.WriteLine(blist.Count);
            short len = (short)vals.Length;
            SetBatchRegister(ConvertFatekAddrToModbusAddr(startAddr), len, blist.ToArray());
        }
        public void SetBatchRegister(byte[] startAddr, short len, params byte[] vals)
        {
            SetBatchVal(ModbusFunCode.WRITE_MULTI_REGISTER, startAddr, len, vals);
        }
        public void ReadAndSetRegister(String readStartAddr, short readLen, String writeStartAddr, short writeLen, params byte[] vals)
        {
            ReadAndSetRegister(ConvertFatekAddrToModbusAddr(readStartAddr), readLen, ConvertFatekAddrToModbusAddr(writeStartAddr), writeLen, vals);
        }
        public void ReadAndSetRegister(byte[] readStartAddr, short readLen, byte[] writeStartAddr, short writeLen, params byte[] vals)
        {
            var reaLlenb = int2ToBytes(readLen);
            var writeLenb = int2ToBytes(writeLen);
            var byteCount = (byte)(vals.Length);
            var b1 = new byte[] { PLC_STATION_ID, ModbusFunCode.READ_WRITE_REGISTER, readStartAddr[0], readStartAddr[1], reaLlenb[0], reaLlenb[1],
                writeStartAddr[0],writeStartAddr[1],writeLenb[0],writeLenb[1],  byteCount };
            var b2 = new byte[b1.Length + byteCount];
            b1.CopyTo(b2, 0);
            Array.Copy(vals, 0, b2, b1.Length, byteCount);
            send(b2);
        }
        private void SetBatchVal(byte funCode, byte[] startAddr, short len, params byte[] vals)
        {
            var lenb = int2ToBytes(len);
            var byteCount = (byte)(vals.Length);
            var b1 = new byte[] { PLC_STATION_ID, funCode, startAddr[0], startAddr[1], lenb[0], lenb[1], byteCount };
            var b2 = new byte[b1.Length + byteCount];
            b1.CopyTo(b2, 0);
            Array.Copy(vals, 0, b2, b1.Length, byteCount);
            send(b2);
        }
        private void ReadAddr(byte fucCode, short count, params byte[] startAddr)
        {
            var countBts = int2ToBytes(count);
            var data = new byte[] { PLC_STATION_ID, fucCode, startAddr[0], startAddr[1], countBts[0], countBts[1] };
            send(data);
        }
        /// <summary>
        /// data 不含前6个字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Message ID</returns>
        public short? send(byte[] data)
        {
            lock (monitor)
            {
                if (!Connected)
                {
                    Connect();
                }
                if (Connected)
                {

                    short msgID = 1;
                    var idByte = int2ToBytes(msgID);
                    byte[] other = new byte[] { 0x0, 0x0 };
                    var lensByte = int2ToBytes((short)data.Length);
                    byte[] newData = new byte[data.Length + 6];
                    Array.Copy(idByte, 0, newData, 0, 2);
                    Array.Copy(other, 0, newData, 2, 2);
                    Array.Copy(lensByte, 0, newData, 4, 2);
                    Array.Copy(data, 0, newData, 6, data.Length);
                    Console.WriteLine("输入：");
                    Console.WriteLine(BitConverter.ToString(newData));
                    socket.Send(newData);
                    Monitor.Pulse(monitor);
                    return msgID;
                }
                else
                {
                    ErrorMessage = "未能成功连接到血型分析仪";
                    return null;
                }
            }
        }

        public delegate void ErrorDelegate(String errorMsg);
        public ErrorDelegate OnErrorHandler;

        public static byte[] ConvertFatekAddrToModbusAddr(String FatekAddrd)
        {
            String type = FatekAddrd.Substring(0, 1).ToUpper();
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
            String dstr = FatekAddrd.Substring(1);
            Int16 addr = (Int16)(baseAddr + Int16.Parse(dstr));
            var result = int2ToBytes(addr);
            if (result.Count() == 1)
            {

                return new byte[] { 0X00, result[0] };
            }
            return result;

        }
        public static byte[] singleToBytes(float value)
        {
            byte[] bs = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                byte temp = bs[0];
                bs[0] = bs[1];
                bs[1] = temp;
                temp = bs[2];
                bs[2] = bs[3];
                bs[3] = temp;
            }
            return bs;
        }
        public static float BytesToSingle(params byte[] data)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToSingle(new byte[] { data[1], data[0], data[3], data[2] }, 0);
            }
            else
            {
                return BitConverter.ToSingle(data, 0);
            }
        }
        /// <summary>
        /// 高位在前，低位在后
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] int2ToBytes(Int16 value)
        {
            byte[] src = new byte[2];
            src[0] = (byte)((value >> 8) & 0xFF);
            src[1] = (byte)(value & 0xFF);
            return src;
        }
        public static short BytesToInt16(params byte[] data)
        {
            return (short)(data[0] << 8 | data[1]);
        }
        public static int BytesToInt(params byte[] data)
        {
            return (short)(data[2] << 24 | data[3] << 16 | data[0] << 8 | data[1]);
        }
        public static byte[] int4ToBytes(Int32 value)
        {
            byte[] src = new byte[4];
            src[2] = (byte)((value >> 24) & 0xFF);
            src[3] = (byte)((value >> 16) & 0xFF);
            src[0] = (byte)((value >> 8) & 0xFF);
            src[1] = (byte)(value & 0xFF);
            return src;
        }
        public void Connect()
        {

            IPEndPoint ie = new IPEndPoint(IPAddress.Parse(ipAddr), port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.ReceiveTimeout = 3000;

            //将套接字与远程服务器地址相连  
            try
            {
                socket.Connect(ie);
                Connected = true;

            }
            catch (SocketException e)
            {
                Console.Out.WriteLine("连接服务器失败  " + e.Message);
                return;
            }

            ThreadStart myThreaddelegate = new ThreadStart(ReceiveMsg);
            var myThread = new Thread(myThreaddelegate);
            myThread.Start();

        }

        public void ReceiveMsg()
        {//00-01-00-00-00-06-01-05-07-D0-00-00
            lock (monitor)
            {
                //Monitor.Pulse(monitor);
                while (true)
                {

                    byte[] data = new byte[1024];//定义数据接收数组  
                    socket.Receive(data);//接收数据到data数组  
                    int length = BytesToInt16(data[4], data[5]);//读取数据长度  
                    Console.WriteLine(String.Format("长度={0}", length));
                    Byte[] realData = new byte[length + 6];//定义所要显示的接收的数据的长度  
                    for (int i = 0; i <= length + 5; i++)//将要显示的数据存放到数组datashow中  
                        realData[i] = data[i];
                    string stringdata = BitConverter.ToString(realData);
                    Console.WriteLine(stringdata);

                    if (data[7] == 0x01)
                    {

                    };
                    Monitor.Pulse(monitor);
                    Monitor.Wait(monitor);

                }
            }
        }
    }
}
