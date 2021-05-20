using SK_ABO.UserCtrls.DeviceParam;
using SKABO.Common;
using SKABO.Common.UserCtrls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SK_ABO.Pages.Device
{
    /// <summary>
    /// JYQParameterView.xaml 的交互逻辑
    /// </summary>
    public partial class JYQParameterView : Page
    {
        public JYQParameterView()
        {
            InitializeComponent();
        }
        private void BindingValue(String Path, DependencyProperty TargetProperty,FrameworkElement Target)
        {
            Binding bindingValue = new Binding(Path) {  Mode = BindingMode.TwoWay };
            
            bindingValue.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            Target.SetBinding(TargetProperty, bindingValue);
        }
        /// <summary>
        /// 绘制是否装有针设置组件
        /// </summary>
        private void PaintTip()
        {
            for(byte i = 0; i < Constants.EntercloseCount; i++)
            {
                Label lab = new Label() { Content = String.Format("{0}# 是否上针", i + 1) };
                TextBox text = new TextBox();
                BindingValue(String.Format("injector.Logic{0}.ExistTipCoil.Addr",i), TextBox.TextProperty, text);
                CheckBox sval = new CheckBox() { IsEnabled = false };
                BindingValue(String.Format("injector.Logic{0}.ExistTipCoil.SetValue", i), CheckBox.IsCheckedProperty, sval);
                CheckBox cval = new CheckBox() { IsEnabled = false };
                BindingValue(String.Format("injector.Logic{0}.ExistTipCoil.CurrentValue", i), CheckBox.IsCheckedProperty, cval);
                this.TipLablePanel.Children.Add(lab);
                this.TipAddrPanel.Children.Add(text);
                this.TipSetValuePanel.Children.Add(sval);
                this.TipCurrentValuePanel.Children.Add(cval);
            }
        }
        /// <summary>
        /// 绘制启用通道设置组件
        /// </summary>
        private void PaintLogicE()
        {
            
            //<CheckBox Content="1#" IsChecked="{Binding injector.Logic0.Valid,Mode=TwoWay}" Style="{DynamicResource chkLogic}" />
            for (byte i = 0; i < Constants.EntercloseCount; i++)
            {
                StackPanel content = new StackPanel() { Orientation= Orientation .Horizontal};
                CheckBox InjEnable_Check = new CheckBox() { Width = 40, ToolTip = "是否启用" };
                NumericUpDown_Control TipDis_Cotr = new NumericUpDown_Control() { MaxValue = 10000,MinValue=-10000, Step =1,Decimation=2,Width= 70,ToolTip= "通道吸管的偏移" };
                NumericUpDown_Control YZero_Cotr = new NumericUpDown_Control() { MaxValue = 10000, Step = 1, Decimation = 2, Width = 70, ToolTip = "Y轴零点的偏移" };
                NumericUpDown_Control InjWidth_Cotr = new NumericUpDown_Control() { MaxValue = 10000, Step = 1, Decimation = 2, Width = 70, ToolTip = "滑块宽度" };

                TipDis_Cotr.Style = this.FindResource("num_up_down") as Style;
                InjWidth_Cotr.Style = YZero_Cotr.Style = TipDis_Cotr.Style;
                BindingValue(String.Format("injector.Logic{0}.InjEnable", i), CheckBox.IsCheckedProperty, InjEnable_Check);
                BindingValue(String.Format("injector.Logic{0}.TipDis", i), NumericUpDown_Control.ValueProperty, TipDis_Cotr);
                BindingValue(String.Format("injector.Logic{0}.YZero", i), NumericUpDown_Control.ValueProperty, YZero_Cotr);
                BindingValue(String.Format("injector.Logic{0}.InjWidth", i), NumericUpDown_Control.ValueProperty, InjWidth_Cotr);
                content.Children.Add(InjEnable_Check);
                content.Children.Add(TipDis_Cotr);
                content.Children.Add(YZero_Cotr);
                content.Children.Add(InjWidth_Cotr);
                EnterPanel.Children.Add(content);
            }
        }
        /// <summary>
        /// 绘制液面探测设置组件
        /// </summary>
        private void PaintLiquid()
        {
            for (byte i = 0; i < Constants.EntercloseCount; i++)
            {
                //<Label Content="1# 开始探测"/>
                 //                       < Label Content = "1# 是否探测到" />
                 Label lab1 = new Label() { Content = String.Format("{0}# 压力", i + 1) };
                TextBox text1 = new TextBox();
                BindingValue(String.Format("injector.Logic{0}.Pressure.Addr", i), TextBox.TextProperty, text1);
                NumericUpDown_Control sval1 = new NumericUpDown_Control() { MaxValue = 900, Step = 1, Decimation = 0 };
                
                BindingValue(String.Format("injector.Logic{0}.Pressure.SetValue", i), NumericUpDown_Control.ValueProperty, sval1);
                TextBox cval1 = new TextBox() { IsEnabled=false};
                BindingValue(String.Format("injector.Logic{0}.Pressure.CurrentValue", i), TextBox.TextProperty, cval1);
                this.LiquidLabelPanel.Children.Add(lab1);
                this.LiquidSetValuePanel.Children.Add(sval1);
            }
        }
        private void PaintMotor()
        {
            //<DeviceParam:Electromotor_Control Header="1#通道 Z轴参数" Motor="{Binding injector.Logic0.ZMotor}"/>
            //<DeviceParam:Electromotor_Control Header="1#通道 吸液泵参数" Motor="{Binding injector.Logic0.PumpMotor}"/>
            for (byte i = 0; i < Constants.EntercloseCount; i++)
            {
                DoubleSpeedMotor_control zmotor = new DoubleSpeedMotor_control() { Header = String.Format("{0}# Z轴参数", i + 1) };
                DoubleSpeedMotor_control pumpMotor = new DoubleSpeedMotor_control() { Header = String.Format("{0}# 吸液泵参数", i + 1) };
                DoubleSpeedMotor_control yMotor = new DoubleSpeedMotor_control() { Header = String.Format("{0}# Y轴参数", i + 1) };
                BindingValue($"injector.Logic{i}.ZMotor", DoubleSpeedMotor_control.MotorProperty, zmotor);
                BindingValue($"injector.Logic{i}.PumpMotor", DoubleSpeedMotor_control.MotorProperty, pumpMotor);
                BindingValue($"injector.Logic{i}.YMotor", DoubleSpeedMotor_control.MotorProperty, yMotor);
                this.zMotorPanel.Children.Add(zmotor);
                this.pumpPanel.Children.Add(pumpMotor);
                Ypanel.Children.Add(yMotor);
            }
        }
        private void Page_Initialized(object sender, EventArgs e)
        {
            PaintLogicE();
            PaintTip();
            PaintLiquid();
            PaintMotor();
        }
    }
}
