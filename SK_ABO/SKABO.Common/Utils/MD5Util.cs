using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Utils
{
    public class MD5Util
    {
        private const String HEX_NUMS_STR = "0123456789ABCDEF";
        private const int SALT_LENGTH = 16;
        /// <summary>
        /// 获取加密后的字符串
        /// </summary>
        /// <param name="password">密码明文</param>
        /// <returns>加密后的密文</returns>
        public static String getEncryptedPwd(String password)
        {
            //声明加密后的口令数组变量   
            byte[] pwd = null;
            //随机数生成器   
            Random random = new Random();
            //声明盐数组变量   
            byte[] salt = new byte[SALT_LENGTH];
            //将随机数放入盐变量中   
            random.NextBytes(salt);

            //声明消息摘要对象   

            System.Security.Cryptography.MD5CryptoServiceProvider md5CSP = new System.Security.Cryptography.MD5CryptoServiceProvider();

            pwd = System.Text.Encoding.UTF8.GetBytes(password);
            var bb = salt.Concat(pwd);
            byte[] resultEncrypt = md5CSP.ComputeHash(salt.Concat(pwd).ToArray());

            return byteToHexString(resultEncrypt.Concat(salt).ToArray());
        }
        /// <summary>
        /// 验证口令是否合法
        /// </summary>
        /// <param name="password">用户输入的密码明文</param>
        /// <param name="passwordInDb">数据库中存放的密码密文</param>
        /// <returns>true:密码合法  fasle:非法,用户输入密码与数据库存放密码不一致</returns>
        public static bool validPassword(String password, String passwordInDb)
        {
            byte[] pwdInDb = hexStringToByte(passwordInDb);
            byte[] salt = new byte[SALT_LENGTH];
            if (pwdInDb.Length < SALT_LENGTH)
            {
                return false;
            }
            Array.Copy(pwdInDb, pwdInDb.Length - SALT_LENGTH, salt, 0, SALT_LENGTH);
            //创建消息摘要对象   
            System.Security.Cryptography.MD5CryptoServiceProvider md5CSP = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var pwd = System.Text.Encoding.UTF8.GetBytes(password);
            byte[] resultEncrypt = md5CSP.ComputeHash(salt.Concat(pwd).ToArray());
            byte[] pindb = new byte[pwdInDb.Length - SALT_LENGTH];
            Array.Copy(pwdInDb, 0, pindb, 0, pindb.Length);

            //比较根据输入口令生成的消息摘要和数据库中消息摘要是否相同   
            if (ByteEquals(resultEncrypt, pindb))
            {
                //口令正确返回口令匹配消息   
                return true;
            }
            else
            {
                //口令不正确返回口令不匹配消息   
                return false;
            }
        }
        public static bool validRegisterKey(String machineCode, String RangeValue, String RegisterKey)
        {
            char[] charBuffer = machineCode.ToCharArray();
            byte[] bytes = null;
            bytes = Convert.FromBase64CharArray(charBuffer, 0, charBuffer.Length);
            var code = Encoding.UTF8.GetString(bytes);
            return MD5Util.validPassword(code + RangeValue, RegisterKey);
        }
        static bool ByteEquals(byte[] b1, byte[] b2)
        {
            if (b1.Length != b2.Length) return false;
            if (b1 == null || b2 == null) return false;
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i])
                    return false;
            }
            return true;
        }
        /**   
         * 将16进制字符串转换成字节数组   
         * @param hex   
         * @return   
         */
        public static byte[] hexStringToByte(String hex)
        {
            int len = (hex.Length / 2);
            byte[] result = new byte[len];
            char[] hexChars = hex.ToCharArray();
            for (int i = 0; i < len; i++)
            {
                int pos = i * 2;
                result[i] = (byte)(HEX_NUMS_STR.IndexOf(hexChars[pos]) << 4
                                | HEX_NUMS_STR.IndexOf(hexChars[pos + 1]));
            }
            return result;
        }


        /**  
         * 将指定byte数组转换成16进制字符串  
         * @param b  
         * @return  
         */
        public static String byteToHexString(byte[] b)
        {
            StringBuilder hexString = new StringBuilder();
            for (int i = 0; i < b.Length; i++)
            {
                String hex = (b[i] & 0xFF).ToString("X2");
                hexString.Append(hex.ToUpper());
            }
            return hexString.ToString();
        }
        public static String GeneratePWDByDate(String dateStr)
        {
            if (String.IsNullOrEmpty(dateStr)) return GeneratePWDByDate(DateTime.Today);
            DateTime date;
            if(DateTime.TryParse(dateStr,out date))
            {
                return GeneratePWDByDate(date);
            }
            return GeneratePWDByDate(DateTime.Today);
        }
        public static String GeneratePWDByDate(DateTime? date)
        {
            String chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (!date.HasValue)
            {
                date = DateTime.Today;
            }
            String str = date.Value.ToString("yyyyddMM");
            int val = int.Parse(str);
            var w = (Int32)(date.Value.DayOfWeek);
            int wn = date.Value.DayOfYear;
            long ft=date.Value.ToFileTime();
            long wft = date.Value.ToFileTimeUtc();
            int len = chars.Length;
            int index1 =(int)( ft % len);
            int index2 = val * 34 % len;
            int index3 = wn * date.Value.Year % len;
            int index4 = (w+1) * date.Value.Year * date.Value.Month * date.Value.Day % len;
            switch (date.Value.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    {
                        str =String.Format("{0}{1}{2}{3}", chars.ElementAt(index1) , chars.ElementAt(index2) , chars.ElementAt(index3) , chars.ElementAt(index4));
                        break;
                    }
                case DayOfWeek.Tuesday:
                    {
                        str = String.Format("{2}{1}{0}{3}", chars.ElementAt(index1), chars.ElementAt(index2), chars.ElementAt(index3), chars.ElementAt(index4));
                        break;
                    }
                case DayOfWeek.Wednesday:
                    {
                        str = String.Format("{0}{3}{2}{1}", chars.ElementAt(index1), chars.ElementAt(index2), chars.ElementAt(index3), chars.ElementAt(index4));
                        break;
                    }
                case DayOfWeek.Thursday:
                    {
                        str = String.Format("{3}{1}{0}{2}", chars.ElementAt(index1), chars.ElementAt(index2), chars.ElementAt(index3), chars.ElementAt(index4));
                        break;
                    }
                case DayOfWeek.Friday:
                    {
                        str = String.Format("{1}{3}{2}{0}", chars.ElementAt(index1), chars.ElementAt(index2), chars.ElementAt(index3), chars.ElementAt(index4));
                        break;
                    }
                case DayOfWeek.Saturday:
                    {
                        str = String.Format("{0}{3}{2}{1}", chars.ElementAt(index1), chars.ElementAt(index2), chars.ElementAt(index3), chars.ElementAt(index4));
                        break;
                    }
                case DayOfWeek.Sunday:
                    {
                        str = String.Format("{2}{3}{1}{0}", chars.ElementAt(index1), chars.ElementAt(index2), chars.ElementAt(index3), chars.ElementAt(index4));
                        break;
                    }
            }
            Console.WriteLine("工厂密码:" + str);
            return str;
        }
        public string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, System.IO.FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
    }
}
