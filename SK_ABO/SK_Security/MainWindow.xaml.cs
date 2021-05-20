using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SK_Security
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        bool beginMove = false;//初始化鼠标位置  
        double currentXPosition;
        double currentYPosition;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            labDate.Content = DateTime.Today.ToString("yyyy-MM-dd");
            txtPwd.Text = GeneratePWDByDate(DateTime.Today);
        }
        public String GeneratePWDByDate(DateTime? date)
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
            long ft = date.Value.ToFileTime();
            long wft = date.Value.ToFileTimeUtc();
            int len = chars.Length;
            int index1 = (int)(ft % len);
            int index2 = val * 34 % len;
            int index3 = wn * date.Value.Year % len;
            int index4 = (w + 1) * date.Value.Year * date.Value.Month * date.Value.Day % len;
            switch (date.Value.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    {
                        str = String.Format("{0}{1}{2}{3}", chars.ElementAt(index1), chars.ElementAt(index2), chars.ElementAt(index3), chars.ElementAt(index4));
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

            return str;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Button btn = sender as Button;
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var p = e.GetPosition(btn);
                var ScreenP = this.PointToScreen(p);
                beginMove = true;
                currentXPosition = ScreenP.X;
                currentYPosition = ScreenP.Y;//鼠标的y坐标为当前窗体左上角y坐标  
            }
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (beginMove)
            {
                Button btn = sender as Button;
                var p = e.GetPosition(btn);
                var ScreenP = this.PointToScreen(p);
                this.Left += ScreenP.X - currentXPosition;//根据鼠标x坐标确定窗体的左边坐标x  
                this.Top += ScreenP.Y - currentYPosition;//根据鼠标的y坐标窗体的顶部，即Y坐标  
                currentXPosition = ScreenP.X;
                currentYPosition = ScreenP.Y;
            }
        }

        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Released)
            {
                currentXPosition = 0; //设置初始状态  
                currentYPosition = 0;
                beginMove = false;
            }
        }
    }
}
