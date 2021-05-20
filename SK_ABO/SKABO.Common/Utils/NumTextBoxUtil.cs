using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace SKABO.Common.Utils
{
    public class NumTextBoxUtil
    {
        public static void TextBox_Number_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox)
            {
                TextBox txt = sender as TextBox;
                TextBox_Number_KeyDown(txt.Text, e);
            }
            else if (sender is ComboBox)
            {
                var txt = sender as ComboBox;
                TextBox_Number_KeyDown(txt.Text, e);
            }
        }
        private static void TextBox_Number_KeyDown(String Text, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                return;
            }
            //屏蔽非法按键
            if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Decimal)
            {
                if (Text.Contains(".") && e.Key == Key.Decimal)
                {
                    e.Handled = true;
                    return;
                }
                e.Handled = false;
            }
            else if (((e.Key >= Key.D0 && e.Key <= Key.D9) || e.Key == Key.OemPeriod) && e.KeyboardDevice.Modifiers != ModifierKeys.Shift)
            {
                if (Text.Contains(".") && e.Key == Key.OemPeriod)
                {
                    e.Handled = true;
                    return;
                }
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
        public static void TextBox_Number_TextChanged(object sender, TextChangedEventArgs e)
        {
            //屏蔽中文输入和非法字符粘贴输入
            TextBox textBox = sender as TextBox;
            TextChange[] change = new TextChange[e.Changes.Count];
            e.Changes.CopyTo(change, 0);

            int offset = change[0].Offset;
            if (change[0].AddedLength > 0)
            {
                double num = 0;
                if (!Double.TryParse(textBox.Text, out num))
                {
                    textBox.Text = textBox.Text.Remove(offset, change[0].AddedLength);
                    textBox.Select(offset, 0);
                }
            }
        }
        public static bool NumMax(object sender, int MaxVal)
        {
            TextBox textBox = sender as TextBox;
            int val = 0;
            if (int.TryParse(textBox.Text.Trim(), out val))
            {
                if (val <= MaxVal)
                {
                    return true;
                }
                else
                {
                    textBox.Text = textBox.Text.Substring(0, textBox.Text.Length - 1);
                    return false;
                }
            }
            return true;
        }
    }
}
