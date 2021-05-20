using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Hardware.Enums
{
    public class ModbusFunCode
    {
        
        /// <summary>
        /// 读线圈
        /// </summary>
        public static byte READ_COIL = 0X01;
        /// <summary>
        /// 读离散输入
        /// </summary>
        public static byte READ_DISCRETE = 0X02;
        /// <summary>
        /// 写单个线圈
        /// </summary>
        public static byte WRITE_SINGLE_COIL = 0X05;
        /// <summary>
        /// 读错误状态
        /// </summary>
        public static byte READ_ERROR_STATUS = 0X07;
        /// <summary>
        /// 写多个线圈
        /// </summary>
        public static byte WRITE_MULTI_COIL = 0X0F;
        /// <summary>
        /// 写单个寄存器 register
        /// </summary>
        public static byte WRITE_SINGLE_REGISTER = 0X06;
        /// <summary>
        ///写多个寄存器
        /// </summary>
        public static byte WRITE_MULTI_REGISTER = 0x10;
        /// <summary>
        /// 读寄存器
        /// </summary>
        public static byte READ_REGISTER = 0X03;
        /// <summary>
        /// 读写多个寄存器
        /// </summary>
        public static byte READ_WRITE_REGISTER = 0x17;
        /// <summary>
        /// 屏蔽寄存器
        /// </summary>
        public static byte MASK_REGISTER = 0x16;
    }
}
