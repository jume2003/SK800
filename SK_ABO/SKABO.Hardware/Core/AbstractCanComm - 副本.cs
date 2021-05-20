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

namespace SKABO.Hardware.Core
{
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
        private System.Threading.Timer _ReceiveTimer;
        public System.Threading.Timer ReceiveTimer
        {
            get
            {
                if (_ReceiveTimer == null)
                {
                    _ReceiveTimer = new System.Threading.Timer(Can_Rec_Tick, null, System.Threading.Timeout.Infinite, Timeout.Infinite);
                }
                return _ReceiveTimer;
            }
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
        public static string last_error = ""; //最近的错误代码
        unsafe public abstract void Can_Rec_Tick(object state);
        public bool IsOpen { get; set; }
        public IDictionary<String, bool> CoilMap = new ConcurrentDictionary<String, bool>();
        public IDictionary<String, int> RegisterMap = new ConcurrentDictionary<String, int>();
        public abstract Boolean Connect();
        public abstract Boolean Close();
        public abstract Boolean ClearBuffer();
        public abstract Boolean Send(byte targetId, byte[] data, byte mask);
        public virtual Boolean SetCoil(String CanAddress, Boolean value)
        {
            return SetCoil(CanAddress, value, 0b111);
        }
        public abstract bool SetCoil(string CanAddress, bool value, byte mask);

        public abstract bool SetByte(string CanAddress, byte value, byte mask);

        public virtual Boolean SetByte(String StartCanAddress, byte value)
        {
            return SetByte(StartCanAddress,value, 0b111);
        }

        public virtual Boolean SetCoils(String StartCanAddress, params Boolean[] values)
        {
            return SetCoils(StartCanAddress, 0b111, values);
        }
        public virtual Boolean SetBytes(String StartCanAddress, params byte[] values)
        {
            return SetBytes(StartCanAddress, 0b111, values);
        }
        public abstract Boolean SetCoils(String StartCanAddress, byte mask, params Boolean[] values);
        public abstract Boolean SetBytes(String StartCanAddress, byte mask, params byte[] values);
        public virtual Boolean SetCoilOn(String CanAddress)
        {
            return SetCoil(CanAddress, true);
        }
        public virtual Boolean SetCoilOff(String CanAddress)
        {
            return SetCoil(CanAddress, false);
        }
        /// <summary>
        /// 关闭完成信号
        /// </summary>
        /// <param name="CanAddress"></param>
        /// <returns></returns>
        public virtual Boolean SetDoneCoilOff(String CanAddress)
        {
            this.ClsCoilMap(CanAddress);
            return SetCoilOff(CanAddress);
        }
        public virtual Boolean ReadCoil(String CanAddress)
        {
            return ReadCoils(CanAddress, 0x01);
        }
       
        public abstract Boolean ReadCoils(String StartCanAddress, byte len);

        public virtual bool ReadRegister(String CanAddress)
        {
            return ReadRegister(CanAddress, 0b111);
        }
        public virtual bool ReadRegister(String CanAddress, UpLoadCallBack.UpLoad_CallBack fun)
        {
            return ReadRegister(CanAddress, 0b111,-9999, fun);
        }
        public virtual bool ReadRegister(String CanAddress,int time)
        {
            int wait_count = 1;
            time = time / 100;
            bool ret = ReadRegister(CanAddress, 0b111,-9999, (int valuei, bool valueb) => {
                wait_count = 0;
            });
            while (wait_count != 0) { Thread.Sleep(50); wait_count++; if (wait_count >= time) wait_count = 0; }
            return ret;
        }
        public virtual bool ReadRegister(String CanAddress,int val, int time)
        {
            int wait_count = 1;
            time = time / 100;
            bool ret = ReadRegister(CanAddress, 0b111,val, (int valuei, bool valueb) => {
                wait_count = 0;
            });
            while (wait_count != 0) { Thread.Sleep(50); wait_count++; if (wait_count >= time) wait_count = 0; }
            return ret;
        }
        public abstract bool ReadRegister(String CanAddress, byte mask);
        public abstract bool ReadRegister(string CanAddress, byte mask,int val, UpLoadCallBack.UpLoad_CallBack fun);

        public virtual bool SetRegister(String CanAddress, int value)
        {
            return SetRegister(CanAddress, value, 0b111);
        }
        public virtual bool SetRegister(String CanAddress, float value)
        {
            int ivalue = (int)value;
            return SetRegister(CanAddress, ivalue, 0b111);
        }
        public abstract bool SetRegister(String CanAddress, int value, byte mask);
        public bool GetBool(String CanAddress, bool defalutVal)
        {
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
        public void RemoveBool(String CanAddress)
        {
            String key = CanAddress.ToUpper();
            CoilMap.Remove(key);
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
        public void ClsCoilMap(String CanAddress)
        {
            String key = CanAddress.ToUpper();
            lock (BoolLock)
            {
                if (this.CoilMap.ContainsKey(key))
                {
                    CoilMap.Remove(key);
                }
            }
        }
        public void RemoveInt(String CanAddress)
        {
            String key = CanAddress.ToUpper();
            RegisterMap.Remove(key);
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
                Thread.Sleep(10);
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
            CoilMap.Remove(key);
            return ret;
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
                Thread.Sleep(10);
                timecount++;
                if (timecount > timeout)
                {
                    last_error = "错误代码:" + CanAddress + " time:" + timeout * 10 + " src:" + GetStacktRace();
                    ErrorSystem.WriteCanError(last_error);
                    //Console.WriteLine(last_error);
                    is_timeout = true;
                    return 0;
                }
            }
            ret = RegisterMap[key];
            RegisterMap.Remove(key);
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

        public bool Doned(int timeout, bool checkVal, params PLCParameter<Boolean>[] coils)
        {
            return Doned(timeout, checkVal, coils.Select(coil => { return coil.Addr; }).ToArray());
        }
        public bool Doned(int timeout, bool checkVal, params String[] outAddrs)
        {
            if(checkVal)
            {
                var result = false;
                bool is_timeout = false;
                foreach (string addr in outAddrs)
                {
                    result = GetBoolBlock(addr, checkVal, timeout,false,out is_timeout);
                    if (result == false) break;
                }

                return result;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// 初始化电机
        /// </summary>
        /// <param name="Motor"></param>
        /// <returns></returns>
        public bool InitMotor(Electromotor Motor, bool OnlyStart = false, decimal gap = 0m)
        {
            Constants.UserAbort = false;
            //var result=MoveMotor(Motor, Comm, 0);
            var result = StopMotor(Motor, true);
            Thread.Sleep(5);
            //result = result && SetRegister(Motor.Distance.Addr, 0);
            result = result && SetDoneCoilOff(Motor.DoneCoil.Addr);
            Thread.Sleep(5);
            result = result && SetCoilOn(Motor.InitCoil.Addr);
            Motor.CurrentDistance = 0m;
            Motor.IsBack = true;
            Motor.IsStarted = false;
            if (!OnlyStart)
            {
                result = result && Doned(30000, true, Motor.DoneCoil);
                if (Motor.ProgramZero > 0)
                {
                    result = result && MoveMotor(Motor,  0,0b111, false, true, gap);
                }

            }
            else
            {
                if (Motor.ProgramZero > 0)
                {
                    Task.Run(() =>
                    {
                        MoveMotor(Motor, 0, 0b111, false, true, gap);
                    });
                }
            }
            return result;
        }
        public bool InitMotorForNoZero(Electromotor Motor, bool OnlyStart = false)
        {
            bool result = true;

            result = result && SetRegister(Motor.InitSpeed.Addr, Motor.InitSpeed.SetValue);
            result = result&&MoveMotor(Motor, 1m * Convert.ToDecimal(Motor.InitDistance.SetValue), null, OnlyStart, true, 0, false);
            ReadRegister(Motor.DoneCoil.Addr, 0b111, 1, (int valuei, bool valueb) => {
                MoveMotor(Motor, 0, null, OnlyStart, true, 0, false);
                ReadRegister(Motor.DoneCoil.Addr, 0b111, 1, (int a, bool b) => {
                    Motor.IsBack = true;
                    result = result && StopMotor(Motor, true);
                    Motor.IsStarted = false;
                    result = result && SetRegister(Motor.Distance.Addr, 0);
                    result = result && SetRegister(Motor.RealDistance.Addr, 0);
                    result = result && SetRegister(Motor.Speed.Addr, Motor.Speed.SetValue);
                    Motor.CurrentDistance = 0m;
                });
            });

            //result =result && this.MoveMotor(Motor, 1m*Convert.ToDecimal( Motor.InitDistance.SetValue),null,OnlyStart,true,0,false);
            //result = result && this.MoveMotor(Motor, 0, null, OnlyStart, true, 0, false);
            return result;
        }
        /// <summary>
        /// 设置电机启动线圈ON
        /// </summary>
        /// <param name="Motor"></param>
        /// <returns></returns>
        public bool StartMotor(Electromotor Motor,bool ForceStart = false)
        {
            var result = SetCoilOn(Motor.StartCoil.Addr);// (Motor.IsStarted && !ForceStart) || SetCoilOn(Motor.StartCoil.Addr);
            Motor.IsStarted = result;
            return result;
        }
        /// <summary>
        /// 停止电机
        /// </summary>
        /// <param name="Motor"></param>
        /// <param name="ForceStope"></param>
        /// <returns></returns>
        public bool StopMotor(Electromotor Motor, bool ForceStop = false)
        {
            var result = true;
            if (ForceStop)
            {
                result = result && SetCoilOff(Motor.StartCoil.Addr);
            }
            else
            {
                result = result && (!Motor.IsStarted || SetCoilOff(Motor.StartCoil.Addr));
            }

            Motor.IsStarted = false;
            return result;
        }
        /// <summary>
        /// 启动多个电机，如有mask 将使用mask，没有mask一个一个启动
        /// </summary>
        /// <param name="Motors"></param>
        /// <param name="mask">Can ID掩码</param>
        /// <param name="ForceStart">true 不管电机IsStarted状态</param>
        /// <returns></returns>
        public bool StartMotor(Electromotor[] Motors, byte? mask, bool ForceStart)
        {
            if (!ForceStart && Motors.All(m => m.IsStarted))
            {
                return true;
            }
            var result = true;
            if (mask.HasValue)
            {
                result = SetCoil(Motors[0].StartCoil.Addr, true, mask.Value);
            }
            else
            {
                foreach (var Motor in Motors)
                {
                    result = result && StartMotor(Motor, ForceStart);
                }
            }
            return result;
        }
        /// <summary>
        /// 停止多个电机，如有mask 将使用mask，没有mask一个一个停止
        /// </summary>
        /// <param name="Motors"></param>
        /// <param name="mask">Can ID掩码</param>
        /// <param name="ForceStop"></param>
        /// <returns></returns>
        public bool StopMotor(Electromotor[] Motors, byte? mask, bool ForceStop)
        {
            if (!ForceStop && Motors.All(m => !m.IsStarted))
            {
                return true;
            }
            var result = true;
            if (mask.HasValue)
            {
                result = SetCoil(Motors[0].StartCoil.Addr, false, mask.Value);
            }
            else
            {
                foreach (var Motor in Motors)
                {
                    result = result && StopMotor(Motor, ForceStop);
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
        public bool MoveMotor(Electromotor Motor, decimal? Distance,byte? mask, bool OnlyStart = false, bool Start = true, decimal gap = 0m,bool MinValueIsZero=true,bool IsSignal = true)
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
            //关闭完成信号 
            if (IsSignal)
            {
                result = result&&this.SetDoneCoilOff(Motor.DoneCoil.Addr);
                result = result&&StartMotor(Motor);
                result = result&&this.SetRegister(Motor.Distance.Addr, distance);
                if (!OnlyStart)
                {
                    result = result&&Doned(10000, true, Motor.DoneCoil);
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
            //关闭完成信号 
            bool result = this.SetDoneCoilOff(Motor.DoneCoil.Addr);
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
        public bool MoveMotors(Electromotor[] motors, decimal[] Distances, decimal[] gap, 
            byte? mask, bool OnlyStart,
            bool IsInjectorY = false, bool IsInitInjectorY = false)
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
                result = result && MoveMotor(motors[b], Distances[b], null, OnlyStart, false, gap == null ? 0m : gap[b],true, IsInjectorY==false);
            }
            if (!OnlyStart&&!IsInjectorY)
                result = result && Doned(30000, true, motors.Select(item => item.DoneCoil.Addr).ToArray());
            return result;
        }

        public void Set2PCB<T>(PLCParameter<T> pLCParameter)
        {
            Set2PCB<T>(pLCParameter, null, 1);
        }
        public void SetMotor(Electromotor Motor)
        {
            Set2PCB(Motor.Maximum, typeof(Int32), Motor.Factor.SetValue);
            Thread.Sleep(50);
            Set2PCB(Motor.Speed);
            Thread.Sleep(50);
            Set2PCB(Motor.InitSpeed);
            Thread.Sleep(50);
            Set2PCB(Motor.InitDistance);
            Thread.Sleep(50);
            Set2PCB(Motor.InitTime);
            Thread.Sleep(50);
            Set2PCB(Motor.DistanceTime);
            Thread.Sleep(50);
            Set2PCB(Motor.PressureTime);
            Thread.Sleep(50);
            Set2PCB(Motor.PressureSwitch);
            Thread.Sleep(50);
            Set2PCB(Motor.StopCoil);
            Thread.Sleep(50);

        }
        public bool ChangeSpeed(Electromotor motor, int speed)
        {
            var res = SetRegister(motor.Speed.Addr, speed);
            return res;
        }

        /// <summary>
        /// 启动连续地址电机，如加样器Y轴,距离地址连续，启动地址连续，反馈信号地址连续
        /// </summary>
        /// <param name="motors"></param>
        /// <param name="Distances">为负值时，不会应用校正因子</param>
        /// <param name="gap"></param>
        /// <param name="Comm"></param>
        /// <returns></returns>

        public void Set2PCB<T>(PLCParameter<T> pLCParameter, Type targetType, float factor)
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
                        //Console.WriteLine("val=" + val);
                        SetRegister(pLCParameter.Addr, val);
                        break;
                    }
                default:
                    { 
                        break;
                    }
            }
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
