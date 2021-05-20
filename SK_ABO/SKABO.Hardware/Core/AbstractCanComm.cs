using SKABO.Common;
using SKABO.Common.Models.Communication;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Common.Utils;
using SKABO.Hardware.Core.ZLG;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SKABO.MAI.ErrorSystem;
using SKABO.Hardware.Enums;

namespace SKABO.Hardware.Core
{
    public static class Unity
    {
        //简单类型转换
        public static T AsType<T>(this object value, T defaultValue = default(T))
        {
            T res = defaultValue;
            if (value == null)//为null返回默认值
            {
                return res;
            }
            object resValue = null;
            Type fromType = value.GetType();
            Type toType = typeof(T);
            TypeCode fromTypeCode = Type.GetTypeCode(fromType);//来源类型
            TypeCode toTypeCode = Type.GetTypeCode(toType);//结果类型
            try
            {
                switch (toTypeCode)
                {

                    case TypeCode.Boolean:
                        #region bool 转换
                        switch (fromTypeCode)
                        {
                            case TypeCode.SByte:
                                resValue = (sbyte)value > 0;
                                break;
                            case TypeCode.Byte:
                                resValue = (byte)value > 0;
                                break;
                            case TypeCode.Int16:
                                resValue = (short)value > 0;
                                break;
                            case TypeCode.UInt16:
                                resValue = (ushort)value > 0;
                                break;
                            case TypeCode.Int32:
                                resValue = (int)value > 0;
                                break;
                            case TypeCode.UInt32:
                                resValue = (uint)value != 0;
                                break;
                            case TypeCode.Int64:
                                resValue = (long)value > 0L;
                                break;
                            case TypeCode.UInt64:
                                resValue = (ulong)value > 0L;
                                break;
                            case TypeCode.String:
                                try
                                {
                                    resValue = int.Parse((string)value) > 0;
                                }
                                catch
                                {
                                }
                                break;
                        }
                        resValue = ((IConvertible)value).ToBoolean(null);
                        #endregion
                        break;
                    case TypeCode.Char:
                        resValue = ((IConvertible)value).ToChar(null);
                        break;
                    case TypeCode.SByte:
                        resValue = ((IConvertible)value).ToSByte(null);

                        break;
                    case TypeCode.Byte:
                        resValue = ((IConvertible)value).ToByte(null);

                        break;
                    case TypeCode.Int16:
                        resValue = ((IConvertible)value).ToInt16(null);
                        break;
                    case TypeCode.UInt16:
                        resValue = ((IConvertible)value).ToUInt16(null);
                        break;
                    case TypeCode.Int32:
                        resValue = ((IConvertible)value).ToInt32(null);
                        break;
                    case TypeCode.UInt32:
                        resValue = ((IConvertible)value).ToUInt32(null);

                        break;
                    case TypeCode.Int64:
                        resValue = ((IConvertible)value).ToInt64(null);

                        break;
                    case TypeCode.UInt64:
                        resValue = ((IConvertible)value).ToUInt64(null);

                        break;
                    case TypeCode.Single:
                        resValue = ((IConvertible)value).ToSingle(null);

                        break;
                    case TypeCode.Double:
                        resValue = ((IConvertible)value).ToDouble(null);
                        break;
                    case TypeCode.Decimal:
                        resValue = ((IConvertible)value).ToDecimal(null);
                        break;
                    case TypeCode.DateTime:
                        resValue = ((IConvertible)value).ToDateTime(null);
                        break;
                    case TypeCode.String:
                        resValue = (value == null ? string.Empty : ((IConvertible)value).ToString(null));
                        break;
                    default:
                        resValue = value;
                        break;
                }


                if (resValue != null)
                {
                    res = (T)resValue;
                }
            }
            catch (Exception)
            {

            }
            return res;
        }
    }

