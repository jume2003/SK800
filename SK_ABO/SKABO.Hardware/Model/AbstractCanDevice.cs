using SKABO.ActionEngine;
using SKABO.Common.Models.Communication;
using SKABO.Common.Models.Communication.Unit;
using SKABO.Hardware.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Hardware.Model
{
    public abstract class AbstractCanDevice
    {
        public bool is_init_ok = false;
        public AbstractCanComm CanComm { get; set; }
        public abstract void LoadPLCValue();
        public abstract void Update2Plc();
        public abstract ActionBase InitAll();
    }
}
