using SKABO.Common.Enums;
using SKABO.Common.Models.BJ;
using SKABO.Common.Models.NotDuplex;
using SKABO.Common.Models.TestStep;
using SKABO.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.GEL
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class T_GelStep : System.ICloneable
    {
        public int ID { get; set; }
        public int GelID { get; set; }
        public int StepID { get; set; }
        public int StepIndex { get; set; }
        public string StepParamters { get; set; }
        public string StepName { get; set; }
        public TestStepEnum StepClass { get; set; }
        public int GoSideID { get; set; }//并行ID
        public object ExperPackage { get; set; }
        //public object Group { get; set; }//所在组合
        public int InjectCount { get; set; }//针头
        public int SampleIndex { get; set; }//病人样本索引exppackage
        //public int State { get; set; } = 0;//0没执行1执行中2执行完
        //混合Code(用于在混合时在deep盘中找到目标)
        public string MixCode { get; set; }
        //当前动作液体类型
        public int LiquidTypeIndex { get; set; }
        //液体信息
        FPYTStepParameter FpytInfo { get; set; } = null;
        //是否有混合动作
        public bool is_mix { get; set; } = false;
        //是否有分配动作
        public bool is_spu { get; set; } = false;
        //动作属于哪个针头
        public int InjIndex { get; set; } = 0;
        //Y轴优先级
        public int HitSort { get; set; } = 0;//Y轴优先级
        //试剂跳过
        public bool is_skip_zjt { get; set; } = false;//是否跳过装针
        public bool is_skip_abs { get; set; } = false;//是否跳过加样
        public bool is_skip_spu { get; set; } = false;//是否跳过分配
        public bool is_skip_puttip { get; set; } = false;//是否跳过脱针
        public bool is_skip_mix { get; set; } = false;//是否跳过混合
        public bool is_skip_spu_back { get; set; } = false;//是否跳过分配回零
        public int skip_spu_times { get; set; } = 1;//是否跳过分配倍数
        public int after_mix_spucapacity { get; set; } = 1;//混合后的吸液倍数
        public void CopySkip(T_GelStep action)
        {
            //试剂跳过
            is_skip_zjt = action.is_skip_zjt;
            is_skip_abs = action.is_skip_abs;
            is_skip_spu = action.is_skip_spu;
            is_skip_puttip = action.is_skip_puttip;
            is_skip_mix = action.is_skip_mix;
            is_skip_spu_back = action.is_skip_spu_back;
        }
        public void SetFpytInfo(FPYTStepParameter FpytInfoTem)
        {
            if (StepClass == TestStepEnum.FPYT)
            {
                StepParamters = FpytInfoTem.ToJsonStr();
            }
            FpytInfo = FpytInfoTem;
        }

        public FPYTStepParameter GetFpytInfo()
        {
            if(StepClass==TestStepEnum.FPYT&& FpytInfo==null)
            {
                FpytInfo = StepParamters.ToInstance<FPYTStepParameter>();
            }
            return FpytInfo;
        }
        public FPYTUnit GetLiquidInfo()
        {
            FPYTStepParameter fpytinfo = GetFpytInfo();
            if (fpytinfo != null&& fpytinfo.LiquidList.Count!=0)
            {
                return fpytinfo.LiquidList[LiquidTypeIndex];
            }
            return null;
        }
        public TestStepEnum GetLiquidType()
        {
            FPYTUnit fpyunity = GetLiquidInfo();
            if(fpyunity!=null&& FpytInfo!=null)
            {
                if (fpyunity.LiquidType== "病人血清") return TestStepEnum.FPBRXQ;
                if (fpyunity.LiquidType == "病人红细胞") return TestStepEnum.FPBRXSHXB;
                if (fpyunity.LiquidType == "献血员血清") return TestStepEnum.FPXXYXQ;
                if (fpyunity.LiquidType == "献血员红细胞") return TestStepEnum.FPXXYXSHXB;
                if (FpytInfo.FindLiquidType("病人*")&& fpyunity.IsSlys) return TestStepEnum.FPBRSLYS;
                if (FpytInfo.FindLiquidType("献血*") && fpyunity.IsSlys) return TestStepEnum.FPXXYSLYS;
            }
            return TestStepEnum.Define;
        }
        public string GetHoldLiquidName()
        {
            string hold_name = "";
            var fpytinfo = GetFpytInfo();
            if(fpytinfo!=null)
            {
                foreach(var lique in fpytinfo.LiquidList)
                {
                    hold_name += lique.LiquidType;
                }
            }
            return hold_name;
        }
        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
        public virtual T_GelStep clone()
        {
           return (T_GelStep)Clone();
        }
    }

    
}
