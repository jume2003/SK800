using SKABO.Common;
using SKABO.Common.Enums;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.Communication;
using SKABO.Common.Utils;
using SKABO.Hardware.Core;
using SKABO.Hardware.Model;
using SKABO.Hardware.Scaner;
using SKABO.Ihardware.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SKABO.MAI.ErrorSystem;
using SKABO.ActionEngine;

namespace SKABO.Hardware.RunBJ
{ 
    public class MachineHandDevice : AbstractCanDevice
    {
        
        //public int SlowSpeed = 2000;
        public bool Z_AT_ZERO = false;
        //public static ManualResetEvent HandReset { get; set; } = new ManualResetEvent(true);
        public int TaskId { get; set; }
        public HandStatusEnum StatusEnum { get;  set; }
        private ScanDevice _scanDevice;
        private ScanDevice scanDevice
        {
            get
            {
                return IoC.Get<ScanDevice>();
            }
        }
        public MachineHandDevice(AbstractCanComm CanComm, MachineHand Hand)
        {
            this.CanComm = CanComm;
            this.Hand = Hand;
        }
        private CentrifugeDevice _centDevice;
        private CentrifugeDevice centDevice
        {
            get
            {
                if (_centDevice == null)
                {
                    _centDevice = IoC.Get<CentrifugeDevice>();
                }
                return _centDevice;
            }
        }
        private GelWarehouseDevice _warehouseDevice;
        private GelWarehouseDevice warehouseDevice
        {
            get
            {
                if (_warehouseDevice == null)
                {
                    _warehouseDevice = IoC.Get<GelWarehouseDevice>();
                }
                return _warehouseDevice;
            }
        }
        public MachineHand Hand { get; set; }
        public override void LoadPLCValue()
        {
            CanComm.ReadMotor(Hand.XMotor);
            CanComm.ReadMotor(Hand.YMotor);
            CanComm.ReadMotor(Hand.ZMotor);
            CanComm.Read2PLC(Hand.InitTime);
            Thread.Sleep(50);
            CanComm.Read2PLC(Hand.DistanceTime);
            Thread.Sleep(50);
        }
        public override void Update2Plc()
        {
            CanComm.SetMotor(Hand.XMotor);
            CanComm.SetMotor(Hand.YMotor);
            CanComm.SetMotor(Hand.ZMotor);
            CanComm.Set2PLC(Hand.InitTime);
            Thread.Sleep(50);
            CanComm.Set2PLC(Hand.DistanceTime);
            Thread.Sleep(50);

        }

        public bool InitX(bool OnlyStart=false)
        {
            return CanComm.InitMotor(Hand.XMotor,OnlyStart);
        }
        public bool InitY(bool OnlyStart = false)
        {
            return CanComm.InitMotor(Hand.YMotor,OnlyStart);
        }
        public bool InitZ(bool OnlyStart = false)
        {
            this.Z_AT_ZERO = true;
            return CanComm.InitMotor(Hand.ZMotor,OnlyStart);
        }
        public bool InitHandTongs(bool OnlyStart = false)
        {
            bool is_time_out = false;
            var result = this.CanComm.SetCoilOn(this.Hand.HandInitCoil.Addr);
            if (OnlyStart) CanComm.GetBoolBlock(this.Hand.HandInitCoil.Addr,false,2000,true,out is_time_out);
            Hand.IsOpen = true;
            return result;
        }
        public override ActionBase InitAll()
        {
            InitHandTongs();
            var act = Sequence.create(InitXyz.create(this,30000, false, false, true));
            act.AddAction(InitXyz.create(this,30000, true, true, false));
            var device = new ActionDevice(this);
            int x = 0;
            int y = 0;
            int z = 0;
            device.GetRealX(ref x);
            device.GetRealY(ref y);
            device.GetRealZ(ref z);
            Hand.XMotor.CurrentDistance = x;
            Hand.YMotor.CurrentDistance = y;
            Hand.ZMotor.CurrentDistance = z;
            return act;
        }
        public bool thread_start = false;
        public bool MoveX(decimal Distance,bool OnlyStart = false)
        {
            var result = false;
            result = CanComm.MoveMotor(Hand.XMotor, Distance,OnlyStart);
            return result;
        }
        private void SetHightSpeed(Object obj)
        {
            if(obj is Common.Models.Communication.Unit.Electromotor motor)
            {
                CanComm.SetRegister(motor.Speed.Addr, motor.Speed.SetValue);
            }
            //Common.Models.Communication.Unit.Electromotor
        }
        public bool MoveY(decimal Distance, bool OnlyStart = false)
        {
            var result = CanComm.MoveMotor(Hand.YMotor, Distance,OnlyStart);
            return result;
        }
        public bool MoveZ(decimal Distance, bool OnlyStart = false)
        {
            var result = CanComm.MoveMotor(Hand.ZMotor, Distance,OnlyStart);
            return result;
        }
        public bool SwitchHand(bool IsOpen)
        {
            if(Hand.IsOpen!=IsOpen)
            {
                var result = CanComm.SetCoil(Hand.HandDonedCoil.Addr, false);
                result = result && CanComm.SetCoil(Hand.HandStartCoil.Addr, true);
                result = result && CanComm.SetCoil(this.Hand.HandCoil.Addr, !IsOpen);
                Hand.IsOpen = IsOpen;
                return result;
            }
            return true;
        }
        /// <summary>
        /// 是否夹到卡
        /// </summary>
        /// <returns>False ,无卡， True,有卡 ，常闭点</returns>
        public bool CheckGel()
        {
            bool ret = false;
            bool is_timeout = true;
            int count = 0;
            while(is_timeout)
            {
                if (count > 5) break;
                this.CanComm.ReadCoil(this.Hand.ExistCoil.Addr);
                ret = this.CanComm.GetBoolBlock(this.Hand.ExistCoil.Addr, false, 500, false, out is_timeout);
                if(is_timeout)
                {
                    is_timeout = true;
                }
                count++;
            }
            return ret;
        }
    }
}
