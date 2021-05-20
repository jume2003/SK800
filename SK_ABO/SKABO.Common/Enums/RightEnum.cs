using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Enums
{
    public enum RightEnum
    {
        /// <summary>
        /// 角色管理
        /// </summary>
        [Description("角色管理")]
        ManageRole =1,
        /// <summary>
        /// 用户管理
        /// </summary>
        [Description("用户管理")]
        ManageUser =2,
        /// <summary>
        /// 试验操作
        /// </summary>
        [Description("试验操作")]
        Test =4,
        /// <summary>
        /// 修改结果
        /// </summary>
        [Description("修改结果")]
        ModifyResult =8,
        /// <summary>
        /// 确认结果
        /// </summary>
        [Description("确认结果")]
        ConfirmResult =16,
        /// <summary>
        /// 发送结果
        /// </summary>
        [Description("发送结果")]
        SendResult =32,
        /// <summary>
        /// 打印结果
        /// </summary>
        [Description("打印结果")]
        PrintResult =64,
        /// <summary>
        /// 统计
        /// </summary>
        [Description("统计")]
        Statistics =128,
        /// <summary>
        /// 查看部件参数
        /// </summary>
        [Description("查看部件参数")]
        DeviceParameter =256,
        /// <summary>
        /// 修改部件参数
        /// </summary>
        [Description("修改部件参数")]
        DeviceParameterModify = 512,
        /// <summary>
        /// 控制台操作
        /// </summary>
        [Description("控制台操作")]
        Console =1024,
        /// <summary>
        /// 日志管理
        /// </summary>
        [Description("日志管理")]
        ManageLog =2048
    }
}
