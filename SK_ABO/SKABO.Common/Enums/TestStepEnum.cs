using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Enums
{
    public enum TestStepEnum
    {
        /// <summary>
        /// 加载Gel卡
        /// </summary>
        LoadGel =0,
        /// <summary>
        /// 开孔Gel卡
        /// </summary>
        KaiKongGel=1,
        /// <summary>
        /// 加样结束
        /// </summary>
        JYJS=2,
        /// <summary>
        /// 转卡到孵育
        /// </summary>
        ZKDFY=3,
        /// <summary>
        /// 转卡到离心机
        /// </summary>
        ZKDLXJ=4,
        /// <summary>
        /// 相机判读
        /// </summary>
        XJPD=5,
        /// <summary>
        /// 转卡到常温
        /// </summary>
        ZKDCW=6,
        /// <summary>
        /// 分配试剂
        /// </summary>
        FPSJ=7,
        /// <summary>
        /// 分配病人血清
        /// </summary>
        FPBRXQ=8,
        /// <summary>
        /// 分配病人稀释红细胞
        /// </summary>
        FPBRXSHXB=9,
        /// <summary>
        /// 分配献血员稀释红细胞
        /// </summary>
        FPXXYXSHXB=10,
        /// <summary>
        /// 分配献血员血清
        /// </summary>
        FPXXYXQ=11,
        /// <summary>
        /// 延时
        /// </summary>
        YS=12,
        /// <summary>
        /// 离心
        /// </summary>
        LXJDZ=13,
        /// <summary>
        /// 稀释红细胞
        /// </summary>
        XSHXB=14,
        /// <summary>
        /// 加载针头
        /// </summary>
        JXZT = 15,
        /// <summary>
        /// 分配红细胞
        /// </summary>
        FPHXB = 16,
        /// <summary>
        /// 分配稀释液
        /// </summary>
        FPXSY = 17,
        PutTip = 18,//脱针
        AbsLiquid = 19,//吸液
        SpuLiquid = 20,//分液
        MixLiquid = 21,//混合
        FollowAbsLiquid = 22,//跟随吸液
        PutPeiGelBack = 23,//放回配平位
        Define = 24,// 默认
        /// <summary>
        /// 分配液体
        /// </summary>
        FPYT = 25,
        /// <summary>
        /// 节约计时
        /// </summary>
        ECONOMIZECOUNTTIME = 26,
        /// <summary>
        /// 组合补位
        /// </summary>
        FULLUP = 27,
        /// <summary>
        /// 病人生理盐水
        /// </summary>
        FPBRSLYS = 28,
        /// <summary>
        /// 献血员生理盐水
        /// </summary>
        FPXXYSLYS = 29,
        /// <summary>
        /// GEL卡动作分隔
        /// </summary>
        GELEND = 1000
    }
}
