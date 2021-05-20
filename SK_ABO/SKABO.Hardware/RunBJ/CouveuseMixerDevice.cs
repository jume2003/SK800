using SKABO.ActionEngine;
using SKABO.Common.Models.Communication;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Hardware.Core;
using SKABO.Hardware.Model;
using SKABO.MAI.ErrorSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Hardware.RunBJ
{
    public class CouveuseMixerDevice : AbstractCanDevice
    {
        public CouveuseMixerDevice(AbstractCanComm CanComm, CouveuseMixer CouMixer)
        {
            this.CanComm = CanComm;
            this.CouMixer = CouMixer;
        }
        public CouveuseMixer CouMixer { get; set; }
        public override void LoadPLCValue()
        {
            CanComm.Read2PLC(this.CouMixer.Mixer.Speed);
            CanComm.Read2PLC(this.CouMixer.Mixer.TimeSpan);
            foreach (var cou in this.CouMixer.Couveuses)
            {
                LoadCouveuse(cou);
            }
        }
        private void LoadCouveuse(Couveuse couveuse)
        {
            CanComm.Read2PLC<float>(couveuse.TempCompensate);
            CanComm.Read2PLC<float>(couveuse.SetupTemp);
        }
        private void SetCouveuse(Couveuse couveuse)
        {
            CanComm.Set2PLC<float>(couveuse.TempCompensate);
            CanComm.Set2PLC<float>(couveuse.SetupTemp);
        }

        public override void Update2Plc()
        {
            CanComm.SetMotor(CouMixer.Mixer);
            foreach (var cou in this.CouMixer.Couveuses)
            {
                SetCouveuse(cou);
            }
        }

        public bool InitMixer(bool OnlyStart=false)
        {
            return CanComm.InitMotor(CouMixer.Mixer, OnlyStart);
        }

        public bool StartHot(byte index)
        {
            var Couv = this.CouMixer.Couveuses[index];
            return CanComm.SetCoilOn(Couv.HotSwitchCoil.Addr);
        }

        public bool StopHot(byte index)
        {
            var Couv = this.CouMixer.Couveuses[index];
            return CanComm.SetCoilOff(Couv.HotSwitchCoil.Addr);
        }

        public bool SetTemp(byte index, float temp)
        {
            int temp_tem = (int)(temp * 10);
            var Couv = this.CouMixer.Couveuses[index];
            return CanComm.SetRegister(Couv.SetupTemp.Addr, temp_tem,true);
        }

        public int ReadTemp(byte index)
        {
            bool is_time_out = false;
            var Couv = this.CouMixer.Couveuses[index];
            CanComm.ReadRegister(Couv.SetupTemp.Addr);
            int temp = CanComm.GetIntBlock(Couv.SetupTemp.Addr, 1000, out is_time_out);
            return is_time_out ? 0 : temp;
        }

        /// <summary>
        /// 混匀器电机，不等待反馈信号
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool MoveZ(decimal Distance, bool OnlyStart = false)
        {
            var result = false;
            result = CanComm.MoveMotor(CouMixer.Mixer, Distance, OnlyStart);
            return result;
        }
        public bool Init(bool onlyStart = false)
        {
            return CanComm.InitMotor(CouMixer.Mixer, onlyStart);
        }
        public override ActionBase InitAll()
        {
            for (int i = 0; i < CouMixer.Couveuses.Length; i++)
            {
                StartHot((byte)i);
            }
            var seque = Sequence.create();
            var reseque = Sequence.create();
            //seque.AddAction(InitXyz.create(this, 3000,false,false,true));
            reseque.AddAction(MoveTo.create(this,3000,-1,-1, 2000));
            reseque.AddAction(SKSleep.create(500));
            reseque.AddAction(MoveTo.create(this, 3000, -1, -1, 0));
            reseque.AddAction(SKSleep.create(15000));
            var rep_act = Repeat.create(this, reseque, 99999);
            rep_act.runAction(this);
            return seque;
        }
    }
}
