using SKABO.Common.Enums;
using SKABO.Common.Models.NotDuplex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.GEL
{
    public class T_Gel : System.ICloneable
    {

        public int ID { get; set; }
        public string GelName { get; set; }
        /// <summary>
        /// 实验名称
        /// </summary>
        public string TestName { get; set; }
        public int GelType { get; set; }
        public string GelMask { get; set; }
        public bool IsMaskAtEnd { get; set; }
        public bool IsEnabled { get; set; }
        public int GelRenFen { get; set; }
        public string Name1 { get; set; }
        public string Name2 { get; set; }
        public string Name3 { get; set; }
        public string Name4 { get; set; }
        public string Name5 { get; set; }
        public string Name6 { get; set; }
        public string Name7 { get; set; }
        public string Name8 { get; set; }
        public String UnknownResult { get; set; }
        /// <summary>
        /// Lis系统卡代码
        /// </summary>
        public int LisGelClass { get; set; }
        /// <summary>
        /// 单批次最大开卡数量
        /// </summary>
        public int MaxInOne { get; set; }
        /// <summary>
        /// 开孔停留时间
        /// </summary>
        public int KeepTime { get; set; }
        /// <summary>
        /// 优先级，越大越高
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 判读后处置方式
        /// </summary>
        public HandledMethodAfterJudgedEnum AfterJudged { get; set; }
        /// <summary>
        /// 是否启用节约卡
        /// </summary>
        public bool IsUsedGel { get; set; }
        /// <summary>
        /// 是否交叉配血卡
        /// </summary>
        public bool IsCrossMatching { get; set; }
        /// <summary>
        /// 开孔后失效时间
        /// </summary>
        public int AfterKKTime { get; set; }
        public List<T_ResultMap> ResultMaps { get; set; }

        public List<T_GelStep> GelSteps { get; set; }

        public T_Gel clone()
        {
            return (T_Gel)Clone();
        }

        public virtual object Clone()
        {
            var gel_copy = (T_Gel)this.MemberwiseClone();
            gel_copy.GelSteps = new List<T_GelStep>();
            foreach(var step in GelSteps)gel_copy.GelSteps.Add(step.clone());
            return gel_copy;
        }
    }
}
