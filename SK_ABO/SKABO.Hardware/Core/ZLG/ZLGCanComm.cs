using SKABO.ActionEngine;
using SKABO.Common;
using SKABO.Common.Utils;
using SKABO.Hardware.Enums;
using SKABO.MAI.ErrorSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SKABO.Hardware.Core.ZLG
{
    public class RecSafeData
    {
        public static readonly object Lock = new object();
        public string data;
        public RecSafeData(string datatem)
        {
            data = datatem;
        }
    }

    public class SendSafeData
    {
        public VCI_CAN_OBJ sendobj;
        public string key_str;
        public bool is_wait;
        public int wait_time = 0;
        public SendSafeData(VCI_CAN_OBJ sendobj_tem,bool is_wait_tem, string key_str_tem, int wait_time_tem)
        {
            sendobj = sendobj_tem;
            key_str = key_str_tem;
            is_wait = is_wait_tem;
            wait_time = wait_time_tem;
        }
        public SendSafeData(int wait_time_tem)
        {
            wait_time = wait_time_tem;
        }
    }

    public class ZLGCanComm:AbstractCanComm
    {
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadErrInfo(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_ERR_INFO pErrInfo);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadCANStatus(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_STATUS pCANStatus);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_SetReference(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, UInt32 RefType, ref byte pData);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);

        //[DllImport("controlcan.dll")]
        //static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);
        [DllImport("controlcan.dll", CharSet = CharSet.Ansi)]
        static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, IntPtr pReceive, UInt32 Len, Int32 WaitTime);

        const UInt32 DevType = 3;//VCI_USBCAN1
        const UInt32 DevIndex = 0;
        const UInt32 CanIndex = 0;
        const byte ID = 0x01;
        public Dictionary<string, bool> Rec_SafeData = new Dictionary<string, bool>();
        public Dictionary<string, CanListenFun> ListenFunList = new Dictionary<string, CanListenFun>();
        public List<SendSafeData> Send_Datas = new List<SendSafeData>();
        public static readonly object Listen_Lock = new object();
        public static readonly object Send_Lock = new object();
        public Thread rec_work_thread = null;
        public Thread send_work_thread = null;
        public override bool Connect()
        {
            if (!IsOpen)
            {
                this.IsOpen = VCI_OpenDevice(DevType, DevIndex, CanIndex) != 0;
                if (this.IsOpen)
                {
                    VCI_INIT_CONFIG config = new VCI_INIT_CONFIG();
                    config.AccCode = 0x00;
                    config.AccMask = System.Convert.ToUInt32("0xFFFFFFFF" , 16);
                    //波特率 250kbps Timing0 = 0x01  Timing1 = 0x1c
                    //波特率 500kbps Timing0 = 0x00  Timing1 = 0x1c
                    //波特率 800kbps Timing0 = 0x00  Timing1 = 0x16
                    //波特率 1000kbps Timing0 = 0x00  Timing1 = 0x14
                    config.Timing0 = 0x00;
                    config.Timing1 = 0x1c;
                    config.Filter = 0x01;
                    config.Mode = 0x00;
                    VCI_InitCAN(DevType, DevIndex, CanIndex, ref config);
                    VCI_StartCAN(DevType, DevIndex, CanIndex);
                    VCI_BOARD_INFO br = new VCI_BOARD_INFO();
                    VCI_ReadBoardInfo(DevType, DevIndex, ref br);
                    Console.WriteLine(System.Text.Encoding.Default.GetString(br.str_Serial_Num));
                    ErrorMessage = System.Text.Encoding.Default.GetString( br.str_Serial_Num);
                    if (rec_work_thread == null)
                    {
                        rec_work_thread = new Thread(RecWorkThread);
                        rec_work_thread.Start();
                    }
                    if (send_work_thread == null)
                    {
                        send_work_thread = new Thread(SendWorkThread);
                        send_work_thread.Start();
                    }
                }
                else
                {
                    ErrorMessage = "Can卡初始化失败！";
                }
            }
            return this.IsOpen;
        }

        ~ZLGCanComm()
        {
            if (rec_work_thread != null)
            {
                rec_work_thread.Join();
                rec_work_thread.Abort();
            }
            if (send_work_thread != null)
            {
                send_work_thread.Join();
                send_work_thread.Abort();
            }
        }

        public void RecWorkThread()
        {
            while(true)
            {
                try
                {
                    //Thread.Sleep(1);
                    Can_Rec_Tick();
                }
                catch (Exception ex)
                {

                }
            }
        }

        public void SendWorkThread()
        {
            while (true)
            {
                try
                {
                    //Can_Send_Tick();
                }
                catch (Exception ex)
                {

                }
            }
        }

        public override bool Close()
        {
            uint res = 0;
            if (this.IsOpen)
            {
                res = VCI_CloseDevice(DevType, 0);
                this.IsOpen = res != 0;
            }
            return res == 0;
        }
        public override Boolean ClearBuffer()
        {
            if (IsOpen)
            {
                return VCI_ClearBuffer(DevType, DevIndex, CanIndex) != 0;
            }
            else
            {
                return false;
            }
        }
        public unsafe override bool Send(byte targetId, byte[] data, byte mask, bool is_wait)
        {
            string sendmsg = "";
            bool is_send_ok = false;
            lock (Send_Lock)
            {
                Thread.Sleep(2);
                VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
                //sendobj.Init();
                string adressstr = string.Format("{0:D2}-{1:x}{2:x}", targetId, data[2], data[3]);
                ClsCoilMap(adressstr);

                sendobj.SendType = 0x00;//02 自发自收 //0x00;//正常发送
                sendobj.RemoteFlag = 0x00;//数据帧
                sendobj.ExternFlag = 0x00;//标准帧
                targetId = (byte)((0xFF << 3 | mask) & targetId);
                var canid = targetId << 3 | (mask & 0b111);
                sendobj.ID = System.Convert.ToUInt32(canid);
                int len = data.Length;
                sendobj.DataLen = System.Convert.ToByte(len);

                sendmsg = string.Format("send:0x{0:x4} ", canid);
                for (var j = 0; j < Math.Min(len, 8); j++)
                {
                    sendobj.Data[j] = data[j];
                    sendmsg += string.Format("0x{0:x2} ", data[j]);
                }
                TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                sendmsg += "TimeStamp:" + Convert.ToInt64(ts.TotalSeconds).ToString();
                //安全重发
                (String CanAddress, CanFunCodeEnum FunCode) = ParseCanData(data);
                string key_str = ByteUtil.ToHex(data[0]) + ByteUtil.ToHex(targetId) + ByteUtil.ToHex(data[1]) + ByteUtil.ToHex(data[2]) + ByteUtil.ToHex(data[3]);
                lock (RecSafeData.Lock)
                {
                    if (!Rec_SafeData.ContainsKey(key_str))
                        Rec_SafeData.Add(key_str, false);
                    else
                        Rec_SafeData[key_str] = false;
                }
                bool is_find = is_wait ? false : true;
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine(sendmsg);
                    is_send_ok = VCI_Transmit(DevType, DevIndex, CanIndex, ref sendobj, 1) != 0;

                    if (!is_send_ok) { continue; }
                    if (is_wait)
                    {

                        for (int wait_count = 0; wait_count < 200; wait_count++)
                        {
                            lock (RecSafeData.Lock)
                            {
                                if (Rec_SafeData[key_str])
                                {
                                    Rec_SafeData[key_str] = false;
                                    is_find = true;
                                    break;
                                }
                            }
                            if (is_find) break;
                            Thread.Sleep(1);
                        }
                    }
                    if (is_find) break;
                    Thread.Sleep(100);
                }
                is_send_ok = is_find;
            }
            if (is_send_ok == false)
            {
               if(ErrorSystem.WriteActError("板号" + targetId + ":通讯错误!\n错误代码:\n" + sendmsg.Replace("send:", ""), true, true, 20)==false)
               {
                    lock (ActionManager.lockObj)
                    {
                        var action_manager = ActionManager.getInstance();
                        var experiment_logic = ExperimentLogic.getInstance();
                        action_manager.removeAllActions();
                        experiment_logic.DelAllPackage();
                    }
                }
            }
            return is_send_ok;
        }

        public override Boolean Wait(int time)
        {
            lock (Send_Lock)
            {
                Send_Datas.Add(new SendSafeData(time));
            }
            return true;
        }

        //public unsafe override bool Send(byte targetId, byte[] data, byte mask, bool is_wait)
        //{
        //    lock (Send_Lock)
        //    {
        //        string adressstr = string.Format("{0:D2}-{1:x}{2:x}", targetId, data[2], data[3]);
        //        ClsCoilMap(adressstr);
        //        //生成sendobj
        //        VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
        //        sendobj.SendType = 0x00;//02 自发自收 //0x00;//正常发送
        //        sendobj.RemoteFlag = 0x00;//数据帧
        //        sendobj.ExternFlag = 0x00;//标准帧
        //        targetId = (byte)((0xFF << 3 | mask) & targetId);
        //        var canid = targetId << 3 | (mask & 0b111);
        //        sendobj.ID = System.Convert.ToUInt32(canid);
        //        int len = data.Length;
        //        sendobj.DataLen = System.Convert.ToByte(len);
        //        for (var i = 0; i < Math.Min(len, 8); i++)
        //        {
        //            sendobj.Data[i] = data[i];
        //        }
        //        //生成key
        //        (String CanAddress, CanFunCodeEnum FunCode) = ParseCanData(data);
        //        string key_str = ByteUtil.ToHex(data[0]) + ByteUtil.ToHex(targetId) + ByteUtil.ToHex(data[1]) + ByteUtil.ToHex(data[2]) + ByteUtil.ToHex(data[3]);
        //        lock (RecSafeData.Lock)
        //        {
        //            if (!Rec_SafeData.ContainsKey(key_str))
        //                Rec_SafeData.Add(key_str, false);
        //            else
        //                Rec_SafeData[key_str] = false;
        //        }
        //        Send_Datas.Add(new SendSafeData(sendobj, is_wait, key_str,0));
        //        return true;
        //    }
        //}

        unsafe public override void Can_Send_Tick()
        {
            lock (Send_Lock)
            {
                foreach (var send_data in Send_Datas)
                {
                    if (send_data.wait_time == 0)
                    {
                        VCI_CAN_OBJ sendobj = send_data.sendobj;
                        string sendmsg = string.Format("send:0x{0:x4} ", sendobj.ID);
                        for (var i = 0; i < sendobj.DataLen; i++)
                        {
                            sendmsg += string.Format("0x{0:x2} ", sendobj.Data[i]);
                        }
                        Console.WriteLine(sendmsg);
                        VCI_Transmit(DevType, DevIndex, CanIndex, ref sendobj, 1);
                        bool is_find = send_data.is_wait ? false : true;
                        if (send_data.is_wait)
                        {
                            for (int wait_count = 0; wait_count < 200; wait_count++)
                            {
                                lock (RecSafeData.Lock)
                                {
                                    if (Rec_SafeData[send_data.key_str])
                                    {
                                        Rec_SafeData[send_data.key_str] = false;
                                        is_find = true;
                                        break;
                                    }
                                }
                                if (is_find) break;
                                Thread.Sleep(1);
                            }
                        }
                    }
                    else
                    {
                        Thread.Sleep(send_data.wait_time);
                    }

                }
                Send_Datas.Clear();
            }
        }

        unsafe public override void Can_Rec_Tick()
        {
            UInt32 res = new UInt32();
            res = VCI_GetReceiveNum(DevType, DevIndex, CanIndex);
            if (res == 0) return;
            /////////////////////////////////////
            UInt32 con_maxlen = 100;
            IntPtr pt = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(VCI_CAN_OBJ)) * (Int32)con_maxlen);
            res = VCI_Receive(DevType, DevIndex, CanIndex, pt, con_maxlen, 5);
            ////////////////////////////////////////////////////////
            String str = "";
            for (UInt32 i = 0; i < res; i++)
            {
                VCI_CAN_OBJ? reviceObj = null;
                try
                {
                    reviceObj = (VCI_CAN_OBJ)Marshal.PtrToStructure((IntPtr)(pt.ToInt64() + i * Marshal.SizeOf(typeof(VCI_CAN_OBJ))), typeof(VCI_CAN_OBJ));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                if (!reviceObj.HasValue)
                {
                    break;
                }
                var obj = reviceObj.Value;
                var targetID = (byte)(obj.ID >> 3);
                var mask = (byte)(obj.ID & 0b111);

                Console.Write("rec: 0x{0:x4} ", obj.ID);
                for (var j = 0; j < obj.DataLen; j++)
                {
                    Console.Write("0x{0:x2} ", obj.Data[j]);
                }
                TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                Console.WriteLine("TimeStamp:" + Convert.ToInt64(ts.TotalSeconds).ToString());

                if (targetID != (byte)(ID & (0xFF << 3 | mask)))
                {
                    Console.WriteLine("ID不符，数据丢弃");
                    continue;
                }

                //Console.WriteLine("objlen=" + obj.DataLen);
                str = "接收到数据: ";
                str += "  设备ID:0x" + Convert.ToString(targetID, 16);
                str += "  ID掩码:0x" + Convert.ToString(mask, 16);
                str += "  帧格式:";
                if (obj.RemoteFlag == 0)
                    str += "数据帧 ";
                else
                    str += "远程帧 ";
                if (obj.ExternFlag == 0)
                    str += "标准帧 ";
                else
                    str += "扩展帧 ";

                //////////////////////////////////////////
                if (obj.RemoteFlag == 0)
                {
                    str += "数据: ";
                    byte len = (byte)(Math.Min(obj.DataLen % 50, 8));
                    byte[] data = new byte[len];
                    for (byte j = 0; j < len; j++)
                    {
                        data[j] = obj.Data[j];
                        str += " " + ByteUtil.ToHex(obj.Data[j]);
                    }
                    //bool is_mask = (data[0] & 0x07) == 0x07;
                    //data[0] = is_mask ? (byte)(data[0] >> 3) : data[0];
                    (String CanAddress, CanFunCodeEnum FunCode) = ParseCanData(data);
                    if (FunCode == CanFunCodeEnum.UPLOAD_COIL||FunCode == CanFunCodeEnum.UPLOAD_REGISTER || FunCode == CanFunCodeEnum.READ_COIL || FunCode == CanFunCodeEnum.READ_REGISTER || FunCode == CanFunCodeEnum.WRITE_SINGLE_COIL || FunCode == CanFunCodeEnum.WRITE_SINGLE_REGISTER)
                    {
                        lock (RecSafeData.Lock)
                        {
                            bool is_pass = true;
                            if (FunCode == CanFunCodeEnum.WRITE_SINGLE_COIL || FunCode == CanFunCodeEnum.WRITE_SINGLE_REGISTER)
                                is_pass = data[3] == 0xb0 || data[4] == 0xff|| data[5] == 0xff;
                            if (is_pass == false)
                            {
                                ErrorSystem.WriteActError("通讯返回错误！", false);
                            }
                            if (is_pass)
                            {
                                string key_str = ByteUtil.ToHex(targetID) + ByteUtil.ToHex(data[0])+ ByteUtil.ToHex(data[1]) + ByteUtil.ToHex(data[2]) + ByteUtil.ToHex(data[3]);
                                if (!Rec_SafeData.ContainsKey(key_str))
                                    Rec_SafeData.Add(key_str, true);
                                else
                                    Rec_SafeData[key_str] = true;
                                if (ListenFunList.ContainsKey(key_str))
                                {
                                    var task = Task.Run(() =>
                                    {
                                        ListenFunList[key_str](targetID, data);
                                    });
                                }
                                    
                            }
                        }
                    }
                    switch (FunCode)
                    {
                        case CanFunCodeEnum.READ_COIL:
                            {

                                byte length = 1;
                                String canId = CanAddress.Split('-')[0];
                                UInt16 addr = ByteUtil.BytesToUInt16(data[2], data[3]);
                                for (byte k = 0; k < length; k++)
                                {
                                    byte curData = data[k / 8 + 4];
                                    String key = canId + "-" + Convert.ToString(addr + k, 16).PadLeft(4, '0').ToUpper();
                                    //var val = ((0x01 << (k % 8)) & curData) == 0x01;
                                    var val = data[4] == 0xff;
                                    SetBool(key, val);
                                }
                                break;
                            }
                        case CanFunCodeEnum.UPLOAD_COIL:
                            {

                                byte length = data[4];
                                String canId = CanAddress.Split('-')[0];
                                UInt16 addr = ByteUtil.BytesToUInt16(data[2], data[3]);
                                for (byte k = 0; k < length; k++)
                                {
                                    byte curData = data[k / 8 + 5];
                                    String key = canId + "-" + Convert.ToString(addr + k, 16).PadLeft(4, '0').ToUpper();
                                    var val = ((0x01 << (k % 8)) & curData) == 0x01;
                                    SetBool(key, val);
                                }
                                break;
                            }
                        case CanFunCodeEnum.READ_REGISTER:
                        case CanFunCodeEnum.UPLOAD_REGISTER:
                            {

                                var val = ByteUtil.BytesToInt(data[4], data[5], data[6], data[7]);
                                SetInt(CanAddress, val);
                                break;
                            }

                        case CanFunCodeEnum.WRITE_MULTI_COIL:
                            {
                                break;
                            }
                        case CanFunCodeEnum.WRITE_SINGLE_COIL:
                            {
                                break;
                            }
                        case CanFunCodeEnum.WRITE_SINGLE_REGISTER:
                            {
                                break;
                            }
                    }
                }
                //Console.WriteLine(str);
            }
            Marshal.FreeHGlobal(pt);
        }

        private Byte[] GenerateSendData(byte[] addr, CanFunCodeEnum FunCode,params byte[] datas)
        {
            Byte[] result = new Byte[8];
            result[0] = ID;
            result[1] = (Byte)FunCode;
            result[2] = addr[0];
            result[3] = addr[1];
            var len = Math.Min(datas.Length, 4);
            for(byte i=0;i< len; i++)
            {
                result[i + 4] = datas[i];
            }
            return result;
        }
      
        public override bool SetCoil(string CanAddress, bool value, byte mask,bool is_wait=true)
        {
            var addrs = ParseCanAddress(CanAddress);
            if (addrs == null) return false;
            (Byte TargetId, Byte[] addr) = addrs.Value;
            Byte val = (Byte)(value ? 0XFF : 0X00);
            var sendData=GenerateSendData(addr, CanFunCodeEnum.WRITE_SINGLE_COIL, val);
            return Send(TargetId, sendData, mask, is_wait);
        }

        public bool SetCoil(Byte tager_id, string CanAddress, bool value, byte mask, bool is_wait = true)
        {
            var addrs = ParseCanAddress(CanAddress);
            if (addrs == null) return false;
            (Byte TargetId, Byte[] addr) = addrs.Value;
            Byte val = (Byte)(value ? 0XFF : 0X00);
            TargetId = tager_id;
            if (TargetId == 30) is_wait = false;
            var sendData = GenerateSendData(addr, CanFunCodeEnum.WRITE_SINGLE_COIL, val);
            return Send(TargetId, sendData, mask, is_wait);
        }

        public override bool SetByte(string CanAddress, byte value, byte mask, bool is_wait = true)
        {
            var addrs = ParseCanAddress(CanAddress);
            if (addrs == null) return false;
            (Byte TargetId, Byte[] addr) = addrs.Value;
            Byte val = (Byte)value;
            var sendData = GenerateSendData(addr, CanFunCodeEnum.WRITE_SINGLE_COIL, val);
            return Send(TargetId, sendData, mask, is_wait);
        }

        public override bool SetCoils(string StartCanAddress,byte mask,bool is_wait, params Boolean[] values)
        {
            var addrs = ParseCanAddress(StartCanAddress);
            if (addrs == null) return false;
            (Byte TargetId, Byte[] addr) = addrs.Value;
            var len = (Byte)values.Length;
            var data = new Byte[len/8+(len % 8==0?0:1)+1];
            data[0] = len;
            Byte curByte = 0x00;
            for(byte i = 0; i < len; i++)
            {
                if (i>0 && (i % 8) == 0)
                {
                    data[i / 8 ] = curByte;
                    curByte = 0x00;
                }
                curByte= (byte)(curByte | (0x01<<i));
            }
            data[len / 8 + (len % 8 == 0 ? 0 : 1)] = curByte;
            var sendData= GenerateSendData(addr, CanFunCodeEnum.WRITE_MULTI_COIL, data);
            return Send(TargetId, sendData, mask, is_wait);
        }

        public override bool SetBytes(string StartCanAddress, byte mask,bool is_wait,params byte[] values)
        {
            bool ret = false;
            var addrs = ParseCanAddress(StartCanAddress);
            if (addrs == null) return false;
            (Byte TargetId, Byte[] addr) = addrs.Value;
            var len = (Byte)values.Length;
            var data = new Byte[4];
            for (int i=0;i<(len/3);i++)
            {
                data[0] = (Byte)(i * 3);
                for(int j=0;j<3;j++)
                {
                    data[j + 1] = values[i * 3 + j];
                }
                var sendData = GenerateSendData(addr, CanFunCodeEnum.WRITE_MULTI_BYTE, data);
                ret = Send(TargetId, sendData, mask, is_wait);
                if (ret == false) break;
            }
            return ret;
        }

        public override bool ReadCoils(string StartCanAddress, byte len, bool is_wait)
        {
            var addrs = ParseCanAddress(StartCanAddress);
            if (addrs == null) return false;
            (Byte TargetId, Byte[] addr) = addrs.Value;
            var sendData = GenerateSendData(addr, CanFunCodeEnum.READ_COIL, len);
            return Send(TargetId, sendData, 0b111, is_wait);
        }

        public override Boolean ReadRegister(string CanAddress, byte mask, bool is_wait)
        {
            var addrs = ParseCanAddress(CanAddress);
            if (addrs == null) return false;
            (Byte TargetId, Byte[] addr) = addrs.Value;
            var sendData = GenerateSendData(addr, CanFunCodeEnum.READ_REGISTER);
            return Send(TargetId, sendData, mask, is_wait);
        }

        public override Boolean SetRegister(string CanAddress, int value, byte mask, bool is_wait)
        {
            var addrs = ParseCanAddress(CanAddress);
            if (addrs == null) return false;
            (Byte TargetId, Byte[] addr) = addrs.Value;
            var sendData = GenerateSendData(addr, CanFunCodeEnum.WRITE_SINGLE_REGISTER, ByteUtil.Int4ToBytes(value));
            return Send(TargetId, sendData, mask, is_wait);
        }

        public Boolean SetRegister(Byte tager_id, string CanAddress, int value, byte mask, bool is_wait)
        {
            var addrs = ParseCanAddress(CanAddress);
            if (addrs == null) return false;
            (Byte TargetId, Byte[] addr) = addrs.Value;
            TargetId = tager_id;
            if (TargetId == 30) is_wait = false;
            var sendData = GenerateSendData(addr, CanFunCodeEnum.WRITE_SINGLE_REGISTER, ByteUtil.Int4ToBytes(value));
            return Send(TargetId, sendData, mask, is_wait);
        }

        public override void SetListenFun(String CanAddress, CanFunCodeEnum UpType, CanListenFun ListenFun)
        {
            lock(Listen_Lock)
            {
                var addrs = ParseCanAddress(CanAddress);
                if (addrs == null) return;
                (Byte TargetId, Byte[] addr) = addrs.Value;
                var data = GenerateSendData(addr, UpType, 0);
                string key_str = ByteUtil.ToHex(data[0]) + ByteUtil.ToHex(TargetId) + ByteUtil.ToHex(data[1]) + ByteUtil.ToHex(data[2]) + ByteUtil.ToHex(data[3]);
                if (ListenFunList.ContainsKey(key_str))
                    ListenFunList[key_str] = ListenFun;
                else
                    ListenFunList.Add(key_str, ListenFun);
            }
        }

        private (String CanAddress, CanFunCodeEnum FunCode) ParseCanData(byte[] recData)
        {
            byte sourceID = recData[0];
            String CanAddress = $"{ByteUtil.ToHex(recData[0])}-{ByteUtil.ToHex(recData[2],recData[3])}";
            CanFunCodeEnum FunCode = (CanFunCodeEnum)recData[1];
            return (CanAddress, FunCode);
        }
    }
}
