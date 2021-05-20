using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Hardware.Enums
{
    public enum CanFunCodeEnum
    {
        /// <summary>
        /// 1、	读线圈
        /// </summary>
        READ_COIL = 0X01,
        /// <summary>
        /// 2、	写单个线圈
        /// </summary>
        WRITE_SINGLE_COIL = 0X05,
        /// <summary>
        /// 3、	写多个线圈
        /// </summary>
        WRITE_MULTI_COIL = 0X0F,
        /// <summary>
        /// 4、	写双寄存器
        /// </summary>
        WRITE_SINGLE_REGISTER = 0X06,
        //8多个字节
        WRITE_MULTI_BYTE = 0X07,
        /// <summary>
        /// 5、	读双寄存器
        /// </summary>
        READ_REGISTER = 0X03,
        /// <summary>
        /// 6、	主动上报双寄存器
        /// </summary>
        UPLOAD_REGISTER = 0X10,
        /// <summary>
        /// 7、	主动上报线圈
        /// </summary>
        UPLOAD_COIL = 0X11

    }
}
