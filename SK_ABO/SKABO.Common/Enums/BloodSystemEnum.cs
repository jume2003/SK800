using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Enums
{
    public enum BloodSystemEnum
    {
        [Description("ABO血型")]
        ABO = 1,
        [Description("Rh(CDE)血型")]
        CDE = 2,
        [Description("人球蛋白")]
        Globulin = 3,
        [Description("其它")]
        Other = 99
    }
}
