using SKABO.Common.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.BJ
{
    /// <summary>
    /// 脱针器
    /// </summary>
    public class T_BJ_Unload : VBJ
    {

        public override int ID { get; set; }
        [GridColumn("名称", 100D)]
        public override string Name { get; set; }
        [GridColumn("X", 70, "{0:#0.00}")]
        public decimal X { get; set; }
        [GridColumn("Y", 70, "{0:#0.00}")]
        public decimal Y { get; set; }
        [GridColumn("Z", 70, "{0:#0.00}")]
        public decimal Z { get; set; }
        [GridColumn("Count", 70)]
        public int Count { get; set; }
        [GridColumn("FZ", 70, "{0:#0.00}")]
        public decimal FZ { get; set; }
        [GridColumn("FirstX", 70, "{0:#0.00}")]
        public decimal FirstX { get; set; }
        
    }
}
