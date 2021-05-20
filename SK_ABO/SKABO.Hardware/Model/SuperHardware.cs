using SKABO.Common;
using SKABO.Common.Models.Communication;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Common.Utils;
using SKABO.Hardware.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Hardware.Model
{
    public abstract class SuperHardware
    {
        public AbstractComm Comm { get; set; }
        protected AbstractComm PcbComm { get; set; }
        
        public abstract void LoadPLCValue();
        public abstract void Update2Plc();
        public abstract bool InitAll();
        public bool ChangeSpeed(Electromotor motor,int speed,AbstractComm Comm)
        {
            var res= Comm.SetRegister(motor.Speed.Addr, speed);
            return res;
        }
        
        public static void LoadPLC<T>(PLCParameter<T> pLCParameter, AbstractComm Comm)
        {
            LoadPLC<T>(pLCParameter, null,1, Comm);
        }
        public static void LoadPLC<T>(PLCParameter<T> pLCParameter, Type pclType,float factor, AbstractComm Comm) 
        {
            if (String.IsNullOrEmpty(pLCParameter.Addr)) return;
            String TypeName = pclType == null ? typeof(T).Name : pclType.Name;
            Object curVal = null;
            switch (TypeName)
            {
                case "Boolean":
                    {
                        curVal = (Object) Comm.ReadCoil(1, pLCParameter.Addr)[0];
                        break;
                    }
                case "Single":
                    {
                        curVal = (Single)(Comm.ReadFloat( pLCParameter.Addr)/ factor);
                        break;
                    }
                case "Int32":
                    {
                        curVal = (Comm.ReadInt32(pLCParameter.Addr) / factor);
                        break;
                    }
                case "Int16":
                    {
                        curVal = (Comm.ReadInt16(pLCParameter.Addr)/factor);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            switch (typeof(T).Name)
            {
                case "Boolean":
                case "Single":
                    {
                        pLCParameter.CurrentValue = (T)curVal;
                        break;
                    }
                case "Int32":
                    {
                        pLCParameter.CurrentValue = (T)(Object)Convert.ToInt32(curVal);
                        break;
                    }
                case "Int16":
                    {
                        pLCParameter.CurrentValue = (T)(Object)Convert.ToInt16(curVal);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            

        }
        public static void LoadMotor(Electromotor Motor, AbstractComm Comm)
        {
            LoadPLC<Boolean>(Motor.InitCoil, Comm);
            LoadPLC<Boolean>(Motor.StartCoil, Comm);
            LoadPLC<bool>(Motor.DoneCoil, Comm);
            LoadPLC<bool>(Motor.Zero, Comm);
            LoadPLC<float>(Motor.Distance, typeof(Int32), Motor.Factor.SetValue, Comm);
            //LoadPLC<float>(Motor.Factor, Comm);
            LoadPLC<float>(Motor.Maximum, Comm);
            LoadPLC<Int32>(Motor.Speed, Comm);
        }
        public static void Set2PLC<T>(PLCParameter<T> pLCParameter, AbstractComm Comm)
        {
            Set2PLC<T>(pLCParameter, null,1, Comm);
        }
        public static void Set2PLC<T>(PLCParameter<T> pLCParameter,Type targetType,float factor, AbstractComm Comm)
        {
            if (String.IsNullOrEmpty(pLCParameter.Addr)) return;
            String TypeName = targetType == null ? typeof(T).Name : targetType.Name;
            switch (TypeName)
            {
                case "Boolean":
                    {
                        Comm.SetCoil((bool)(Object)pLCParameter.SetValue, Comm.ConvertFatekAddrToModbusAddr(pLCParameter.Addr));
                        break;
                    }
                case "Single":
                    {
                        Comm.SetRegister(pLCParameter.Addr, (float)(Object)pLCParameter.SetValue* factor);
                        break;
                    }
                case "Int32":
                    {
                        Comm.SetRegister(pLCParameter.Addr, Convert.ToInt32(Convert.ToSingle((Object)pLCParameter.SetValue)*factor));
                        break;
                    }
                case "Int16":
                    {
                        var val = Convert.ToInt16(Convert.ToSingle((Object)pLCParameter.SetValue) * factor);
                        //Console.WriteLine("val=" + val);
                        Comm.SetRegister(pLCParameter.Addr, val);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
        public static void SetMotor(Electromotor Motor, AbstractComm Comm)
        {
            Set2PLC(Motor.Maximum, typeof(Int32), Motor.Factor.SetValue, Comm);
            Set2PLC(Motor.Speed, Comm);
        }
        /// <summary>
        /// 初始化电机,先归零，再初始化
        /// </summary>
        /// <param name="Motor"></param>
        /// <param name="Comm"></param>
        /// <returns></returns>
        public static bool InitMotor(Electromotor Motor, AbstractComm Comm, bool OnlyStart = false,decimal gap=0m)
        {
            Constants.UserAbort = false;
            //var result=MoveMotor(Motor, Comm, 0);
            var result = StopMotor(Motor,Comm,true);
            result = result && Comm.SetRegister(Motor.Distance.Addr,0);
            result = result && Comm.SetCoilOff(Motor.InitCoil.Addr);
            result = result && Comm.SetCoilOn(Motor.InitCoil.Addr);
            Motor.CurrentDistance = 0m;
            Motor.IsBack = true;
            Motor.IsStarted = false;
            if (!OnlyStart)
            {
                result = result && Comm.Doned(30000, false, Motor.InitCoil);
                if (Motor.ProgramZero > 0)
                {
                    result = result && MoveMotor(Motor, Comm, 0, false, true, gap);
                }

            }
            else
            {
                if (Motor.ProgramZero > 0)
                {
                    Task.Run(() =>
                      {
                           MoveMotor(Motor, Comm, 0,false,true,gap);
                      });
                }
            }
            return result;
        }
        /// <summary>
        /// 启动连续地址电机，如加样器Y轴,距离地址连续，启动地址连续，反馈信号地址连续
        /// </summary>
        /// <param name="motors"></param>
        /// <param name="Distances">为负值时，不会应用校正因子</param>
        /// <param name="gap"></param>
        /// <param name="Comm"></param>
        /// <returns></returns>
        public static bool MoveMotors(Electromotor[] motors, decimal[] Distances, decimal[] gap,AbstractComm Comm,bool OnlyStart,bool PauseScan=true,bool IsInjectorY=false, bool IsInitInjectorY = false)
        {
            Constants.PauseResetEvent.WaitOne();
            if (Constants.UserAbort)
            {
                return false;
            }
            var len = motors.Length;
            byte[] dis = new byte[len*4];
            for(byte b = 0; b < len; b++)
            {
                var realdis = motors[b].GetRealDistance(Distances[b], gap==null?0m:gap[b]);
                if (motors[b].CurrentDistance == realdis)
                {
                    return true;
                }
                else
                {
                    motors[b].CurrentDistance = realdis<0?0: realdis;
                    Distances[b] = realdis;
                    var val=Convert.ToInt32(realdis < 0? realdis:(realdis * Convert.ToDecimal(motors[b].Factor.SetValue)));
                    var bval = ByteUtil.Int4ToBytes(val);
                    Array.Copy(bval, 0, dis, 4 * b, 4);
                }
            }
            var result = true;
            if(PauseScan)Comm.ScanRestEvent.Reset();
            //关闭完成信号
            result = result && Comm.SetBatchCoil(motors[0].DoneCoil.Addr,(short)len,0x00);

            result = result && Comm.SetBatchRegister(Comm.ConvertFatekAddrToModbusAddr(motors[0].Distance.Addr.Substring(1)),(short)(len*2), dis);
            if (IsInjectorY)
            {
                result = result && Comm.SetCoilOn("M99");
                if (IsInitInjectorY)
                {
                    for(byte b=0;b< Distances.Length; b++)
                    {
                        if (Distances[b] == 0)
                        {
                            Comm.SetCoilOn(motors[b].DoneCoil.Addr);//开启反馈信号
                        }
                    }
                }
            }
            else
            {
                result = result && StartMotor(motors, Comm, true, false);
            }
            if (PauseScan) Comm.ScanRestEvent.Set();
            if (!OnlyStart)
                result = result && Comm.Doned(30000, true, motors.Select(item=>item.DoneCoil.Addr).ToArray());
            return result;
        }
        /// <summary>
        /// 移到指定位置,移动前关闭反馈完成信号
        /// </summary>
        /// <param name="Motor"></param>
        /// <param name="Comm"></param>
        /// <returns></returns>
        public static bool MoveMotor(Electromotor Motor, AbstractComm Comm, decimal? Distance, bool OnlyStart = false, bool Start = true, decimal gap =0m)
        {
            Constants.PauseResetEvent.WaitOne();
            if (Constants.UserAbort)
            {
                return false;
            }
            if (!Distance.HasValue) return false;
            if (Distance.Value < 0) Distance = 0;
            var realdis = Motor.GetRealDistance(Distance.Value, gap);
            if (Motor.CurrentDistance == realdis)
            {
                return true;
            }
            else
            {
                //关闭完成信号
                Comm.SetCoil(false, Motor.DoneCoil.Addr);
            }
            Motor.CurrentDistance = realdis;
            int distance = Convert.ToInt32(realdis * Convert.ToDecimal(Motor.Factor.SetValue));          
            var result=Comm.SetRegister(Motor.Distance.Addr, distance);
            if (Start )
            {
                StartMotor(Motor, Comm);
            }
            if (!OnlyStart)
            {
                result = Comm.Doned(30000, true, Motor.DoneCoil);
            }
            return result;
        }
        public static bool FindMachineZero(AbstractComm Comm, DoubleSpeedMotor[] Motors)
        {
            if (Motors == null || Motors.Length == 0) return false;
            var result = SwitchSpeed(Comm, Motors, Motors[0].SecondSpeed.SetValue);
            result= result && MoveMotors(Motors, Motors.Select(m => -100m).ToArray(), null, Comm, false);
            result = SwitchSpeed(Comm, Motors, Motors[0].Speed.SetValue);
            
            return result;
        }
        /// <summary>
        /// 切换连续速度地址的电机速度
        /// </summary>
        /// <param name="Motors"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        public static bool SwitchSpeed(AbstractComm Comm,Electromotor[] Motors, int speed)
        {
            var result = true;
            var len = Motors.Length;
            byte[] dis = new byte[4 * len];
            for (byte b = 0; b < len; b++)
            {
                var bval = ByteUtil.Int4ToBytes(speed);
                Array.Copy(bval, 0, dis, 4 * b, 4);
            }
            result = result && Comm.SetBatchRegister(Comm.ConvertFatekAddrToModbusAddr(Motors[0].Speed.Addr.Substring(1)), (short)(len * 2), dis);
            return result;
        }
        public static bool SwitchSpeed(AbstractComm Comm,DoubleSpeedMotor Motor, bool IsSencond)
        {
            var result = true;
            result = result && Comm.SetRegister(Motor.Speed.Addr, (IsSencond ? Motor.SecondSpeed : Motor.Speed).SetValue);
            return result;
        }
        public static bool StartMotor(Electromotor Motor, AbstractComm Comm)
        {
            var result= Motor.IsStarted || Comm.SetCoilOn(Motor.StartCoil.Addr);
            Motor.IsStarted = result;
            return result;
        }
        public static bool StopMotor(Electromotor Motor, AbstractComm Comm,bool ForceStope=false)
        {
            var result = true;
            if (ForceStope)
            {
                result= result && Comm.SetCoilOff(Motor.StartCoil.Addr);
            }
            else
            {
                result= result && (!Motor.IsStarted || Comm.SetCoilOff(Motor.StartCoil.Addr));
            }
            
            Motor.IsStarted = false;
            return result;
        }
        public static bool StartMotor(Electromotor[] Motors, AbstractComm Comm,bool IsContinuity,bool ForceStart)
        {
            if (!ForceStart && Motors.All(m => m.IsStarted))
            {
                return true;
            }
            var result = true;
            if (IsContinuity)
            {
                byte val = 0x00;
                for (int i = Motors.Length - 1; i >= 0; i--)
                {
                    Motors[i].IsStarted = true;
                    val = (byte)(val | (0x01 << i));
                }
                result =  Comm.SetBatchCoil(Motors[0].StartCoil.Addr,(short)Motors.Length, val);
            }
            else
            {
                foreach(var Motor in Motors)
                {
                    result = result && StartMotor(Motor, Comm);
                }
            }
            return result;
        }
        public static bool StopMotor(Electromotor[] Motors, AbstractComm Comm,bool IsContinuity,bool ForceStop)
        {
            if (!ForceStop && Motors.All(m => !m.IsStarted))
            {
                return true;
            }
            var result = true;
            if (IsContinuity)
            {
                byte val = 0x00;
                for (int i = Motors.Length - 1; i >= 0; i--)
                {
                    Motors[i].IsStarted = false;
                }
                result = Comm.SetBatchCoil(Motors[0].StartCoil.Addr, (short)Motors.Length, val);
            }
            else
            {
                foreach (var Motor in Motors)
                {
                    result = result && StopMotor(Motor, Comm);
                }
            }
            return result;
        }
        
    }
}
