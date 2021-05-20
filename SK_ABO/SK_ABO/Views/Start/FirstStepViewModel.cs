using SKABO.Common;
using SKABO.Common.Models.BJ;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;

namespace SK_ABO.Views.Start
{
    public class FirstStepViewModel:Screen
    {
        #region 隐藏关闭按钮
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        #endregion 隐藏关闭按钮

        private String Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private bool CanSave = true;
        String Key = "T_BJ_Tip";
        public int CurrentX, CurrentY;
        /// <summary>
        /// Tip盒索引
        /// </summary>
        public int TipIndex { get; set; }
        protected override void OnViewLoaded()
        {
            var hwnd = new WindowInteropHelper(this.View as Window).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            this.DisplayName = "选择Tip头开始位置";
            base.OnViewLoaded();
            var MainPanel = (this.View as FirstStepView).MainPanel;
            var TipPanel = (this.View as FirstStepView).TipPanel;

            if (Constants.BJDict.ContainsKey(Key))
            {
                var list = Constants.BJDict[Key];
                int i = 0;
                foreach (var item in list)
                {
                    i++;
                    if (i == 1)
                    {
                        var dg = InitGrid(item);
                        if (dg != null)
                        {
                            if (item is VBJ vbj)
                            {
                                object[,] vals = (object[,])vbj.Values.Clone();
                                dg.DataContext = vals;
                                dg.Tag = item;
                            }
                            if (MainPanel.Children.Count != 0)
                            {
                                dg.Margin = new System.Windows.Thickness(0, 10, 0, 0);
                            }
                            MainPanel.Children.Add(dg);
                        }
                    }
                    RadioButton radio = new RadioButton()
                    {
                        GroupName = "Tip_Radio",
                        Content = item.Name,
                        Margin = i == 0 ? new System.Windows.Thickness(0, 20, 0, 0) : new System.Windows.Thickness(50, 20, 0, 0),
                        Tag = i - 1
                    };
                    radio.Checked += Radio_Checked;
                    radio.SetResourceReference(RadioButton.StyleProperty,"BoxRadioButton");
                    if (i == 1) radio.IsChecked = true;
                    TipPanel.Children.Add(radio);
                }
            }
            Label lab = new Label();
            lab.Content = "注意：蓝色表示存在Tip头可用！";
            lab.Margin = new System.Windows.Thickness(100, 10, 0, 0);
            lab.SetResourceReference(Label.StyleProperty, "labAlarm");
            MainPanel.Children.Add(lab);
        }

        private void Radio_Checked(object sender, RoutedEventArgs e)
        {
            if(sender is RadioButton btn)
            {
                TipIndex = Convert.ToInt32(btn.Tag);
            }
        }

        private Grid InitGrid(VBJ bj)
        {
            var vbj = bj as VBJ;
            if (vbj == null) return null;
            Grid grid = new Grid();

            int X = vbj.Values.GetLength(0);
            int Y = vbj.Values.GetLength(1);
            for (int i = -1; i < X; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new System.Windows.GridLength(i == -1 ? 60 : 50) });
            }
            for (int i = -1; i < Y; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = new System.Windows.GridLength(35) });
            }
            for (int i = -1; i < X; i++)
            {
                for (int j = -1; j < Y; j++)
                {
                    Border border = new Border();
                    if (i == -1 || j == -1)
                    {
                        border.Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0));
                        border.Name = String.Format("border_{0}_{1}", i, j).Replace("-", "Z");
                    }
                    border.SetValue(Grid.RowProperty, j + 1);
                    border.SetValue(Grid.ColumnProperty, i + 1);
                    border.BorderBrush = Brushes.Black;
                    border.BorderThickness = new System.Windows.Thickness(1);
                    grid.Children.Add(border);
                    if (i == -1)
                    {
                        TextBlock tb = new TextBlock();
                        if (j == -1)
                        {
                            //tb.Text = bj.Name;
                        }
                        else
                        {
                            tb.Text = Letters.ElementAt(j).ToString();
                        }
                        tb.TextAlignment = System.Windows.TextAlignment.Center;
                        tb.Margin = new System.Windows.Thickness(0, 5, 0, 0);
                        border.Child = tb;
                    }
                    else
                    {
                        if (j == -1)
                        {
                            TextBlock tb = new TextBlock();
                            tb.Margin = new System.Windows.Thickness(0, 5, 0, 0);
                            tb.Text = (i + 1).ToString();
                            tb.TextAlignment = System.Windows.TextAlignment.Center;
                            border.Child = (tb);
                        }
                        else
                        {
                            Ellipse ell = new Ellipse();
                            ell.Height = 30;
                            ell.Width = 30;
                            ell.ToolTip = String.Format("{0}{1}", Letters.ElementAt(j), i + 1);
                            ell.StrokeThickness = 1;
                            ell.Stroke = Brushes.Black;
                            ell.Name = String.Format("ell_{0}_{1}", i, j);
                            ell.SetResourceReference(Ellipse.FillProperty, vbj.Values[i, j] == null ? "BJFillUnUsed" : "BJFillUsed");
                            ell.Tag = vbj.Values[i, j];
                            if (CanSave)
                            {
                                ell.MouseLeftButtonUp += Ell_MouseLeftButtonUp;
                            }
                            border.Child = ell;
                        }
                    }
                }
            }
            return grid;
        }

        private void Ell_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CanConfirm = true;
            ChangeEllipse(sender);
            e.Handled = true;
        }

        private void ChangeEllipse(object sender)
        {
            if (sender is Ellipse ell)
            {
                if (ell.Parent is Border border)
                {
                    if (border.Parent is Grid gd)
                    {
                        var vals = ell.DataContext as object[,];
                        var strs = ell.Name.Split('_');
                        CurrentX = int.Parse(strs[1]);
                        CurrentY = int.Parse(strs[2]);

                        String Key = "T_BJ_Tip";
                        if (Constants.BJDict.ContainsKey(Key))
                        {
                            var list = Constants.BJDict[Key];
                            var item = list[TipIndex];
                            if (item is VBJ vbj)
                            {
                                //vbj.Values[CurrentX, CurrentY] = vbj.Values[CurrentX, CurrentY] != null ? null : (object)1;
                                //var e = gd.GetControl<Ellipse>(String.Format("ell_{0}_{1}", CurrentX, CurrentY));
                                //e.SetResourceReference(Ellipse.FillProperty, (vbj.Values[CurrentX, CurrentY] == null) ? "BJFillUnUsed" : "BJFillUsed");
                                int MaxX = vals.GetLength(0);
                                int MaxY = vals.GetLength(1);
                                for (int i = 0; i < MaxX; i++)
                                {
                                    for (int j = 0; j < MaxY; j++)
                                    {
                                        vbj.Values[i, j] = (i < CurrentX || (i == CurrentX && j < CurrentY)) ? null : (object)1;
                                        var e = gd.GetControl<Ellipse>(String.Format("ell_{0}_{1}", i, j));
                                        e.SetResourceReference(Ellipse.FillProperty, vbj.Values[i, j]==null ? "BJFillUnUsed" : "BJFillUsed");
                                    }
                                }

                            }
                        }
                    }
                }
            }
        }
        public bool CanConfirm { get; set; } = true;
        public void Confirm()
        {
            this.RequestClose(true);
        }
    }
}
