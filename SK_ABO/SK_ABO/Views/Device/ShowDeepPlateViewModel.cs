using SKABO.Common;
using SKABO.Common.Models.BJ;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using SKABO.Common.Utils;
using SKABO.ResourcesManager;

namespace SK_ABO.Views.Device
{
    class ShowDeepPlateViewModel:Screen
    {
        private String Letters ="ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public String Key { get; set; }
        public bool CanSave { get; set; } = true;
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            this.DisplayName = "深孔板信息";
            var MainPanel = (this.View as ShowDeepPlateView).MainPanel;
            if (Constants.BJDict.ContainsKey(Key))
            {
                var list = Constants.BJDict[Key];
                foreach (var item in list)
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
            }
            Label lab = new Label();
            lab.Content = "注意：蓝色表示已使用了！";
            lab.Margin = new System.Windows.Thickness(100, 10, 0, 0);
            lab.SetResourceReference(Label.StyleProperty, "labAlarm");
            MainPanel.Children.Add(lab);

        }
        private Grid InitGrid(VBJ bj)
        {
            var vbj = bj as VBJ;
            if (vbj == null) return null;
            Grid grid = new Grid();
            
            int X = vbj.Values.GetLength(0);
            int Y = vbj.Values.GetLength(1);
            for(int i = -1; i < X; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() {Width=new System.Windows.GridLength(i==-1?60:50) });
            }
            for (int i = -1; i < Y; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition() {  Height = new System.Windows.GridLength(35)  });
            }
            for (int i = -1; i < X; i++)
            {
                for (int j = -1; j < Y; j++)
                {
                    Border border = new Border();
                    if (i == -1 || j == -1)
                    {
                        border.Background = new SolidColorBrush(Color.FromRgb(0xF0, 0xF0, 0xF0));
                        border.Name = String.Format("border_{0}_{1}", i, j).Replace("-","Z");
                        
                            if(CanSave)
                                border.MouseLeftButtonDown += Border_MouseLeftButtonDown;
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
                            tb.Text = bj.Name;
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
                            border.Child=(tb);
                        }
                        else
                        {
                            string tip_msg = i + "," + j;
                            if (vbj.Values[i, j] is ResInfoData resinfo)
                            {
                                tip_msg += "\n" + resinfo.GetCodeAt(0) + "\n" + resinfo.GetCodeAt(1);
                            }
                            Ellipse ell = new Ellipse();
                            ell.Height = 30;
                            ell.Width = 30;
                            ell.ToolTip = tip_msg;
                            ell.StrokeThickness = 1;
                            ell.Stroke = Brushes.Black;
                            ell.Name = String.Format("ell_{0}_{1}", i, j);
                            ell.SetResourceReference(Ellipse.FillProperty, vbj.Values[i,j]==null? "BJFillUnUsed": "BJFillUsed");
                            ell.Tag = vbj.Values[i, j];
                            if (CanSave)
                            {
                                ell.MouseLeave += Ell_MouseLeave;
                                ell.MouseLeftButtonUp += Ell_MouseLeftButtonUp;
                            }
                            border.Child=ell;
                        }
                    }
                }
            }
            return grid;
        }

        private void Ell_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ChangeEllipse(sender);
            e.Handled = true;
        }
        private void ChangeEllipse(object sender)
        {
            if (sender is Ellipse ell)
            {
                var vals = ell.DataContext as object[,];
                ell.SetResourceReference(Ellipse.FillProperty, ell.Tag != null ? "BJFillUnUsed" : "BJFillUsed");
                var strs = ell.Name.Split('_');
                int x = int.Parse(strs[1]);
                int y = int.Parse(strs[2]);
                if (ell.Tag == null)
                {
                    ell.Tag = 1;
                    vals[x, y]= 1;
                }
                else
                {
                    ell.Tag = null;
                    vals[x, y] = null;
                }
            }
        }
        private void Ell_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                ChangeEllipse(sender);
                e.Handled = true;
            }
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(sender is Border border)
            {
                if(border.Parent is Grid gd)
                {
                    var vals = gd.DataContext as object[,];
                    var strs = border.Name.Split('_');
                    int x = int.Parse(strs[1].Replace("Z", "-"));
                    int y = int.Parse(strs[2].Replace("Z", "-"));

                    int LenX = vals.GetLength(0);
                    int LenY = vals.GetLength(1);
                    bool? Filled = null;
                    int MinX, MinY,MaxX,MaxY;
                    MinX = x == -1 ? 0 : x;
                    MinY = y == -1 ? 0 : y;
                    MaxX = x == -1 ? LenX - 1 : x;
                    MaxY = y == -1 ? LenY - 1 : y;
                    for(int cx = MinX; cx <= MaxX; cx++)
                    {
                        for(int cy = MinY; cy <= MaxY; cy++)
                        {
                            var ell = gd.GetControl<Ellipse>(String.Format("ell_{0}_{1}", cx, cy));
                            if (!Filled.HasValue)
                            {
                                Filled = ell.Tag != null;
                            }
                            if (Filled.Value)
                            {
                                ell.Tag = null;
                                vals[cx, cy] = null;
                            }
                            else
                            {
                                ell.Tag = 1;
                                vals[cx, cy] = 1;
                            }
                            ell.SetResourceReference(Ellipse.FillProperty, Filled.Value ? "BJFillUnUsed" : "BJFillUsed");
                        }
                    }
                }
            }
            e.Handled = true;
        }

        

        public void Close()
        {
            this.RequestClose();
        }
        public void Save()
        {
            var MainPanel = (this.View as ShowDeepPlateView).MainPanel;
            var gds= MainPanel.GetControls<Grid>();
            foreach(var gd in gds)
            {
                if (gd.DataContext != null)
                {
                    if(gd.Tag is VBJ vbj)
                    {
                        if(gd.DataContext is object[,] vals)
                        {
                            int X = vals.GetLength(0);
                            int Y = vals.GetLength(1);
                            for(int i = 0; i < X; i++)
                            {
                                for(int j = 0; j < Y; j++)
                                {
                                    if (vals[i, j] != null)
                                    {
                                        var resinfo_tem = new ResInfoData();
                                        resinfo_tem.Values = vbj.Values;
                                        resinfo_tem.SetCode("userfill");
                                        resinfo_tem.SetCode("userfill");
                                        vals[i, j] = resinfo_tem;
                                    }
                                    vbj.SetValue(i, j, vals[i, j]);
                                }
                            }
                        }
                    }
                }
            }
            this.RequestClose();
        }
    }
}
