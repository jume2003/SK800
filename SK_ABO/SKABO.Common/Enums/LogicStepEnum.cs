using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Enums
{
    public enum LogicStepEnum
    {
        /// <summary>
        /// 全部初始化
        /// </summary>
        [Description("全部初始化")]
        InitAll=0,
        /// <summary>
        /// 提示信息
        /// </summary>
        [Description("提示信息")]
        Alert=1,
        /// <summary>
        /// 延时(ms)
        /// </summary>
        [Description("延时(ms)")]
        Delay=2,
        /// <summary>
        /// 计时器开启
        /// </summary>
        [Description("计时器开启")]
        TimerStart=3,
        /// <summary>
        /// 等待计时器
        /// </summary>
        [Description("等待计时器")]
        WaitTimer=4,
        /// <summary>
        /// 循环开始
        /// </summary>
        [Description("循环开始")]
        LoopStart = 5,
        /// <summary>
        /// 循环结束
        /// </summary>
        [Description("循环结束")]
        LoopEnd = 6,
        /// <summary>
        /// 子过程
        /// </summary>
        [Description("子过程")]
        SubFunc = 7,
        /// <summary>
        /// 16进制指令
        /// </summary>
        [Description("16进制指令")]
        Hex = 8,
        /// <summary>
        /// 脱吸头
        /// </summary>
        [Description("脱吸头")]
        OutTip = 9,
        /// <summary>
        /// 装吸头
        /// </summary>
        [Description("装吸头")]
        TakeTip = 10,
        /// <summary>
        /// 部件XY移动
        /// </summary>
        [Description("部件XY移动")]
        MoveXY = 11,
        /// <summary>
        /// 部件Z移动
        /// </summary>
        [Description("部件Z移动")]
        MoveZ = 12,
        /// <summary>
        /// 简单动作
        /// </summary>
        [Description("简单动作")]
        SimpleAction = 13,
        /// <summary>
        /// Z初始化
        /// </summary>
        [Description("Z初始化")]
        InitZ = 14,
        /// <summary>
        /// S初始化
        /// </summary>
        [Description("S初始化")]
        InitS = 15,
        /// <summary>
        /// 液面探测
        /// </summary>
        [Description("液面探测")]
        DetectSquid = 16,
        /// <summary>
        /// Z相对液面移动
        /// </summary>
        [Description("Z相对液面移动")]
        ZBySquid = 17,
        /// <summary>
        /// 固定吸液
        /// </summary>
        [Description("固定吸液")]
        FixedIn = 18,
        /// <summary>
        /// 固定分液
        /// </summary>
        [Description("固定分液")]
        FixedOut = 19,
        /// <summary>
        /// 抓GEL卡
        /// </summary>
        [Description("抓GEL卡")]
        TakeGel = 20,
        /// <summary>
        /// 放GEL卡
        /// </summary>
        [Description("放GEL卡")]
        PutDownGel = 21,
        /// <summary>
        /// 抓/放GEL卡
        /// </summary>
        [Description("抓/放GEL卡")]
        TakeAndPutDownGel = 22,
        /// <summary>
        /// 破孔器
        /// </summary>
        [Description("破孔器")]
        Piercer = 23,
        /// <summary>
        /// 离心机
        /// </summary>
        [Description("离心机")]
        Centrifuge = 24,
        /// <summary>
        /// 卡仓
        /// </summary>
        [Description("卡仓")]
        GelWarehouse = 25,
        /// <summary>
        /// 孵育器
        /// </summary>
        [Description("孵育器")]
        Couveuse = 26
    }
}
