using SKABO.Common.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.BJ
{
    /// <summary>
    /// 相机
    /// </summary>
    public class T_BJ_Camera: VBJ
    {
        public override int ID { get; set; }
        [GridColumn("名称", 100D)]
        public override string Name { get; set; }
        [GridColumn("HandX(mm)", 100, "{0:#0.00}")]
        public decimal HandX { get; set; }
        [GridColumn("HandY(mm)", 100, "{0:#0.00}")]
        public decimal HandY { get; set; }
        [GridColumn("HandZ(mm)", 100, "{0:#0.00}")]
        public decimal HandZ { get; set; }
        [GridColumn("LED CMD", 100, "{0:#0.00}")]
        public int OrderForLED { get; set; }
    }
}
