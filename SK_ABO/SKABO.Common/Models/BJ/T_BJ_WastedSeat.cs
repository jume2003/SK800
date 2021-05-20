using SKABO.Common.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.BJ
{
    /// <summary>
    /// 废卡位
    /// </summary>
    public class T_BJ_WastedSeat : VBJ
    {

        public override int ID { get; set; }
        [GridColumn("名称", 100D)]
        public override string Name { get; set; }
        [GridColumn("HandX", 70, "{0:#0.00}")]
        public decimal HandX { get; set; }
        [GridColumn("HandY", 70, "{0:#0.00}")]
        public decimal HandY { get; set; }
        [GridColumn("HandZ", 70, "{0:#0.00}")]
        public decimal HandZ { get; set; }
        [GridColumn("Purpose\r0：针箱\r1：废箱", 70, "{0:#0}")]
        public byte Purpose { get; set; }


    }
}
