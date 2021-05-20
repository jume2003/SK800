using System;
using System.Collections.Generic;
using System.Linq;
namespace SKABO.Common.Models.Communication
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class PLCParameter<T>
    {
        /// <summary>
        /// PLC地址
        /// </summary>
        public String Addr { get; set; }
        /// <summary>
        /// 设定值
        /// </summary>
        //[Newtonsoft.Json.JsonIgnore]
        public T SetValue { get; set; }
        /// <summary>
        /// PLC当前值
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public T CurrentValue { get; set; }
    }
}
