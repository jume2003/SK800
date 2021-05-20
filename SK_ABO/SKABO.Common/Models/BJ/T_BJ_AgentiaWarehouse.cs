using SKABO.Common.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.BJ
{
    /// <summary>
    /// 试剂仓
    /// </summary>
    public class T_BJ_AgentiaWarehouse: VBJ
    {
        /// <summary>
        /// 当前的液面高度
        /// </summary>
        public decimal? CurrentZ { get; set; }
        public override int ID { get; set; }
        [GridColumn("名称", 100D)]
        public override string Name { get; set; }
        [GridColumn("Count", 70, StringFormat: "{0:#0}")]
        public int Count { get; set; }
        [GridColumn("X(mm)",70,StringFormat: "{0:#0.00}")]
        public decimal X { get; set; }
        [GridColumn("Y(mm)",70, "{0:#0.00}")]
        public decimal Y { get; set; }
        [GridColumn("Z(mm)",70, "{0:#0.00}")]
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
        [GridColumn("Gap(mm)", 70, "{0:#0.00}")]
        public decimal Gap { get; set; }
        [GridColumn("Limit(mm)",70, "{0:#0.00}")]
        public decimal Limit { get; set; }
        [GridColumn("mm/ul",70, "{0:#0.0000}")]
        public decimal DeepForUl { get; set; }
        public override Object[,] Values { get {
                if (_Values == null)
                {
                    _Values = new object[Count, 1];
                }
                return _Values;
            }
        }
        public bool FindAgByCode(string code,ref int index)
        {
            bool is_find = false;
            index = 0;
            foreach (var en in Values)
            {
                if(en!=null)
                {
                    if (en.ToString() == code)
                    {
                        is_find = true;
                        break;
                    }
                    index++;
                }
            }
            return is_find;
        }
        public int GetY(int index)
        {
            decimal[] FixPoints = { FixPoint1, FixPoint2, FixPoint3, FixPoint4, FixPoint5 };
            decimal[] FixIndexs = { FixIndex1, FixIndex2, FixIndex3, FixIndex4, FixIndex5 };
            int fixindex = GetFixPoint(index, FixPoints, FixIndexs);
            int seaty = fixindex != -1 ? (int)FixPoints[fixindex] : (int)Y;
            index = fixindex != -1 ? index - (int)FixIndexs[fixindex] : index;
            seaty = seaty + index * (int)Gap;
            return seaty;
        }
    }
}
