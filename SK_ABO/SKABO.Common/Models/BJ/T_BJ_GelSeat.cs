using SKABO.Common.Models.Attributes;
using System;

namespace SKABO.Common.Models.BJ
{
    /// <summary>
    /// Gel卡位
    /// </summary>
    public class T_BJ_GelSeat : VBJ
    {

        public override int ID { get; set; }
        [GridColumn("名称", 100D)]
        public override string Name { get; set; }
        [GridColumn("Count", 100D, "{0:#0}")]
        public int Count { get; set; }
        [GridColumn("X(mm)", 100D, "{0:#0.00}")]
        public decimal X { get; set; }
        [GridColumn("Y(mm)", 100D, "{0:#0.00}")]
        public decimal Y { get; set; }
        [GridColumn("Z(mm)", 100D, "{0:#0.00}")]
        public decimal Z { get; set; }
        [GridColumn("MinWidth", 70, "{0:#0.00}")]
        public decimal MinWidth { get; set; }
        [GridColumn("FixPoint1", 70, "{0:#0.00}")]
        public decimal FixPoint1 { get; set; }
        [GridColumn("FixIndex1", 70, "{0:#0.00}")]
        public decimal FixIndex1 { get; set; }
        [GridColumn("FixPoint2", 70, "{0:#0.00}")]
        public decimal FixPoint2 { get; set; }
        [GridColumn("FixIndex2", 70, "{0:#0.00}")]
        public decimal FixIndex2 { get; set; }
        [GridColumn("FixPoint3", 70, "{0:#0.00}")]
        public decimal FixPoint3 { get; set; }
        [GridColumn("FixIndex3", 70, "{0:#0.00}")]
        public decimal FixIndex3 { get; set; }
        [GridColumn("FixPoint4", 70, "{0:#0.00}")]
        public decimal FixPoint4 { get; set; }
        [GridColumn("FixIndex4", 70, "{0:#0.00}")]
        public decimal FixIndex4 { get; set; }
        [GridColumn("FixPoint5", 70, "{0:#0.00}")]
        public decimal FixPoint5 { get; set; }
        [GridColumn("FixIndex5", 70, "{0:#0.00}")]
        public decimal FixIndex5 { get; set; }
        [GridColumn("ZLimit(mm)", 100D, "{0:#0.00}")]
        public decimal ZLimit { get; set; }
        [GridColumn("ZCatch(mm)", 100D, "{0:#0.00}")]
        public decimal ZCatch { get; set; }
        [GridColumn("ZPut(mm)", 100D, "{0:#0.00}")]
        public decimal ZPut { get; set; }
        [GridColumn("Gap(mm)", 100D, "{0:#0.00}")]
        public decimal Gap { get; set; }
        [GridColumn("代号\r\n孵育器要设置", 70, "{0:#0.00}")]
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
        /// 0：分配位
        /// 1：孵育位
        /// 2：配平/节约位
        /// 3：待定位
        /// 4：破孔位
        /// </summary>
        [GridColumn("用途\r\n0：分配位\r\n1：孵育位\r\n2：配平/节约位\r\n3：待定位\r\n4：破孔位", 100D)]
        public int Purpose { get; set; }
        [GridColumn("破孔Y(mm)", 100D, "{0:#0.00}")]
        public decimal YForPie { get; set; }
        [GridColumn("破孔Z(mm)", 100D, "{0:#0.00}")]
        public decimal ZForPie { get; set; }
        [GridColumn("破孔Gap(mm)", 100D, "{0:#0.00}")]
        public decimal GapForPie { get; set; }
        [GridColumn("InjectorX(mm)\r\n加样器", 100D, "{0:#0.00}")]
        public decimal InjectorX { get; set; }
        [GridColumn("InjectorY(mm)\r\n加样器", 100D, "{0:#0.00}")]
        public decimal InjectorY { get; set; }
        [GridColumn("InjectorZ(mm)\r\n加样器", 100D, "{0:#0.00}")]
        public decimal InjectorZ { get; set; }
        [GridColumn("InjectorGapX(mm)\r\n加样器", 100D, "{0:#0.00}")]
        public decimal InjectorGapX { get; set; }
        [GridColumn("InjectorGapY(mm)\r\n加样器", 100D, "{0:#0.00}")]
        public decimal InjectorGapY { get; set; }

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

        public int GetInjectorY(int index)
        {
            decimal[] FixPoints = { FixPoint1, FixPoint2, FixPoint3, FixPoint4, FixPoint5 };
            decimal[] FixIndexs = { FixIndex1, FixIndex2, FixIndex3, FixIndex4, FixIndex5 };
            int fixindex = GetFixPoint(index, FixPoints, FixIndexs);
            int seaty = fixindex != -1 ? (int)FixPoints[fixindex] : (int)InjectorY;
            index = fixindex != -1 ? index - (int)FixIndexs[fixindex] : index;
            seaty = seaty + index * (int)InjectorGapY;
            return seaty;
        }
    }
}
