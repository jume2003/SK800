using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Utils
{
    public class ByteUtil
    {
        public static byte[] SingleToBytes(float value)
        {
            byte[] bs = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                byte temp = bs[0];
                bs[0] = bs[1];
                bs[1] = temp;
                temp = bs[2];
                bs[2] = bs[3];
                bs[3] = temp;
            }
            return bs;
        }
        public static float BytesToSingle(params byte[] data)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToSingle(new byte[] { data[1], data[0], data[3], data[2] }, 0);
            }
            else
            {
                return BitConverter.ToSingle(data, 0);
            }
        }
        /// <summary>
        /// 高位在前，低位在后
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] Int2ToBytes(Int16 value)
        {
            byte[] src = new byte[2];
            src[0] = (byte)((value >> 8) & 0xFF);
            src[1] = (byte)(value & 0xFF);
            return src;
        }
        public static byte[] Uint2ToBytes(UInt16 value)
        {
            byte[] src = new byte[2];
            src[0] = (byte)((value >> 8) & 0xFF);
            src[1] = (byte)(value & 0xFF);
            return src;
        }
        public static short BytesToInt16(params byte[] data)
        {
            return (short)(data[0] << 8 | data[1]);
        }
        public static UInt16 BytesToUInt16(params byte[] data)
        {
            return (UInt16)(data[0] << 8 | data[1]);
        }
        public static int BytesToInt(params byte[] data)
        {
            return (int)(data[2] << 24 | data[3] << 16 | data[0] << 8 | data[1]);
        }
        public static byte[] Int4ToBytes(Int32 value)
        {
            byte[] src = new byte[4];
            src[2] = (byte)((value >> 24) & 0xFF);
            src[3] = (byte)((value >> 16) & 0xFF);
            src[0] = (byte)((value >> 8) & 0xFF);
            src[1] = (byte)(value & 0xFF);
            return src;
        }
        public static String ToHex(params Byte[] vals)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < vals.Length; i++)
            {
                sb.Append(vals[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
