using System.ComponentModel;

namespace SKABO.Hardware.Enums
{
    public enum ModbusErrorEnum
    {
        /// <summary>
        /// 非法的功能码
        /// </summary>
        [Description("非法的功能码")]
        FunCodeError =0x01,
        /// <summary>
        /// 非法的数据地址
        /// </summary>
        [Description("非法的数据地址")]
        AddrError =0x02,
        /// <summary>
        /// 非法的数据值
        /// </summary>
        [Description("非法的数据值")]
        DataValError =0x03,
        /// <summary>
        /// 服务器故障
        /// </summary>
        [Description("服务器故障")]
        ServerError =0x04,
        /// <summary>
        /// 服务器确认请求
        /// </summary>
        [Description("服务器确认请求")]
        ServerConfirm =0x05,
        /// <summary>
        /// 服务器繁忙
        /// </summary>
        [Description("服务器繁忙")]
        ServerBusy =0x06,
        /// <summary>
        /// 网关无效
        /// </summary>
        [Description("网关无效")]
        GatewayError =0x0a,
        /// <summary>
        /// 目标设备没有响应
        /// </summary>
        [Description("目标设备没有响应")]
        NoResponse =0x0b
    }
}
