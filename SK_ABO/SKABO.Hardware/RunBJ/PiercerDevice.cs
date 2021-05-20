using SKABO.ActionEngine;
using SKABO.Common;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.Communication;
using SKABO.Common.Utils;
using SKABO.Hardware.Core;
using SKABO.Hardware.Model;
using SKABO.MAI.ErrorSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SKABO.Hardware.RunBJ
{
    public class PiercerDevice: AbstractCanDevice
    {
        public PiercerDevice(AbstractCanComm CanComm, Piercer Pie)
        {
            this.CanComm = CanComm;
            this.Pie = Pie;
        }
        public Piercer Pie { get; set; }
        public override void LoadPLCValue()
        {
            CanComm.ReadMotor(this.Pie.YMotor);
            CanComm.ReadMotor(this.Pie.ZMotor);
        }
        public override void Update2Plc()
        {
            CanComm.SetMotor(this.Pie.YMotor);
            CanComm.SetMotor(this.Pie.ZMotor);
        }
        public bool InitZ(bool OnlyStart = false)
        {
            return CanComm.InitMotor(this.Pie.ZMotor,OnlyStart);
        }
        public bool InitY(bool OnlyStart = false)
        {
            return CanComm.InitMotor(this.Pie.YMotor, OnlyStart);
        }
        public override ActionBase InitAll()
        {
            var seque = Sequence.create();
            seque.AddAction(InitXyz.create(this, 7000, false, false, true));
            seque.AddAction(InitXyz.create(this, 7000, false, true, false));
            return seque;
        }
        public bool MoveYTo(decimal? Y, bool OnlyStart = false)
        {
            return CanComm.MoveMotor(this.Pie.YMotor,Y,OnlyStart);
        }
        public bool MoveZTo(decimal? Z, bool OnlyStart = false)
        {
            return CanComm.MoveMotor(this.Pie.ZMotor,Z,OnlyStart);
        }
    }
}
