using SKABO.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.GEL
{
    public class T_StepDefine
    {
        public int ID { get; set; }
        public string StepName { get; set; }
        public TestStepEnum StepClass { get; set; }
        public string StepDesc { get; set; }
    }
}
