using SKABO.Common;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.Communication;
using SKABO.Common.Utils;
using SKABO.Hardware.Core;
using SKABO.Hardware.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SKABO.ActionEngine;
using SKABO.MAI.ErrorSystem;

namespace SKABO.Hardware.RunBJ
{
    public class OtherPartDevice : AbstractCanDevice
    {
        public System.Threading.ManualResetEvent ScanRestEvent = new ManualResetEvent(true);
        public OtherPartDevice(AbstractCanComm CanComm, OtherPart OP)
        {
            this.CanComm = CanComm;
            this.OP = OP;
            this.scanFlag = true;
        }
        public bool scanFlag = false;
        public int ledcolor = 0;
        public bool is_handstop = false;
        private Boolean[] _RunResult;
        public Boolean[] RunResult
        {
            get
            {
                return _RunResult;
            }
            set
            {
                _RunResult = value;
            }
        }
        public OtherPart OP { get; set; }
        public override void LoadPLCValue()
        {
            CanComm.ReadMotor(OP.ScanMotor);
        }

        public override void Update2Plc()
        {
            CanComm.SetMotor(OP.ScanMotor);
        }

        public byte[] ConvertFatekAddrToModbusAddr(String FatekAddr)
        {
            return new Byte[] { 0x00, Byte.Parse(FatekAddr) };
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

        public void ScanRunResult()
        {
            this.CanComm.ReadRegister(OP.SampleRackCoils[0].Addr);
            var retval = this.CanComm.GetInt(OP.SampleRackCoils[0].Addr,0x00);
            RunResult = retval < 0 ? null : this.CanComm.IntToBools(6, retval, true);
            while (scanFlag)
            {
                this.ScanRestEvent.WaitOne();
                this.CanComm.ClsRegisterMap(OP.SampleRackCoils[0].Addr);
                bool is_timeout = false;
                retval = this.CanComm.GetIntBlock(OP.SampleRackCoils[0].Addr,1000,out is_timeout);
                bool[] result = retval < 0 ? null : this.CanComm.IntToBools(6, retval, true);
                if (result != null) result = result.Reverse().ToArray();
                if (RunResult != null && result != null)
                {
                    IList<byte> idList = new List<byte>();
                    for (byte i = 0; i < 6; i++)
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
        /// <summary>
        /// 开关报警
        /// </summary>
        /// <param name="IsOn">true:响警 false:关警</param>
        public bool Alarm(bool IsOn)
        {
            var result = true;
            if (IsOn)
            {
                result = CanComm.SetRegister(OP.LightAlarmCoil.Addr, 0xffffff);
                 //= result&& this.PcbComm.SetRegister(OP.LightAlarmCoil.Addr,ByteUtil.BytesToInt16(0xFF,0xFF));
            }
            else
            {
                result = CanComm.SetRegister(OP.LightAlarmCoil.Addr, 0x000000);
               // result = result && this.PcbComm.SetRegister(OP.LightAlarmCoil.Addr,(short)0);
            }
            //result &&this.Comm.SetCoil(IsOn, OP.VoiceAlarmCoil.Addr);
            return result;
        }
        public void MakeColor(int color)
        {
            bool[] leds = { true, true, true, true, true, true, true };
            for (int i = 0; i < 7; i++)
            {
                if (leds[i])
                {
                    int mask1 = color << i * 2;
                    int mask2 = (0x03 << i * 2) ^ 0xffff;
                    int mask3 = mask1 | mask2;
                    int mask4 = (0x03 << i * 2);
                    ledcolor = ledcolor | mask4;
                    ledcolor = ledcolor & mask3;
                }
            }
        }
        //灯光光
        public void LedGreen()
        {
            MakeColor(0x00);
            var canComm = IoC.Get<AbstractCanComm>();
            canComm.SetRegister(OP.LightAlarmCoil.Addr, ledcolor, false);
        }
        public void LedBule()
        {
            MakeColor(0x01);
            var canComm = IoC.Get<AbstractCanComm>();
            canComm.SetRegister(OP.LightAlarmCoil.Addr, ledcolor, false);
        }
        public void LedRed()
        {
            MakeColor(0x02);
            var canComm = IoC.Get<AbstractCanComm>();
            canComm.SetRegister(OP.LightAlarmCoil.Addr, ledcolor, false);
        }
        public void LedRedBlink()
        {
            MakeColor(0x03);
            var canComm = IoC.Get<AbstractCanComm>();
            canComm.SetRegister(OP.LightAlarmCoil.Addr, ledcolor,false);
        }
        /// <summary>
        /// 开关灯光
        /// </summary>
        /// <param name="IsOn">true:开灯 false:关灯</param>
        public bool Light(bool IsOn)
        {
            return this.CanComm.SetCoil(OP.LightCoil.Addr, IsOn);
        }
        /// <summary>
        /// 相机背光灯
        /// </summary>
        /// <param name="IsOn"></param>
        /// <returns></returns>
        public bool CameraLight(bool IsOn)
        {
            var ret = this.CanComm.SetCoil(OP.CameraFLightCoil.Addr, IsOn);
            return ret&&this.CanComm.SetCoil(OP.CameraLightCoil.Addr, IsOn);
        }
        public bool GetCameraLight()
        {
            bool is_timeout = false;
            CanComm.ReadCoil(OP.CameraLightCoil.Addr);
            var ret  = CanComm.GetBoolBlock(OP.CameraLightCoil.Addr, true, 3000, false, out is_timeout);
            return ret&& is_timeout==false;
        }
        public bool HandStop(bool IsOn)
        {
            if(is_handstop!= IsOn)
            {
                is_handstop = IsOn;
                return this.CanComm.SetCoil(OP.HandStopCoil.Addr, IsOn);
            }
            return true;
        }
        /// <summary>
        /// 移动扫描器
        /// </summary>
        /// <param name="index">从1开始</param>
        /// <returns></returns>
        public bool MoveScaner(byte index)
        {
            String Key = typeof(T_BJ_SampleRack).Name;
            if (Constants.BJDict.ContainsKey(Key))
            {
                var bj=Constants.BJDict[Key].Where(item => (item as T_BJ_SampleRack).Index == index).FirstOrDefault();
                if(bj is T_BJ_SampleRack bjs)
                {
                    return MoveScaner(bjs.ReaderX);
                }
            }
            return false;
        }
        /// <summary>
        /// 移动扫码器电机，不等待反馈信号
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool MoveX(decimal Distance, bool OnlyStart = false)
        {
            var result = false;
            result = CanComm.MoveMotor(OP.ScanMotor, Distance,OnlyStart);
            return result;
        }

        public bool InitX(bool OnlyStart = false)
        {
            return CanComm.InitMotor(OP.ScanMotor, OnlyStart);
        }
        /// <summary>
        /// 移动扫码器电机，不等待反馈信号
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool MoveScaner(decimal distance)
        {
            return CanComm.MoveMotor(this.OP.ScanMotor,distance,true);
        }
        public bool InitScanerMotor(bool OnlyStart=false)
        {
            return CanComm.InitMotor(this.OP.ScanMotor,OnlyStart);
        }
        /// <summary>
        /// 初始化所有设备
        /// </summary>
        /// <returns></returns>
        public ActionBase InitAllDevice()
        {
            var devices = new ValueTuple<AbstractCanDevice, String>[] {(this,"样本扫码器载架初始化失败")
                ,(IoC.Get<PiercerDevice>(),"打孔器初始化失败")
                ,(IoC.Get<CentrifugeMrg>(),"离心管理器初始化失败")
                //,(IoC.Get<GelWarehouseDevice>(),"卡仓初始化失败")
                ,(IoC.Get<CouveuseMixerDevice>(),"孵育或混匀初始化失败")
                //,(IoC.Get<MachineHandDevice>(),"机械手初始化失败")
                //,(IoC.Get<InjectorDevice>(),"加样器初始化失败")
            };
            var seq = Sequence.create();
            var spa = Spawn.create();
            spa.AddAction(InitXyz.create(this, 10000, true, false, false));
            foreach (var device in devices)
            {
                var act = device.Item1.InitAll();
                if (act != null) spa.AddAction(act);
            }
            var seq_hand_ware_inj = Sequence.create();
            var act_inj = IoC.Get<InjectorDevice>().InitAll();
            var act_hand = IoC.Get<MachineHandDevice>().InitAll();
            var act_ware = IoC.Get<GelWarehouseDevice>().InitAll();
            seq_hand_ware_inj.AddAction(act_inj);
            seq_hand_ware_inj.AddAction(act_hand);
            seq_hand_ware_inj.AddAction(act_ware);
            spa.AddAction(seq_hand_ware_inj);
            seq.AddAction(Sequence.create(spa, SkCallBackFun.create((ActionBase acttem) =>
            {
                if (acttem.istimeout == false)
                {
                    ErrorSystem.WriteActError("开机成功!", true, false);
                }
                return true;
            })));
            CameraLight(false);
            LedGreen();
            return seq;
        }
        public override ActionBase InitAll()
        {
            return null;
        }
        private Func<Boolean, Boolean> _AlarmFunc;
        public Func<Boolean, Boolean> AlarmFun
        {
            get
            {
                if (_AlarmFunc == null)
                {
                    var method = this.GetType().GetMethod("Alarm");
                    _AlarmFunc = (Func<Boolean, Boolean>)method.CreateDelegate(typeof(Func<Boolean, Boolean>), this);
                }
                return _AlarmFunc;
            }
        }
        
    }
}
