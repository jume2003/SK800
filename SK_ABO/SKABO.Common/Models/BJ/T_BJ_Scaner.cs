using SKABO.Common.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.BJ
{
    public class T_BJ_Scaner : VBJ
    {
        /// <summary>
        /// 扫描仪
        /// </summary>
        public override int ID { get; set; }
        [GridColumn("名称", 100D)]
        public override string Name { get; set; }
        [GridColumn("HandX", 70, "{0:#0.00}")]
        public decimal HandX { get; set; }
        [GridColumn("HandY", 70, "{0:#0.00}")]
        public decimal HandY { get; set; }
        [GridColumn("HandZ", 70, "{0:#0.00}")]
        public decimal HandZ { get; set; }
        /*
        [GridColumn("HandZLimit", 70, "{0:#0.00}")]
        public decimal HandZLimit { get; set; }
        */
        [GridColumn("端口", 70, "{0:#0.00}")]
        public string Port { get; set; }
        [GridColumn("目标\r\n0：样本\r\n1：Gel卡", 70)]
        public int Purpose { get; set; }
        [GridColumn("型号", 70)]
        public String ScanerType { get; set; }
        
        
    }
}
