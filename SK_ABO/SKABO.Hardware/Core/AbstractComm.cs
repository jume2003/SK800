using SKABO.Common;
using SKABO.Common.Models.Communication;
using SKABO.Common.Utils;
using SKABO.Hardware.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace SKABO.Hardware.Core
{
    public abstract class AbstractComm
    {
        protected abstract String DeviceName{get;}
        private object monitor = new object();
        private object msgIdLock = new object();
        private object resultLock = new object();

        public System.Threading.ManualResetEvent ScanRestEvent = new ManualResetEvent(true);

        public abstract String Key { get; }

        public abstract byte PLC_STATION_ID { get; }

        protected String ipAddr;
        protected int port;
        protected Socket socket;
        protected bool Connected;
        private UInt16 _MsgId;
        protected UInt16 GenerateMsgId()
        {
            lock (msgIdLock)
            {
                UInt16 mid = 0;
                mid = _MsgId++;
                if (_MsgId == UInt16.MaxValue)
                {
                    _MsgId = 0;
                }
                return mid;
            }
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
                OnError?.Invoke(Connected, value);
            }
        }

        public Boolean SetCoilOn(String fatekAddr)
        {
            byte[] addr = ConvertFatekAddrToModbusAddr(fatekAddr);
            return SetCoilOn(addr[0], addr[1]);
        }
        /// <summary>
        /// addrs[0]为地址高位 addrs[1]为地址低位
        /// </summary>
        /// <param name="addr"></param>
        public Boolean SetCoilOn(params byte[] addr)
        {
            return SetCoil(true, addr);
        }
        public Boolean SetCoilOff(String fatekAddr)
        {
            byte[] addr = ConvertFatekAddrToModbusAddr(fatekAddr);
            return SetCoilOff(addr);
        }
        public Boolean SetCoil(bool on, String fatekAddr)
        {
            return SetCoil(on, ConvertFatekAddrToModbusAddr(fatekAddr));
        }

        public Boolean SetCoil(bool on, params byte[] addr)
        {
            return SetSingleVal(ModbusFunCode.WRITE_SINGLE_COIL, addr, (byte)(on ? 0xFF : 0x00), 0x00);
        }
        private void SetAddrException(byte funCode)
        {
            this.ErrorMessage = $"通讯地址错误，请检查！功能码{funCode.ToString("X2")}";
        }
        private Boolean SetSingleVal(byte funCode, byte[] addr, params byte[] vals)
        {
            if (addr == null)
            {
                SetAddrException(funCode);
                return false;
            }
            byte[] recData;
            var msgID = send(new byte[] { PLC_STATION_ID, funCode, addr[0], addr[1], vals[0], vals[1] }, out recData);

            return recData != null;
        }
        /// <summary>
        /// addrs[0]为地址高位 addrs[1]为地址低位
        /// </summary>
        /// <param name="addr"></param>
        public Boolean SetCoilOff(params byte[] addr)
        {
            return SetCoil(false, addr);
        }
        /// <summary>
        /// "T0 10 0b00000011 0b01" 则设置T0=On,T1=On, T8=On
        /// </summary>
        /// <param name="startFatekAddr"></param>
        /// <param name="len"></param>
        /// <param name="bitValsk"></param>
        /// <returns></returns>
        public Boolean SetBatchCoil(String startFatekAddr, short len, params byte[] bitValsk)
        {
            return SetBatchCoil(ConvertFatekAddrToModbusAddr(startFatekAddr), len, bitValsk);
        }
        public Boolean SetBatchCoil(byte[] startAddr, short len, params byte[] bitValsk)
        {
            return SetBatchVal(ModbusFunCode.WRITE_MULTI_COIL, startAddr, len, bitValsk);
        }
        public Boolean? ReadCoil(String startFatekAddr)
        {
            var res = ReadCoil(1, ConvertFatekAddrToModbusAddr(startFatekAddr));
            if (res == null)
            {
                return null;
            }
            else
            {
                return res[0];
            }
        }
        public Boolean[] ReadCoil(short coilCount, String startFatekAddr)
        {
            return ReadCoil(coilCount, ConvertFatekAddrToModbusAddr(startFatekAddr));
        }
        public Boolean[] ReadCoil(short coilCount, params byte[] startAddr)
        {
            var data = ReadAddr(ModbusFunCode.READ_COIL, coilCount, startAddr);
            //return new bool[2];
            return RecDataToBools(coilCount, data);
        }
        /// <summary>
        /// 读取寄存器中数据，将其转换为bool变量的数组
        /// </summary>
        /// <param name="coilCount">线圈个数，不能大于 regCount*16</param>
        /// <param name="regCount">寄存器个数</param>
        /// <param name="startFatekAddr">寄存器起始地址</param>
        /// <returns></returns>
        public Boolean[] ReadCoil(short coilCount, short regCount, String startFatekAddr,bool NeedNot=false)
        {
            var data = ReadAddr(ModbusFunCode.READ_REGISTER, regCount, ConvertFatekAddrToModbusAddr(startFatekAddr));
            //return new bool[2];
            return RecDataToBools(coilCount, data, NeedNot);
        }
        protected Boolean[] RecDataToBools(short coilCount, byte[] data, bool NeedNot = false)
        {
            if (data == null) return null;
            if (NeedNot)
            {
                for(byte j = 1; j < data.Length; j++)
                {
                    data[j] =(byte) ~data[j];
                }
            }
            Boolean[] result = new Boolean[coilCount];
            for (short i = 0; i < coilCount; i++)
            {
                var byteIndex = i / 8 + 1;
                var bitIndex = i % 8;
                result[i] = (data[byteIndex] >> bitIndex & 0x01) == 0x01;
            }
            return result;
        }
        public Boolean[] ReadDiscrete(short coilCount, String startFatekAddr)
        {
            return ReadDiscrete(coilCount, ConvertFatekAddrToModbusAddr(startFatekAddr));
        }
        public Boolean[] ReadDiscrete(short coilCount, params byte[] startAddr)
        {
            var recData = ReadAddr(ModbusFunCode.READ_DISCRETE, coilCount, startAddr);
            return RecDataToBools(coilCount, recData);
        }
        public short[] ReadRegister(short count, String startAddr)
        {
            return ReadRegister(count, ConvertFatekAddrToModbusAddr(startAddr));
        }
        public short[] ReadRegister(short count, params byte[] startAddr)
        {
            var recData = ReadAddr(ModbusFunCode.READ_REGISTER, count, startAddr);
            var result = new short[count];
            for (short i = 0; i < count; i++)
            {
                try
                {
                    result[i] = ByteUtil.BytesToInt16(recData[2 * i + 1], recData[2 * i + 2]);
                }catch(Exception ex)
                {

                }
            }
            return result;
        }
        public Boolean SetRegister(String addr, short val)
        {
            return SetRegister(ConvertFatekAddrToModbusAddr(addr), val);
        }

        public Boolean SetRegister(byte[] addr, short val)
        {
            return SetSingleVal(ModbusFunCode.WRITE_SINGLE_REGISTER, addr, ByteUtil.Int2ToBytes(val));
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
            if (addr == null) return false;
            return DoubleAddrReg.IsMatch(addr);
        }
        public Boolean SetRegister(String addr, int val)
        {
            if (IsDoubleAddr(addr))
                return SetRegister(ConvertFatekAddrToModbusAddr(addr.Substring(1)), val);
            else
            {
                ErrorMessage = "地址格式不正确";
                return false;
            }

        }

        public Boolean SetRegister(byte[] addr, int val)
        {

            return SetBatchRegister(addr, 2, ByteUtil.Int4ToBytes(val));
        }
        public Boolean SetRegister(String addr, float val)
        {
            if (IsDoubleAddr(addr))
                return SetRegister(ConvertFatekAddrToModbusAddr(addr.Substring(1)), val);
            else
            {
                ErrorMessage = "地址格式不正确";
                return false;
            }
        }

        public Boolean SetRegister(byte[] addr, float val)
        {
            return SetBatchRegister(addr, 2, ByteUtil.SingleToBytes(val));
        }
        public float? ReadFloat(String addr)
        {
            if (IsDoubleAddr(addr))
            {
                var recData = ReadAddr(ModbusFunCode.READ_REGISTER, 2, ConvertFatekAddrToModbusAddr(addr.Substring(1)));
                return ByteUtil.BytesToSingle(recData[1], recData[2], recData[3], recData[4]);
            }
            else
            {
                ErrorMessage = "地址格式不正确";
                return null;
            }
        }
        public int? ReadInt16(String addr)
        {
            var result = ReadRegister(1, addr);
            if (result != null && result.Length > 0) return result[0];
            return null;
        }
        public int? ReadInt32(String addr)
        {
            if (IsDoubleAddr(addr))
            {
                var recData = ReadAddr(ModbusFunCode.READ_REGISTER, 2, ConvertFatekAddrToModbusAddr(addr.Substring(1)));
                return ByteUtil.BytesToInt(recData[1], recData[2], recData[3], recData[4]);
            }
            else
            {
                ErrorMessage = "地址格式不正确";
                return null;
            }
        }
        public int[] ReadInt32(String addr,byte Count)
        {
            if (IsDoubleAddr(addr))
            {
                var recData = ReadAddr(ModbusFunCode.READ_REGISTER, 2*4, ConvertFatekAddrToModbusAddr(addr.Substring(1)));
                if (recData != null)
                {
                    var result = new int[Count];
                    for (byte b = 0; b < Count; b++)
                    {
                        result[b]= ByteUtil.BytesToInt(recData[4*b+1], recData[4 * b + 2], recData[4 * b + 3], recData[4 * b + 4]);
                    }
                    return result;
                }

                return null;
            }
            else
            {
                ErrorMessage = "地址格式不正确";
                return null;
            }
        }
        /*
        public void SetBatchRegister(String startAddr, short len, params byte[] vals)
        {
            SetBatchRegister(ConvertFatekAddrToModbusAddr(startAddr), len, vals);
        }*/
        public Boolean SetBatchRegister(String startAddr, params short[] vals)
        {
            List<Byte> blist = new List<byte>();
            for (short i = 0; i < vals.Length; i++)
            {
                blist.AddRange(ByteUtil.Int2ToBytes(vals[i]));
            }
            short len = (short)vals.Length;
            return SetBatchRegister(ConvertFatekAddrToModbusAddr(startAddr), len, blist.ToArray());
        }
        public Boolean SetBatchRegister(byte[] startAddr, short len, params byte[] vals)
        {
            return SetBatchVal(ModbusFunCode.WRITE_MULTI_REGISTER, startAddr, len, vals);
        }
        public short[] ReadAndSetRegister(String readStartAddr, short readLen, String writeStartAddr, short writeLen, params short[] vals)
        {
            var bvals = new byte[vals.Length * 2];
            for (short i = 0; i < vals.Length; i++)
            {
                var bs = ByteUtil.Int2ToBytes(vals[i]);
                bvals[2 * i] = bs[0];
                bvals[2 * i + 1] = bs[1];
            }
            return ReadAndSetRegister(ConvertFatekAddrToModbusAddr(readStartAddr), readLen, ConvertFatekAddrToModbusAddr(writeStartAddr), writeLen, bvals);
        }
        public short[] ReadAndSetRegister(byte[] readStartAddr, short readLen, byte[] writeStartAddr, short writeLen, params byte[] vals)
        {
            if (readStartAddr == null)
            {
                SetAddrException(ModbusFunCode.READ_WRITE_REGISTER);
                return null;
            }
            var reaLlenb = ByteUtil.Int2ToBytes(readLen);
            var writeLenb = ByteUtil.Int2ToBytes(writeLen);
            var byteCount = (byte)(vals.Length);
            var b1 = new byte[] { PLC_STATION_ID, ModbusFunCode.READ_WRITE_REGISTER, readStartAddr[0], readStartAddr[1], reaLlenb[0], reaLlenb[1],
                writeStartAddr[0],writeStartAddr[1],writeLenb[0],writeLenb[1],  byteCount };
            var b2 = new byte[b1.Length + byteCount];
            b1.CopyTo(b2, 0);
            Array.Copy(vals, 0, b2, b1.Length, byteCount);
            byte[] recData;
            var msgID = send(b2, out recData);
            if (recData == null)
            {
                return null;
            }
            var result = new short[readLen];
            for (short i = 0; i < readLen; i++)
            {
                result[i] = ByteUtil.BytesToInt16(recData[2 * i + 1], recData[2 * i + 2]);
            }
            return result;
        }

        private Boolean SetBatchVal(byte funCode, byte[] startAddr, short len, params byte[] vals)
        {
            if (startAddr == null)
            {
                SetAddrException(funCode);
                return false;
            }
            var lenb = ByteUtil.Int2ToBytes(len);
            var byteCount = (byte)(vals.Length);
            var b1 = new byte[] { PLC_STATION_ID, funCode, startAddr[0], startAddr[1], lenb[0], lenb[1], byteCount };
            var b2 = new byte[b1.Length + byteCount];
            b1.CopyTo(b2, 0);
            Array.Copy(vals, 0, b2, b1.Length, byteCount);
            byte[] recData;
            var msgID = send(b2, out recData);
            return recData != null;
        }
        protected byte[] ReadAddr(byte funCode, short count, params byte[] startAddr)
        {
            if (startAddr == null)
            {
                SetAddrException(funCode);
                return null;
            }
            var countBts = ByteUtil.Int2ToBytes(count);
            var data = new byte[] { PLC_STATION_ID, funCode, startAddr[0], startAddr[1], countBts[0], countBts[1] };
            byte[] recData;
            var msgID = send(data, out recData);
            return recData;
        }
        /// <summary>
        /// data 不含前6个字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Message ID</returns>
        public UInt16? send(byte[] data, out byte[] result)
        {
            lock (monitor)
            {
                if (!Connected)
                {
                    Connect();
                }
                if (Connected && !socket.Poll(Constants.Timespan * 1000, SelectMode.SelectError))
                {

                    UInt16 msgID = GenerateMsgId();
                    var idByte = ByteUtil.Uint2ToBytes(msgID);
                    byte[] other = new byte[] { 0x0, 0x0 };
                    var lensByte = ByteUtil.Int2ToBytes((short)data.Length);
                    byte[] newData = new byte[data.Length + 6];
                    Array.Copy(idByte, 0, newData, 0, 2);
                    Array.Copy(other, 0, newData, 2, 2);
                    Array.Copy(lensByte, 0, newData, 4, 2);
                    Array.Copy(data, 0, newData, 6, data.Length);
                    //Tool.AppLogDebug("输入：" + BitConverter.ToString(newData));

                    try
                    {
                        //var t1 = DateTime.Now;
                        socket.Send(newData);
                        //var t2 = DateTime.Now;
                        //var df = t2 - t1;
                        //Console.WriteLine($"发 {df.TotalMilliseconds} ms");
                        result = ReceiveData(msgID);
                    }
                    catch (Exception ex)
                    {
                        Connected = false;
                        ErrorMessage = ex.Message;
                        try
                        {
                            socket.Close();
                        }
                        catch { }
                        result = null;
                        return null;
                    }
                    
                    return msgID;
                }
                else
                {
                    Connected = false;
                    ErrorMessage = $"未能成功连接到{DeviceName}";
                    result = null;
                    return null;
                }
            }
        }
        public abstract byte[] ConvertFatekAddrToModbusAddr(String FatekAddr);
        public delegate void ErrorDelegate(bool IsOnline, String errorMsg);
        public event ErrorDelegate OnError;

        
        
        public void Connect()
        {

            IPEndPoint ie = new IPEndPoint(IPAddress.Parse(ipAddr), port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.ReceiveTimeout = 10000;

            //将套接字与远程服务器地址相连  
            try
            {
                socket.Connect(ie);
                Connected = socket.Connected;
                this.ErrorMessage = "OK";
            }
            catch (SocketException e)
            {
                //Console.Out.WriteLine("连接服务器失败  " + e.Message);
                Tool.AppLogError($"连接【{ipAddr}】失败！");
                Tool.AppLogError(e);
                this.ErrorMessage = "设备联机失败";
                return;
            }
            /*
            ThreadStart myThreaddelegate = new ThreadStart(ReceiveMsg);
            var myThread = new Thread(myThreaddelegate);
            myThread.Start();
            */

        }
        private byte[] ReceiveData(UInt16? MsgId)
        {
            byte[] data = new byte[1024];//定义数据接收数组  
            try
            {
                //var t1 = DateTime.Now;
                socket.Receive(data);//接收数据到data数组
                //var t2 = DateTime.Now;
                //var df = t2 - t1;
                //Console.WriteLine($"收 {df.TotalMilliseconds} ms");
                
                //Console.WriteLine(data);
            }
            catch (Exception ex)
            {
                this.Connected = false;
                ErrorMessage = ex.Message;
                
                try
                {
                    socket.Close();
                }
                catch { }
                return null;
            }
            var id = ByteUtil.BytesToUInt16(data[0], data[1]);
            if (!MsgId.HasValue || id != MsgId.Value)
            {
                ErrorMessage = "信息标识号不正确！";
                return null;
            }
            int length = ByteUtil.BytesToInt16(data[4], data[5]);//读取数据长度  
                                                        //Console.WriteLine(String.Format("长度={0}", length));


            if (length == 3 && data[7] > 0x80)
            {
                var errorCode = (ModbusErrorEnum)data[8];
                this.ErrorMessage = errorCode.GetDescription();
                return null;
            }
            Byte[] realData = new byte[length - 2];//定义所要显示的接收的数据的长度  
            Array.Copy(data, 8, realData, 0, length - 2);
            return realData;
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
                    int length = ByteUtil.BytesToInt16(data[4], data[5]);//读取数据长度  
                    //Console.WriteLine(String.Format("长度={0}", length));
                    Byte[] realData = new byte[length + 6];//定义所要显示的接收的数据的长度  
                    for (int i = 0; i <= length + 5; i++)//将要显示的数据存放到数组datashow中  
                        realData[i] = data[i];
                    if (length == 3 && data[7] > 0x80)
                    {
                        var errorCode = (ModbusErrorEnum)data[8];
                        this.ErrorMessage = errorCode.GetDescription();
                    }
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
        private Boolean[] _RunResult;
        public Boolean[] RunResult
        {
            get
            {
                lock (resultLock)
                {
                    return _RunResult;
                }
            }set{
                _RunResult = value;
            }
        }
        Object lockTread = new object();
        private Thread ScanTread;
        /// <summary>
        /// 开始结束扫描
        /// </summary>
        public void StartScan()
        {
            scanFlag = true;
            lock (lockTread)
            {
                if (ScanTread == null)
                {
                    ScanTread = new Thread(new ThreadStart(this.ScanRunResult));
                    ScanTread.IsBackground = true;
                    ScanTread.Start();
                }
            }
        }
        /// <summary>
        /// 停止结果扫描
        /// </summary>
        public void StopScan()
        {
            scanFlag = false;
            ScanTread = null;
        }
        private IDictionary<String, bool> _WatchEventCoil;
        public IDictionary<String,bool> WatchEventCoil { get
            {
                if (_WatchEventCoil == null)
                {
                    _WatchEventCoil = new Dictionary<String, bool>();
                }
                return _WatchEventCoil;
            }
        }
        protected Boolean scanFlag;
        public virtual void ScanRunResult()
        {
            //int i = 0;
            while (scanFlag)
            {
                this.ScanRestEvent.WaitOne();
                bool[] result = null;
                if (Key == "PLC")
                {
                    result = this.ReadCoil(100, Constants.OutputStartAddr);//Y300结果输出起始地址
                    if (WatchEventCoil .Count>0)
                    {
                        var ch=WatchEventCoil.Where(kv => this[kv.Key] != kv.Value).ToArray();
                        if (ch.Length > 0)
                        {
                            foreach(var kv in ch)
                            {
                                WatchEventCoil.Remove(kv);
                                WatchEventCoil.Add(kv.Key, this[kv.Key]);
                            }
                            OnCoilSwitchEvent?.Invoke(ch);
                        }
                    }
                }
                else if (Key == "PCB")
                    result = this.ReadCoil(16, 1, "0");
                lock (resultLock)
                {
                    _RunResult = result;
                }
                Thread.Sleep(Constants.Timespan);//间隔时间
            }
        }
        public delegate void CoilSwitchEventHandler(KeyValuePair<String,bool>[] kvs);
        /// <summary>
        /// 监测扫描线圈中特定线圈值变化事件，这里主要用于监测混匀器是否在零位（混匀器是否脱离）
        /// </summary>
        public event CoilSwitchEventHandler OnCoilSwitchEvent;
        private int? _ScanStartIndex;
        private int ScanStartIndex
        {
            get
            {
                if (!_ScanStartIndex.HasValue)
                {
                    _ScanStartIndex = int.Parse(Constants.OutputStartAddr.Substring(1));
                }
                return _ScanStartIndex.Value;
            }
        }
        /// <summary>
        /// 查询线圈当前状态
        /// </summary>
        /// <param name="coilAddr"></param>
        /// <returns></returns>
        public bool this[String coilAddr]
        {
            get
            {
                if (String.IsNullOrEmpty(coilAddr))
                {
                    return false;
                }
                int start = ScanStartIndex;
                int end = int.Parse(coilAddr.Substring(1));
                try
                {
                    if (end >= start)
                    {
                        return this.RunResult[end - start];
                    }
                }
                catch (Exception ex)
                {
                    Tool.AppLogError(ex);
                }
                return false;
            }
            set
            {
                if (String.IsNullOrEmpty(coilAddr))
                {
                    return;
                }
                int start = int.Parse(Constants.OutputStartAddr.Substring(1));
                int end = int.Parse(coilAddr.Substring(1));
                try
                {
                    if (end >= start)
                    {
                        this.RunResult[end - start] = value;
                    }
                }
                catch (Exception ex)
                {
                    Tool.AppLogDebug(ex);
                }
            }
        }

        public bool Doned(int timeout, bool checkVal, params PLCParameter<Boolean>[] coils)
        {
            return Doned(timeout, checkVal, coils.Select(coil => { return coil.Addr; }).ToArray());
        }
        public bool Doned(int timeout, bool checkVal, params String[] outAddrs)
        {
            //Thread.Sleep(Constants.Timespan);
            foreach (string addr in outAddrs)
            {
                this[addr] = !checkVal;
            } 
            var result = false;
            int count = 0;
            while (Constants.Timespan * count < timeout)
            {
                result = true;
                count++;
                foreach (string addr in outAddrs)
                {
                    result = result && (checkVal ? this[addr] : !this[addr]);
                }

                if (result) break;
                Thread.Sleep(Constants.Timespan);
            }
            return result;
        }

    }
}
