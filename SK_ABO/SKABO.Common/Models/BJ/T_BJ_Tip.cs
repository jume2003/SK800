using SKABO.Common.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.BJ
{
    /// <summary>
    /// 吸头盒
    /// </summary>
    public class T_BJ_Tip : VBJ
    {

        public override int ID { get; set; }
        [GridColumn("名称", 100D)]
        public override string Name { get; set; }
        [GridColumn("X", 70, "{0:#0.00}")]
        public decimal X { get; set; }
        [GridColumn("Y", 70, "{0:#0.00}")]
        public decimal Y { get; set; }
        [GridColumn("Limit", 70, "{0:#0.00}")]
        public decimal Limit { get; set; }
        [GridColumn("MinSidth", 70, "{0:#0.00}")]
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
        [GridColumn("GapX", 70, "{0:#0.00}")]
        public decimal GapX { get; set; }
        [GridColumn("GapY", 70, "{0:#0.00}")]
        public decimal GapY { get; set; }
        [GridColumn("FZ", 70, "{0:#0.00}")]
        public decimal FZ { get; set; }
        [GridColumn("CountX", 70)]
        public int CountX { get; set; }
        [GridColumn("CountY", 70)]
        public int CountY { get; set; }
        [GridColumn("Hard", 70, "{0:#0.00}")]
        public int Hard { get; set; }
        [GridColumn("Volumn", 70, "{0:#0.00}")]
        public int Volumn { get; set; }
        public override Object[,] Values
        {
            get
            {
                if (_Values == null)
                {
                    _Values = new object[CountX, CountY];
                }
                return _Values;
            }
        }
        public int GetY(int index)
        {
            decimal[] FixPoints = { FixPoint1, FixPoint2, FixPoint3, FixPoint4, FixPoint5 };
            decimal[] FixIndexs = { FixIndex1, FixIndex2, FixIndex3, FixIndex4, FixIndex5 };
            int fixindex = GetFixPoint(index, FixPoints, FixIndexs);
            int seaty = fixindex != -1 ? (int)FixPoints[fixindex] : (int)Y;
            index = fixindex != -1 ? index - (int)FixIndexs[fixindex] : index;
            seaty = seaty + index * (int)GapY;
            return seaty;
        }

    }
}
