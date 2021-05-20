using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Enums
{
    public enum HandStatusEnum
    {
        NoWork=0,
        LoadingGEL=1,
        MoveGEL2Cent=2,
        MoveGEL2FY = 3,
        PD =4,
        Waiting=5
    }
}
