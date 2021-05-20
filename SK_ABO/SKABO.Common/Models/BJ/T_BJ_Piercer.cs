using SKABO.Common.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.BJ
{
    public class T_BJ_Piercer : VBJ
    {
        /// <summary>
        /// 破孔器
        /// </summary>
        public override int ID { get; set; }
        [GridColumn("名称",100D)]
        public override string Name { get; set; }
        [GridColumn("Y1(mm)", 100, "{0:#0.00}")]
        public decimal Y1 { get; set; }
        [GridColumn("步长(mm)", 100, "{0:#0.00}")]
        public decimal StepY { get; set; }
        [GridColumn("孔深(mm)", 100, "{0:#0.00}")]
        public decimal Z { get; set; }
    }
}
