using SKABO.Common.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.BJ
{
    /// <summary>
    /// Gel卡仓
    /// </summary>
    public class T_BJ_GelWarehouse : VBJ
    {
        public override int ID { get; set; }
        [GridColumn("名称", 100D)]
        public override string Name { get; set; }
        [GridColumn("Count", 100)]
        public int Count { get; set; }
        [GridColumn("HandX(mm)", 100, "{0:#0.00}")]
        public decimal HandX { get; set; }
        [GridColumn("HandY(mm)", 100, "{0:#0.00}")]
        public decimal HandY { get; set; }
        [GridColumn("HandZ(mm)", 100, "{0:#0.00}")]
        public decimal HandZ { get; set; }
        [GridColumn("ZLimit(mm)", 100, "{0:#0.00}")]
        public decimal ZLimit { get; set; }
        [GridColumn("ZCatch(mm)", 100, "{0:#0.00}")]
        public decimal ZCatch { get; set; }
        [GridColumn("ZPut(mm)", 100, "{0:#0.00}")]
        public decimal ZPut { get; set; }
        [GridColumn("Gap(mm)", 100, "{0:#0.00}")]
        public decimal Gap { get; set; }
        [GridColumn("代号", 100D)]
        public String Code
        {
            get
            {
                return _Code;
            }
            set
            {
                _Code = value;
            }
        }
        /// <summary>
        /// 取卡位的X值
        /// </summary>
        [GridColumn("StoreX(mm)", 100, "{0:#0.00}")]
        public decimal StoreX { get; set; }
        /// <summary>
        /// 探卡卡位的X值
        /// </summary>
        [GridColumn("探卡X", 100, "{0:#0.00}")]
        public decimal DetectX { get; set; }
        /// <summary>
        /// 开仓门时的移动的距离
        /// </summary>
        [GridColumn("DoorX(mm)", 100, "{0:#0.00}")]
        public decimal DoorX { get; set; }
        public override Object[,] Values
        {
            get
            {
                if (_Values == null)
                {
                    _Values = new object[Count, 1];
                }
                return _Values;
            }
        }
    }
}