    /// <summary>
    /// CanAddress :FF-FFFF,“-”前为设备CanID,“-”后为寄存器地址
    /// </summary>
    public abstract class AbstractCanComm
    {
        private object BoolLock = new object();
        private object IntlLock = new object();
        #region 事件
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
                OnError?.Invoke(IsOpen, value);
            }
        }
        public delegate void ErrorDelegate(bool IsOnline, String errorMsg);
        public event ErrorDelegate OnError;
        #endregion 事件
        public ValueTuple<Byte, Byte[]>? ParseCanAddress(String CanAddress)
        {
            if (String.IsNullOrEmpty(CanAddress)) return null;
            Regex reg = new Regex(@"^[\dA-Fa-f]{2}\-[\dA-Fa-f]{4}$");
            if (!reg.IsMatch(CanAddress)) return null;
            String[] strs = CanAddress.Split('-');
            Byte targetId = Convert.ToByte(strs[0], 16);
            Byte[] bs = new byte[] { Convert.ToByte(strs[1].Substring(0, 2), 16), Convert.ToByte(strs[1].Substring(2, 2), 16) };
            return ValueTuple.Create(targetId, bs);
        }
        public static string GetStacktRace()
        {
            string stack_name = "";
            var stacktrace = new System.Diagnostics.StackTrace();
            for (int i = 0; i < stacktrace.FrameCount; i++)
            {
                var method = stacktrace.GetFrame(i).GetMethod();
                stack_name = stack_name + method.Name + "_";
            }
            return stack_name;
        }
        public delegate void CanListenFun(int tagerid,byte[] data);
        public static string last_error = ""; //最近的错误代码
        unsafe public abstract void Can_Rec_Tick();
        unsafe public abstract void Can_Send_Tick();
        public bool IsOpen { get; set; }
        public IDictionary<String, bool> CoilMap = new ConcurrentDictionary<String, bool>();
        public IDictionary<String, int> RegisterMap = new ConcurrentDictionary<String, int>();
        public abstract Boolean Connect();
        public abstract Boolean Close();
        public abstract Boolean ClearBuffer();
        public abstract Boolean Send(byte targetId, byte[] data, byte mask,bool is_wait=true);
        public abstract Boolean Wait(int time);
        public abstract bool SetCoil(string CanAddress, bool value, byte mask,bool is_wait = true);
        public abstract bool SetByte(string CanAddress, byte value, byte mask, bool is_wait = true);
        public abstract Boolean SetCoils(String StartCanAddress, byte mask,bool is_wait, params Boolean[] values);
        public abstract Boolean SetBytes(String StartCanAddress, byte mask,bool is_wait, params byte[] values);
        public abstract Boolean ReadCoils(String StartCanAddress, byte len, bool is_wait = true);
        public abstract bool ReadRegister(String CanAddress, byte mask, bool is_wait = true);
        public abstract bool SetRegister(String CanAddress, int value, byte mask,bool is_wait = true);
        public abstract void SetListenFun(String CanAddress, CanFunCodeEnum UpType, CanListenFun ListenFun);

        public virtual Boolean SetCoil(String CanAddress, Boolean value, bool is_wait = true)
        {
            ClsCoilMap(CanAddress);
            return SetCoil(CanAddress, value, 0b111, is_wait);
        }
        public virtual Boolean SetByte(String StartCanAddress, byte value)
        {
            ClsRegisterMap(StartCanAddress);
            return SetByte(StartCanAddress,value, 0b111);
        }
        public virtual Boolean SetCoils(String StartCanAddress,bool is_wait, params Boolean[] values)
        {
            ClsRegisterMap(StartCanAddress);
            return SetCoils(StartCanAddress, 0b111, is_wait);
        }
        public virtual Boolean SetBytes(String StartCanAddress, bool is_wait, params byte[] values)
        {
            ClsRegisterMap(StartCanAddress);
            return SetBytes(StartCanAddress, 0b111, is_wait, values);
        }
        public virtual Boolean SetCoilOn(String CanAddress, bool is_wait = true)
        {
            return SetCoil(CanAddress, true, is_wait);
        }
        public virtual Boolean SetCoilOff(String CanAddress)
        {
            return SetCoil(CanAddress, false);
        }
        public virtual Boolean ReadCoil(String CanAddress)
        {
            ClsCoilMap(CanAddress);
            return ReadCoils(CanAddress, 0x01);
        }
        public virtual bool ReadRegister(String CanAddress)
        {
            ClsRegisterMap(CanAddress);
            return ReadRegister(CanAddress, 0b111);
        }
        public virtual bool SetRegister(String CanAddress, int value,bool is_wait = true)
        {
            ClsRegisterMap(CanAddress);
            return SetRegister(CanAddress, value, 0b111, is_wait);
        }
        public void ClsCoilMap(String CanAddress)
        {
            if (CanAddress == null)
            {
                ErrorSystem.WriteActError("后台设置有误!", true, false,9999);
                return;
            }
            String key = CanAddress.ToUpper();
            lock (BoolLock)
            {
                if (this.CoilMap.ContainsKey(key))
                {
                    CoilMap.Remove(key);
                }
            }
        }
        public void ClsRegisterMap(String CanAddress)
        {
            if(CanAddress!=null)
            {
                String key = CanAddress.ToUpper();
                RegisterMap.Remove(key);
            }
        }

        public void SetBool(String CanAddress, bool value)
        {
            String key = CanAddress.ToUpper();
            lock (BoolLock)
            {
                if (this.CoilMap.ContainsKey(key))
                {
                    CoilMap[key] = value;
                }
                else
                {
                    CoilMap.Add(key, value);
                }
                Console.WriteLine("set:{0}={1}", CanAddress, value);
            }
        }
        public bool GetBool(String CanAddress, bool defalutVal)
        {
            //Thread.Sleep(10);
            String key = CanAddress.ToUpper();
            lock (BoolLock)
            {
                if (CoilMap.ContainsKey(key))
                {
                    return CoilMap[key];
                }
                else
                {
                    return defalutVal;
                }
            }
        }
        public bool GetBoolBlock(String CanAddress, bool value, int timeout, bool is_check_value, out bool is_timeout)
        {
            bool ret = false;
            String key = CanAddress.ToUpper();
            int timecount = 0;
            timeout = timeout / 10;
            is_timeout = false;
            if (key.Length == 0) return ret;
            while (!CoilMap.ContainsKey(key) || (value != CoilMap[key] && is_check_value))
            {
                Thread.Sleep(5);
                timecount++;
                if (timecount > timeout)
                {
                    last_error = "错误代码:" + CanAddress + " time:" + timeout * 10 + " src:" + GetStacktRace();
                    ErrorSystem.WriteCanError(last_error);
                    //Console.WriteLine(last_error);
                    is_timeout = true;
                    return false;
                }

            }
            ret = CoilMap[key];
            return ret;
        }

        public void SetInt(String CanAddress, int value)
        {
            String key = CanAddress.ToUpper();
            lock (IntlLock)
            {
                if (this.RegisterMap.ContainsKey(key))
                {
                    RegisterMap.Remove(key);
                }
                RegisterMap.Add(key, value);
            }
        }
        public int GetIntBlock(String CanAddress, int timeout, out bool is_timeout)
        {
            int ret = 0;
            if(CanAddress==null)
            {
                ErrorSystem.WriteActError("地址为空",true,false);
                is_timeout = true;
                return ret;
            }
            String key = CanAddress.ToUpper();
            int timecount = 0;
            timeout = timeout / 10;
            is_timeout = false;
            if (key.Length == 0) return ret;
            while (!RegisterMap.ContainsKey(key))
            {
                Thread.Sleep(1);
                timecount++;
                if (timecount > timeout)
                {
                    last_error = "错误代码:" + CanAddress + " time:" + timeout * 10 + " src:" + GetStacktRace();
                    ErrorSystem.WriteCanError(last_error);
                    is_timeout = true;
                    return 0;
                }
            }
            ret = RegisterMap[key];
            return ret;
        }
        public int GetInt(String CanAddress, int defalutVal)
        {
            if (CanAddress == null) return defalutVal;
            String key = CanAddress.ToUpper();
            lock (IntlLock)
            {
                if (RegisterMap.ContainsKey(key))
                {
                    return RegisterMap[key];
                }
                else
                {
                    return defalutVal;
                }
            }
        }
        /// <summary>
        /// 初始化电机
        /// </summary>
        /// <param name="Motor"></param>
        /// <returns></returns>
        public bool InitMotor(Electromotor Motor, bool OnlyStart = false)
        {
            Constants.UserAbort = false;
            var result = StopMotor(Motor);
            result = result && SetCoilOff(Motor.DoneCoil.Addr);
            result = result && SetCoilOn(Motor.InitCoil.Addr);
            Motor.CurrentDistance = 0m;
            Motor.IsBack = true;
            Motor.IsStarted = false;
            return result;
        }
        /// <summary>
        /// 设置电机启动线圈ON
        /// </summary>
        /// <param name="Motor"></param>
        /// <returns></returns>
        public bool StartMotor(Electromotor Motor,bool is_wait=true)
        {
            //加速测试代码
            var result = true;
            if (Motor.IsStarted == false)
            result = SetCoilOn(Motor.StartCoil.Addr, is_wait);
            Motor.IsStarted = result;
            return result;
        }
        /// <summary>
        /// 停止电机
        /// </summary>
        /// <param name="Motor"></param>
        /// <param name="ForceStope"></param>
        /// <returns></returns>
        public bool StopMotor(Electromotor Motor)
        {
            var result = SetCoilOff(Motor.StartCoil.Addr);
            Motor.IsStarted = false;
            return result;
        }
        /// <summary>
        /// 移到指定位置,移动前关闭反馈完成信号
        /// </summary>
        /// <param name="Motor">马达</param>
        /// <param name="Distance">运动位置</param>
        /// <param name="mask">掩码，如果不为null,要在调用前关闭程序中完成信号</param>
        /// <param name="OnlyStart">是否仅仅起动，不等待完成</param>
        /// <param name="Start">如果电机启动线圈没有打开，是否马上起动电机</param>
        /// <param name="gap">电机反向运动时的间隙</param>
        /// <returns></returns>
        public bool MoveMotor(Electromotor Motor, decimal? Distance, bool OnlyStart = false, bool Start = true, decimal gap = 0m,bool MinValueIsZero=true,bool IsSignal = true)
        {
            Constants.PauseResetEvent.WaitOne();
            if (Constants.UserAbort)
            {
                return false;
            }
            if (!Distance.HasValue) return false;
            if (MinValueIsZero &&Distance.Value < 0) Distance = 0;
            var realdis = Distance.Value + gap;
            Motor.CurrentDistance = realdis;
            int distance = Convert.ToInt32(Distance.Value * Convert.ToDecimal(Motor.Factor.SetValue));
            bool result = true;
            //信号 
            if (IsSignal)
            {
                //加速测试代码
                if(Start)
                {
                    result = SetCoilOff(Motor.DoneCoil.Addr);
                    result = result && StartMotor(Motor);
                }
                result = result&&SetRegister(Motor.Distance.Addr, distance);
            }
            return result;
        }
        public bool MoveMotors(Electromotor[] Motor, int []Distance, bool OnlyStart = false, bool Start = true, decimal gap = 0m, bool MinValueIsZero = true, bool IsSignal = true,int []SleepTime=null)
        {
            Constants.PauseResetEvent.WaitOne();
            if (Constants.UserAbort)
            {
                return false;
            }
            bool result = true;
            int[] distance = new int[Motor.Length];
            for (int i=0;i< Motor.Length;i++)
            {
                if (Distance==null) return false;
                if (MinValueIsZero && Distance[i] < 0) Distance[i] = 0;
                var realdis = Distance[i] + gap;
                Motor[i].CurrentDistance = realdis;
                distance[i] = Convert.ToInt32(Distance[i] * Convert.ToDecimal(Motor[i].Factor.SetValue));
                if (Start)
                {
                    result = SetCoilOff(Motor[i].DoneCoil.Addr);
                    result = result && StartMotor(Motor[i]);
                }
            }
            //信号 
            if (IsSignal)
            {
                //加速测试代码
                for (int i = 0; i < Motor.Length; i++)
                {
                    result = result && SetRegister(Motor[i].Distance.Addr, distance[i],false);
                    Thread.Sleep(10);
                }
            }
            return result;
        }
        /// <summary>
        /// 移到指定位置,移动前关闭反馈完成信号
        /// </summary>
        /// <param name="Motor">马达</param>
        /// <param name="Distance">运动位置</param>
        /// <param name="mask">掩码，如果不为null,要在调用前关闭程序中完成信号</param>
        /// <param name="OnlyStart">是否仅仅起动，不等待完成</param>
        /// <param name="Start">如果电机启动线圈没有打开，是否马上起动电机</param>
        /// <param name="gap">电机反向运动时的间隙</param>
        /// <returns></returns>
        public bool MoveMotorBegin(Electromotor Motor)
        {
            var result = SetCoilOff(Motor.DoneCoil.Addr);
            result = result&&StartMotor(Motor);
            return result;
        }
        /// <summary>
        /// 移到指定位置,移动前关闭反馈完成信号
        /// </summary>
        /// <param name="Motor">马达</param>
        /// <param name="Distance">运动位置</param>
        /// <param name="mask">掩码，如果不为null,要在调用前关闭程序中完成信号</param>
        /// <param name="OnlyStart">是否仅仅起动，不等待完成</param>
        /// <param name="Start">如果电机启动线圈没有打开，是否马上起动电机</param>
        /// <param name="gap">电机反向运动时的间隙</param>
        /// <returns></returns>
        public bool MoveMotorEnd(Electromotor Motor, decimal? Distance,decimal gap = 0m)
        {
            if (!Distance.HasValue) return false;
            bool result = true;
            var realdis = Distance.Value + gap;
            Motor.CurrentDistance = realdis;
            int distance = Convert.ToInt32(Distance.Value * Convert.ToDecimal(Motor.Factor.SetValue));
            result = this.SetRegister(Motor.Distance.Addr, distance);
            return result;
        }
        public bool MoveMotors(Electromotor[] motors, decimal[] Distances, decimal[] gap, bool OnlyStart,bool IsInjectorY = false, bool IsInitInjectorY = false)
        {
            Constants.PauseResetEvent.WaitOne();
            if (Constants.UserAbort)
            {
                return false;
            }
            var len = motors.Length;
            bool result = true;

            for (byte b = 0; b < len; b++)
            {
                result = result && MoveMotor(motors[b], Distances[b],OnlyStart, false, gap == null ? 0m : gap[b],true, IsInjectorY==false);
            }
            return result;
        }

        public void Set2PLC<T>(PLCParameter<T> pLCParameter, Type targetType, float factor)
        {
            if (String.IsNullOrEmpty(pLCParameter.Addr)) return;
            String TypeName = targetType == null ? typeof(T).Name : targetType.Name;
            switch (TypeName)
            {
                case "Boolean":
                    {
                        SetCoil(pLCParameter.Addr, (bool)(Object)pLCParameter.SetValue);
                        break;
                    }
                case "Single":
                    {
                        SetRegister(pLCParameter.Addr, Convert.ToInt32((float)(Object)pLCParameter.SetValue * factor));
                        break;
                    }
                case "Int32":
                    {
                        SetRegister(pLCParameter.Addr, Convert.ToInt32(Convert.ToSingle((Object)pLCParameter.SetValue) * factor));
                        break;
                    }
                case "Int16":
                    {
                        var val = Convert.ToInt16(Convert.ToSingle((Object)pLCParameter.SetValue) * factor);
                        SetRegister(pLCParameter.Addr, val);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public void Read2PLC<T>(PLCParameter<T> pLCParameter)
        {
            Read2PLC<T>(pLCParameter, null, 1);
        }

        public void Read2PLC<T>(PLCParameter<T> pLCParameter, Type targetType, float factor)
        {
            if (String.IsNullOrEmpty(pLCParameter.Addr)) return;
            String TypeName = targetType == null ? typeof(T).Name : targetType.Name;
            bool is_time_out = false;
            switch (TypeName)
            {
                case "Boolean":
                    {
                        ReadCoil(pLCParameter.Addr);
                        var value = GetBoolBlock(pLCParameter.Addr, false, 1000, false, out is_time_out);
                        pLCParameter.SetValue = Unity.AsType<T>(value);
                        break;
                    }
                case "Single":
                    {
                        ReadRegister(pLCParameter.Addr);
                        var value = GetIntBlock(pLCParameter.Addr,1000,out is_time_out)/factor;
                        pLCParameter.SetValue = Unity.AsType<T>(value);
                        break;
                    }
                case "Int32":
                    {
                        ReadRegister(pLCParameter.Addr);
                        var value = GetIntBlock(pLCParameter.Addr, 1000, out is_time_out) / factor;
                        pLCParameter.SetValue = Unity.AsType<T>(value);
                        break;
                    }
                case "Int16":
                    {
                        ReadRegister(pLCParameter.Addr);
                        var value = GetIntBlock(pLCParameter.Addr, 1000, out is_time_out) / factor;
                        pLCParameter.SetValue = Unity.AsType<T>(value);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public void Set2PLC<T>(PLCParameter<T> pLCParameter)
        {
            Set2PLC<T>(pLCParameter, null, 1);
        }

        public void SetMotor(Electromotor Motor)
        {
            Set2PLC(Motor.Maximum);
            Thread.Sleep(50);
            Set2PLC(Motor.Speed);
            Thread.Sleep(50);
            Set2PLC(Motor.InitSpeed);
            Thread.Sleep(50);
            Set2PLC(Motor.InitDistance);
            Thread.Sleep(50);
            Set2PLC(Motor.InitTime);
            Thread.Sleep(50);
            Set2PLC(Motor.DistanceTime);
            Thread.Sleep(50);
            Set2PLC(Motor.PressureTime);
            Thread.Sleep(50);
            Set2PLC(Motor.PressureSwitch);
            Thread.Sleep(50);
            Set2PLC(Motor.StopCoil);
            Thread.Sleep(50);
            Set2PLC(Motor.StartAfter);
            Thread.Sleep(50);
        }

        public void ReadMotor(Electromotor Motor)
        {
            Read2PLC(Motor.Maximum);
            Thread.Sleep(50);
            Read2PLC(Motor.Speed);
            Thread.Sleep(50);
            Read2PLC(Motor.InitSpeed);
            Thread.Sleep(50);
            Read2PLC(Motor.InitDistance);
            Thread.Sleep(50);
            Read2PLC(Motor.InitTime);
            Thread.Sleep(50);
            Read2PLC(Motor.DistanceTime);
            Thread.Sleep(50);
            Read2PLC(Motor.PressureTime);
            Thread.Sleep(50);
            Read2PLC(Motor.PressureSwitch);
            Thread.Sleep(50);
            Read2PLC(Motor.StopCoil);
            Thread.Sleep(50);
            Read2PLC(Motor.StartAfter);
            Thread.Sleep(50);
        }

        public Boolean[] IntToBools(short coilCount, int? data, bool NeedNot = false)
        {
            if (data == null) return null;
            int val = data.Value;
            if (NeedNot)
            {
                val = ~val;
            }
            Boolean[] result = new Boolean[coilCount];
            for (short i = 0; i < coilCount; i++)
            {
                int curVal = val >> i;
                result[i] = (curVal & 0x01) == 0x01;
            }
            return result;
        }
    }
}
