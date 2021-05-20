using SKABO.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models
{
    public class CentrifugeStatusChangeEventArg
    {
        public String Code { get; set; }
        public int Time { get; set; }
        public CentrifugeStatusEnum StatusEnum { get; set; }
    }
}
