using SKABO.ActionEngine;
using SKABO.Common.Enums;
using SKABO.Common.Models;
using SKABO.Common.Models.Communication;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Common.Utils;
using SKABO.Hardware.Core;
using SKABO.Hardware.Model;
using SKABO.ResourcesManager;
using System;
using System.Linq;

namespace SKABO.Hardware.RunBJ
{
    public class CentrifugeDevice : AbstractCanDevice
    {
        public Centrifuge[] Centrifuges { get; set; }
        public CentrifugeDevice(AbstractCanComm CanComm, CentrifugeSystem CentSys)
        {
            this.CanComm = CanComm;
            this.Centrifuges = CentSys.Centrifuges;
        }
        public override void LoadPLCValue()
        {

        }

        public override void Update2Plc()
        {

        }

        public override ActionBase InitAll()
        {
            return null;
        }
    }
}
