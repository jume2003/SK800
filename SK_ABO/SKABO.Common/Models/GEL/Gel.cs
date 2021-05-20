using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.GEL
{
    /// <summary>
    /// Gel卡信息，用于记录开孔时间等信息，为节约用卡功能准备
    /// </summary>
    public struct Gel
    {
        public Gel(String BarCode)
        {
            this.BarCode = BarCode;
            Pierce = null;
            FirstNo = 0;
            SampleBarcodes = new List<String>();
        }
        /// <summary>
        /// Gel卡条码
        /// </summary>
        public String BarCode { get; set; }
        /// <summary>
        /// 开孔时间
        /// </summary>
        public DateTime? Pierce { get; set; }
        /// <summary>
        /// 第一个可用的微柱索引号
        /// </summary>
        public byte FirstNo { get; set; }
        /// <summary>
        /// 当前卡对应的样本号
        /// </summary>
        public IList<String> SampleBarcodes { get;
        }
        public override string ToString()
        {
            var res= $"{BarCode}";
            if (SampleBarcodes != null)
            {
                foreach(String b in SampleBarcodes)
                {
                    res += "\r\n" + b;
                }
            }
            return res;
        }
    }
}
