using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Enums
{
    public enum CentrifugeActionEnum
    {
        
        [Description("停止")]
        Stop =0,
        [Description("低速")]
        Low =1,
        [Description("高速")]
        High =2,
        [Description("启动")]
        Start = 4,
    }
}
