using SKABO.Common;
using SKABO.Common.Enums;
using SKABO.Common.Models.GEL;
using SKABO.Common.Models.Judger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK_ABO.robot
{
    /// <summary>
    /// 测试包对象
    /// </summary>
    public class TestBag123
    {
        internal TestBag123(TestLevelEnum testLevel)
        {
            this.TestLevel = testLevel;
            
        }
        internal TestLevelEnum TestLevel { get; set; }
        /// <summary>
        /// 测试项目卡信息
        /// </summary>
        internal T_Gel GelType { get; set; }
        private IList<ValueTuple<String, byte, byte, T_Result>> _SamplesInfo;
        /// <summary>
        /// T1:样本号，T2，载架号，T3，位置号，T4,测试结果
        /// </summary>
        internal IList<ValueTuple<String,byte,byte,T_Result>> SamplesInfo { get
            {
                if (_SamplesInfo == null)
                {
                    _SamplesInfo = new List<ValueTuple<String, byte, byte, T_Result>>();
                }
                return _SamplesInfo;
            }
        }
        internal void Add(String SampleBarcode, byte RackIndex, byte Index)
        {
            T_Result result = new T_Result();
            result.GelID = GelType.ID;
            result.SmpBarcode = SampleBarcode;
            result.TestUser = Constants.Login.LoginName;
            var vt = ValueTuple.Create<String, byte, byte, T_Result>(SampleBarcode, RackIndex, Index, result);
            this.SamplesInfo.Add(vt);
        }
        internal (String SampleBarcode,byte RackIndex,byte Index,T_Result Result) this[byte i]
        {
            get
            {
                return SamplesInfo[i];
            }
        }
        
    }
}
