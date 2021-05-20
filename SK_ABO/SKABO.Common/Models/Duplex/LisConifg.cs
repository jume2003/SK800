using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Duplex
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class LisConifg
    {
        /// <summary>
        /// 双工文件夹
        /// </summary>
        public String DuplexDir { get; set; }
        /// <summary>
        /// 双工结果文件夹
        /// </summary>
        public String ResultDir { get; set; }
        /// <summary>
        /// 是否需确认窗口
        /// </summary>
        public bool NeedConfirm { get; set; } 
        /// <summary>
        /// 是否自动发送测试结果
        /// </summary>
        public bool AutoSendResult { get; set; }
        /// <summary>
        /// 是否DES加密传输
        /// </summary>
        public bool NeedDes { get; set; }
        /// <summary>
        /// DES密码
        /// </summary>
        public String DesPassword { get; set; }
        /// <summary>
        /// 结果中是否包含图片
        /// </summary>
        public bool IncludePic { get; set; }
        /// <summary>
        /// 测试项目
        /// </summary>
        public int TI { get; set; } = -1;
    }
}
