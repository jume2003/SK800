using SKABO.Common.Models.Communication.Unit;
using System;

namespace SKABO.Common.Models.Communication
{
    /// <summary>
    /// 加样器
    /// </summary>
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class Injector
    {
        //public int
        public Injector()
        {
            
        }
        public Injector(byte count)
        {
            XMotor = new Electromotor();
            TMotor = new Electromotor();
            Entercloses = new Enterclose[count];
            for(byte i=0;i<count;i++)
            {
                Entercloses[i] = new Enterclose(i);
            }
        }
        public void checkNull()
        {
            XMotor = XMotor ?? new Electromotor();
            TMotor = TMotor ?? new Electromotor();
            Entercloses = Entercloses ?? new Enterclose[Constants.EntercloseCount];
            if (Entercloses.Length > Constants.EntercloseCount)
            {
                var ets = new Enterclose[Constants.EntercloseCount];
                Array.Copy(Entercloses, ets, Constants.EntercloseCount);
                Entercloses = ets;
            }
            else if (Entercloses.Length < Constants.EntercloseCount)
            {
                var ets = new Enterclose[Constants.EntercloseCount];
                Array.Copy(Entercloses, ets, Entercloses.Length);
                Entercloses = ets;
            }
            for(byte i = 0; i < Constants.EntercloseCount; i++)
            {
                var et = Entercloses[i];
                if (et == null) et = new Enterclose(i);
                et.PumpMotor = et.PumpMotor ?? new DoubleSpeedMotor();
                et.YMotor = et.YMotor ?? new DoubleSpeedMotor();
                et.ZMotor = et.ZMotor ?? new DoubleSpeedMotor();

                if (et.ZMotor.SecondSpeed == null)
                    et.ZMotor.SecondSpeed = new PLCParameter<int>();
                if (et.PumpMotor.SecondSpeed == null)
                    et.PumpMotor.SecondSpeed = new PLCParameter<int>();
                if (et.YMotor.SecondSpeed == null)
                    et.YMotor.SecondSpeed = new PLCParameter<int>();

                if (et.ZMotor.DownSpeed == null)
                    et.ZMotor.DownSpeed = new PLCParameter<int>();
                if (et.PumpMotor.DownSpeed == null)
                    et.PumpMotor.DownSpeed = new PLCParameter<int>();
                if (et.YMotor.DownSpeed == null)
                    et.YMotor.DownSpeed = new PLCParameter<int>();
                //et.Valid = true;
                et.Pressure = et.Pressure ?? new PLCParameter<int>();
                et.ExistTipCoil = et.ExistTipCoil ?? new PLCParameter<bool>();

                //et.PumpMotor.ProgramZero = 0;
                //et.ValidCoil = et.ValidCoil ?? new PLCParameter<bool>();
            }
        }
        private bool checkIndex(byte index)
        {
            var result = index <Entercloses.Length;
            if (!result) throw new System.Exception("超出通道索引范围");
            return result;
        }
        [Newtonsoft.Json.JsonIgnore]
        public Enterclose Logic0
        {
            get
            {
                return Entercloses[0];
            }
            set
            {
                Entercloses[0] = value;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public Enterclose Logic1
        {
            get
            {
                return Entercloses[1];
            }
            set
            {
                Entercloses[1] = value;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public Enterclose Logic2
        {
            get
            {
                return Entercloses[2];
            }
            set
            {
                Entercloses[2] = value;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public Enterclose Logic3
        {
            get
            {
                return Entercloses[3];
            }
            set
            {
                Entercloses[3] = value;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public Enterclose Logic4
        {
            get
            {
                return Entercloses[4];
            }
            set
            {
                Entercloses[4] = value;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public Enterclose Logic5
        {
            get
            {
                return Entercloses[5];
            }
            set
            {
                Entercloses[5] = value;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public Enterclose Logic6
        {
            get
            {
                return Entercloses[6];
            }
            set
            {
                Entercloses[6] = value;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public Enterclose Logic7
        {
            get
            {
                return Entercloses[7];
            }
            set
            {
                Entercloses[7] = value;
            }
        }
        public Electromotor XMotor { get; set; }

        public Electromotor TMotor { get; set; }//全动轴

        public Enterclose[] Entercloses { get; set; }
    }
}
