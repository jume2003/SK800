using SKABO.Common.Models.Communication.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Communication
{
    public class CentrifugeSystem
    {
        public CentrifugeSystem()
        {
        }
        public CentrifugeSystem(bool Init)
        {
            if(Init)
                Centrifuges = new Centrifuge[2] { new Centrifuge(), new Centrifuge() };
        }
        public Centrifuge[] Centrifuges { get; set; }
    }
}
