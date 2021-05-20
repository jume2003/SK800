using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SKABO.Common.Utils
{
    public static class Extensions
    {
        /// <summary>
        /// owner为新窗体的Owner
        /// </summary>
        /// <param name="win"></param>
        /// <param name="owner"></param>
        public static void ShowDialog(this Window win, Window owner)
        {
            win.Owner = owner;
            win.ShowDialog();
        }
        /// <summary>
        /// 设置程序主窗体为新窗体的owner
        /// </summary>
        /// <param name="win"></param>
        public static void ShowDialogEx(this Window win)
        {
            win.ShowDialog(Application.Current.MainWindow);
        }
        public static void ShowHint(this UIElement ele)
        {
            ele.Visibility = Visibility.Visible;
            DoubleAnimation da = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(2)));

            da.Completed += Da_Completed;
            Storyboard.SetTarget(da, ele);

            //开始动画
            ele.BeginAnimation(FrameworkElement.OpacityProperty, da);
        }
        public static void ShowHint(this UIElement ele,Window win,int time=2)
        {
            var ScaleX = ScreenUtil.DpiX / 96f;
            var ScaleY = ScreenUtil.DpiY / 96f;
            ScreenUtil.GetCursorPos(out ScreenUtil.POINT p);
            win.Left = p.X/ScaleX - win.Width / 2 - 10;
            win.Top = p.Y/ScaleY - win.Height;
            win.WindowStartupLocation = WindowStartupLocation.Manual;
            win.Show();
            win.Activate();
           
            DoubleAnimation da = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(time)));

            da.Completed += Da_Completed;
            Storyboard.SetTarget(da, win);

            //开始动画
            win.BeginAnimation(Window.OpacityProperty, da);
        }

        private static void Da_Completed(object sender, EventArgs e)
        {
            AnimationTimeline timeline = (sender as AnimationClock).Timeline;
            /* !!! 通过附加属性把UI对象取回 !!! */
            FrameworkElement uiElement = Storyboard.GetTarget(timeline) as FrameworkElement;
            if(uiElement is Window)
            {
                (uiElement as Window).Close();
            }
            else{ uiElement.Visibility = Visibility.Hidden; }
            
        }
        public static String SafeSqlLiteral(this String inputSQL)
        {
            inputSQL = inputSQL.Replace("[", "[[]"); // 这句话一定要在下面两个语句之前，否则作为转义符的方括号会被当作数据被再次处理 
            inputSQL = inputSQL.Replace("_", "[_]");
            inputSQL = inputSQL.Replace("%", "[%]");
            inputSQL = inputSQL.Replace("^", "[^]");
            inputSQL = inputSQL.Replace("'", "['']");
            return inputSQL;
        }
        public static T GetControl<T>(this DependencyObject uie, String propertyName) where T : FrameworkElement
        {
            var p = System.Windows.LogicalTreeHelper.FindLogicalNode(uie, propertyName);
            if (p == null) return default(T);
            return p as T;
        }
        public static IList<T> GetControls<T>(this DependencyObject uie) where T : FrameworkElement
        {

            IList<T> result = new List<T>();
            TakeControl<T>(uie, result);
            return result;
        }
        private static void TakeControl<T>(DependencyObject uie, IList<T> result)
        {
            var Children = System.Windows.LogicalTreeHelper.GetChildren(uie);
            if (Children == null) return;
            foreach (var item in Children)
            {
                if (item is T)
                {
                    result.Add((T)item);
                }
                else if(item is DependencyObject)
                {
                    TakeControl<T>((DependencyObject)item, result);
                }
            }
        }
        /// <summary>  

        /// 获取枚举变量值的 Description 属性  

        /// </summary>  

        /// <param name="obj">枚举变量</param>  

        /// <returns>如果包含 Description 属性，则返回 Description 属性的值，否则返回枚举变量值的名称</returns>  
        public static string GetDescription(this object obj)
        {

            return GetDescription(obj, false);

        }



        /// <summary>  

        /// 获取枚举变量值的 Description 属性  

        /// </summary>  

        /// <param name="obj">枚举变量</param>  

        /// <param name="isTop">是否改变为返回该类、枚举类型的头 Description 属性，而不是当前的属性或枚举变量值的 Description 属性</param>  

        /// <returns>如果包含 Description 属性，则返回 Description 属性的值，否则返回枚举变量值的名称</returns>  

        public static string GetDescription(this object obj, bool isTop)
        {

            if (obj == null)
            {

                return string.Empty;

            }

            try
            {

                Type _enumType = obj.GetType();
                DescriptionAttribute dna = null;

                if (isTop)
                {

                    dna = (DescriptionAttribute)Attribute.GetCustomAttribute(_enumType, typeof(DescriptionAttribute));

                }

                else
                {

                    FieldInfo fi = _enumType.GetField(Enum.GetName(_enumType, obj));

                    dna = (DescriptionAttribute)Attribute.GetCustomAttribute(

                       fi, typeof(DescriptionAttribute));

                }

                if (dna != null && string.IsNullOrEmpty(dna.Description) == false)
                    return dna.Description;

            }

            catch
            {

            }

            return obj.ToString();

        }
        /// <summary>
        /// 取得拼音首字母，不能识别多单字
        /// </summary>
        /// <param name="CnStr"></param>
        /// <returns></returns>
        public static string GetSpellCode(this string CnStr)
        {

            string strTemp = "";

            int iLen = CnStr.Length;

            int i = 0;

            for (i = 0; i <= iLen - 1; i++)
            {

                strTemp += GetCharSpellCode(CnStr.Substring(i, 1));
                //break;
            }

            return strTemp;

        }

        /// <summary>

        /// 得到一个汉字的拼音第一个字母，如果是一个英文字母则直接返回大写字母

        /// </summary>

        /// <param name="CnChar">单个汉字</param>

        /// <returns>单个大写字母</returns>

        private static string GetCharSpellCode(string CnChar)
        {

            long iCnChar;

            byte[] ZW = System.Text.Encoding.Default.GetBytes(CnChar);

            //如果是字母，则直接返回首字母

            if (ZW.Length == 1)
            {

                return CnChar.ToUpper();

            }
            else
            {

                // get the array of byte from the single char

                int i1 = (short)(ZW[0]);

                int i2 = (short)(ZW[1]);

                iCnChar = i1 * 256 + i2;

            }

            // iCnChar match the constant

            if ((iCnChar >= 45217) && (iCnChar <= 45252))
            {

                return "A";

            }

            else if ((iCnChar >= 45253) && (iCnChar <= 45760))
            {

                return "B";

            }
            else if ((iCnChar >= 45761) && (iCnChar <= 46317))
            {

                return "C";

            }
            else if ((iCnChar >= 46318) && (iCnChar <= 46825))
            {

                return "D";

            }
            else if ((iCnChar >= 46826) && (iCnChar <= 47009))
            {

                return "E";

            }
            else if ((iCnChar >= 47010) && (iCnChar <= 47296))
            {

                return "F";

            }
            else if ((iCnChar >= 47297) && (iCnChar <= 47613))
            {

                return "G";

            }
            else if ((iCnChar >= 47614) && (iCnChar <= 48118))
            {

                return "H";

            }
            else if ((iCnChar >= 48119) && (iCnChar <= 49061))
            {

                return "J";

            }
            else if ((iCnChar >= 49062) && (iCnChar <= 49323))
            {

                return "K";

            }
            else if ((iCnChar >= 49324) && (iCnChar <= 49895))
            {

                return "L";

            }
            else if ((iCnChar >= 49896) && (iCnChar <= 50370))
            {

                return "M";

            }
            else if ((iCnChar >= 50371) && (iCnChar <= 50613))
            {

                return "N";

            }
            else if ((iCnChar >= 50614) && (iCnChar <= 50621))
            {

                return "O";

            }
            else if ((iCnChar >= 50622) && (iCnChar <= 50905))
            {

                return "P";

            }
            else if ((iCnChar >= 50906) && (iCnChar <= 51386))
            {

                return "Q";

            }
            else if ((iCnChar >= 51387) && (iCnChar <= 51445))
            {

                return "R";

            }
            else if ((iCnChar >= 51446) && (iCnChar <= 52217))
            {

                return "S";

            }
            else if ((iCnChar >= 52218) && (iCnChar <= 52697))
            {

                return "T";

            }
            else if ((iCnChar >= 52698) && (iCnChar <= 52979))
            {

                return "W";

            }
            else if ((iCnChar >= 52980) && (iCnChar <= 53640))
            {

                return "X";

            }
            else if ((iCnChar >= 53689) && (iCnChar <= 54480))
            {

                return "Y";

            }
            else if ((iCnChar >= 54481) && (iCnChar <= 55289))
            {

                return "Z";

            }
            else

                return ("?");

        }
    }
}
