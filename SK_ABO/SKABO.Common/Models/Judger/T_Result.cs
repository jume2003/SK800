using SKABO.Common.Models.GEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Judger
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class T_Result
    {
        public long ID { get; set; }
        public long PictureID { get; set; }
        public int GelID { get; set; }
        public string GelBarcode { get; set; }
        public string SmpBarcode { get; set; }
        public byte TubeStartNo { get; set; }
        public byte TubeCount { get; set; }
        public string TubeNums { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Result { get; set; }
        public string TestUser { get; set; }
        public string EditUser { get; set; }
        public string VerifyUser { get; set; }
        public DateTime? EditTime { get; set; }
        public DateTime? VerifyTime { get; set; }
        public DateTime? ReportTime { get; set; }
        public string ReportUser { get; set; }
        public string LED { get; set; }
        /// <summary>
        /// 供血者条码
        /// </summary>
        public string DonorBarcode { get; set; }
        public string Color { get; set; }
        public string Remark { get; set; }
        /// <summary>
        /// 是否人工判定
        /// </summary>
        public bool IsManJudger { get; set; }
        /// <summary>
        /// 是否为质控样本
        /// </summary>
        public bool IsQC { get; set; }
        /// <summary>
        /// 载架的位置
        /// </summary>
        public String RackIndex { get; set; }
        /// <summary>
        /// 测试时是否脱离过载架
        /// </summary>
        public bool Outed { get; set; }

        public T_Picture Picture { get; set; }
        public T_Gel Gel { get; set; }
        public string GelName { get; set; }
        /// <summary>
        /// 实验名称
        /// </summary>
        public string TestName { get; set; }
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSel { get; set; }
    }
}