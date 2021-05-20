using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Enums
{
    public enum CentrifugeStatusEnum
    {
        [Description("就绪")]
        Ready =0,
        [Description("加速中...")]
        AddSpeed =1,
        [Description("减速中...")]
        MinusSpeed =2,
        [Description("恒速中...")]
        KeepSpeed =3,
        [Description("停止...")]
        Stop = 4,
        [Description("禁用")]
        Disabled = 5
    }
}
