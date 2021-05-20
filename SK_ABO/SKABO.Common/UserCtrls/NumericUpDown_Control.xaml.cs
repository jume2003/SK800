using SKABO.Common.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

namespace SKABO.Common.UserCtrls
{
    /// <summary>
    /// NumericUpDown_Control.xaml 的交互逻辑
    /// </summary>
    public partial class NumericUpDown_Control : UserControl
    {
        /// <summary>
        /// 不知什么原因，Value属性在Xaml中绑定没有效果，要代码Binding
        /// 2018-04-13找到原因，在控件中设置了DataContext为self,所以不成功,现在页面可以正常绑定
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="Source"></param>
        public void BindingValue(String Path,Object Source)
        {
            Binding bindingValue = new Binding(Path) { Source = Source, Mode = BindingMode.TwoWay };
            bindingValue.Converter = new DecimationConverter();
            bindingValue.ConverterParameter = Decimation;
            bindingValue.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            SetBinding(NumericUpDown_Control.ValueProperty, bindingValue);
        }
        /// <summary>
        /// 不知什么原因，Value属性在Xaml中绑定没有效果，要代码Binding
        /// 2018-04-13找到原因，在控件中设置了DataContext为self,所以不成功,现在页面可以正常绑定
        /// </summary>
        /// <param name="Source"></param>
        public void BindingValue( Object Source)
        {
            if (this.Tag == null) return;
            BindingValue(this.Tag.ToString(), Source);
        }
        /// <summary>
        /// Dependency Object for the value of the UpDown Control
        /// </summary>
        public static  readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(NumericUpDown_Control), new FrameworkPropertyMetadata(0.0, ValuePropertyChangeCallback, CoerceValidateValue), validateValue);

        /// <summary>
        /// Dependency Object for the Minimal Value of the UpDown Control
        /// </summary>
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(double), typeof(NumericUpDown_Control), new FrameworkPropertyMetadata(0.0, MinMaxValueCallback));

        /// <summary>
        /// Dependency Object for the Maximal Value of the UpDown Control
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(double), typeof(NumericUpDown_Control), new FrameworkPropertyMetadata(100.0, MinMaxValueCallback));

        /// <summary>
        /// Dependency Object for the Maximal Value of the UpDown Control
        /// </summary>
        public static readonly DependencyProperty DecimationProperty =
            DependencyProperty.Register("Decimation", typeof(uint), typeof(NumericUpDown_Control), new FrameworkPropertyMetadata(0U, DecimationCallback));

        /// <summary>
        /// Dependency Object for the Step Value of the UpDown Control
        /// </summary>
        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register("Step", typeof(double), typeof(NumericUpDown_Control), new FrameworkPropertyMetadata(1.0));

        /// <summary>
        /// Dependency Object for the state of visibility of the UpDown Buttons
        /// </summary>
        public static readonly DependencyProperty ShowButtonsProperty =
            DependencyProperty.Register("ShowButtons", typeof(bool), typeof(NumericUpDown_Control), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Event Definition Value Change
        /// </summary>
        public static RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumericUpDown_Control));

        /// <summary>
        /// Event fired when value changes
        /// </summary>
        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        /// <summary>
        /// Event Helper Function when Value is changed
        /// </summary>
        protected virtual void OnValueChanged()
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = ValueChangedEvent;
            RaiseEvent(args);
        }

        /// <summary>
        /// Minimal possible value of the control
        /// </summary>
        double _minValue = double.MinValue;

        /// <summary>
        /// Maximum possible value of the control
        /// </summary>
        double _maxValue = double.MaxValue;

        /// <summary>
        /// Default Constructor (nothing special here x) )
        /// </summary>
        public NumericUpDown_Control()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Destroys the Class (sets thispointer null)
        /// </summary>
        ~NumericUpDown_Control()
        {
        }
        /// <summary>
        /// Specifies / Reads the number of digits shown after the decimal point
        /// </summary>
        public uint Decimation
        {
            get
            {
                return (uint)GetValue(DecimationProperty);
            }
            set
            {
                SetValue(DecimationProperty, value);
                SetDecimationBinding(value);
            }
        }

        /// <summary>
        /// Gets / Sets the value that the control is showing
        /// </summary>
        /// <exception cref="ArgumentException" />
        /// <remarks>If Value exceeds <see cref="MaxValue"/> or falls below <see cref="MinValue"/>, an <see cref="ArgumentException"/> is thrown</remarks>
        public double Value
        {
            get
            {
                return (double)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        /// <summary>
        /// Specifies / Reads weather the UpDown Buttons are to be shown
        /// </summary>
        public bool ShowButtons
        {
            get
            {
                return (bool)GetValue(ShowButtonsProperty);
            }
            set
            {
                SetValue(ShowButtonsProperty, value);
            }
        }

        /// <summary>
        /// Gets / Sets the minimal value of the control's value
        /// </summary>
        public double MinValue
        {
            get
            {
                return (double)GetValue(MinValueProperty);
            }
            set
            {
                SetValue(MinValueProperty, value);
            }
        }

        /// <summary>
        /// Gets / Sets the maximal value of the control's value
        /// </summary>
        public double MaxValue
        {
            get
            {
                return (double)GetValue(MaxValueProperty);
            }
            set
            {
                SetValue(MaxValueProperty, value);
            }
        }

        /// <summary>
        /// Gets / Sets the step size (increment / decrement size) of the control's value
        /// </summary>
        public double Step
        {
            get
            {
                return (double)GetValue(StepProperty);
            }
            set
            {
                SetValue(StepProperty, value);
            }
        }


        /// <summary>
        /// Increments the control's value by the value defined by <see cref="Step"/>
        /// </summary>
        /// <remarks>The value doesn't increment over MaxValue or under MinValue</remarks>
        public void Increment()
        {
            try
            {
                Value += Step;
                if (Value > _maxValue)
                {
                    Value = _maxValue;
                }
                if (Value < _minValue)
                {
                    Value = _minValue;
                }
            }
            catch (ArgumentException)
            {
            }
        }

        /// <summary>
        /// Decrements the control's value by the value defined by <see cref="Step"/>
        /// </summary>
        /// <remarks>The value doesn't increment over MaxValue or under MinValue</remarks>
        public void Decrement()
        {
            try
            {
                Value -= Step;
                if (Value > _maxValue)
                {
                    Value = _maxValue;
                }
                if (Value < _minValue)
                {
                    Value = _minValue;
                }
            }
            catch (ArgumentException e)
            {
            }
        }


        /// <summary>
        /// Validation function for the value.
        /// Checks weather Value is inbetween <see cref="MinValue"/> and <see cref="MaxValue"/>
        /// </summary>
        /// <param name="value">The current value of the Dependency Property</param>
        /// <returns><list type="bullet"><item><term>true</term><description>The Value is inbetween <see cref="MinValue"/> and <see cref="MaxValue"/></description></item><item><term>false</term><description>The value is out of bounds</description></item></list></returns>
        static bool validateValue(object value)
        {
            return true;
        }

        /// <summary>
        /// Handler for the Up Button Click.
        /// Increments the <see cref="Value"/> by <see cref="Step"/>
        /// </summary>
        /// <param name="sender">The Up Button Control</param>
        /// <param name="e"></param>
        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            Increment();
        }

        /// <summary>
        /// Handler for the Down Button Click.
        /// Decrements the <see cref="Value"/> by <see cref="Step"/>
        /// </summary>
        /// <param name="sender">The Down Button Control</param>
        /// <param name="e"></param>
        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            Decrement();
        }

        /// <summary>
        /// Sets the decimation binding.
        /// </summary>
        /// <param name="decimation">The decimation.</param>
        private void SetDecimationBinding(uint decimation)
        {
            Binding bindingValue = new Binding("Value") { Source=this,Mode=BindingMode.TwoWay};
            bindingValue.Converter = new DecimationConverter();
            bindingValue.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bindingValue.ConverterParameter = decimation;
            bindingValue.ValidationRules.Add(new ExceptionValidationRule());
            tbValue.SetBinding(TextBox.TextProperty, bindingValue);
        }
        private static Object CoerceValidateValue(DependencyObject d, object value)
        {
            if (d != null)
            {
                var _this = d as NumericUpDown_Control;
                double NewValue = Convert.ToDouble(value);
                if (_this.MinValue > NewValue)
                {
                    value= _this.MinValue;
                    _this.Value= _this.MinValue;//如果没有这一步，Binding在上的数据源不正确，还是原来的值
                }
                else if (_this.MaxValue < NewValue)
                {
                    value= _this.MaxValue;
                    _this.Value = _this.MaxValue;//如果没有这一步，Binding在上的数据源不正确，还是原来的值
                }
            }
            else
            {
                value= 0;
            }
            return value;
        }
        /// <summary>
        /// Change Event for Value
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void ValuePropertyChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            if (d != null)
            {
                var _this = d as NumericUpDown_Control;
                double NewValue = Convert.ToDouble(e.NewValue);
                double OldValue = Convert.ToDouble(e.OldValue);
                
                if (NewValue != OldValue)
                {
                    _this.OnValueChanged();
                }
            }
        }

        /// <summary>
        /// Change Event for Min and Max Value
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void MinMaxValueCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = d as NumericUpDown_Control;
            if (e.Property == MinValueProperty)
            {
                _this._minValue = Convert.ToDouble(e.NewValue);
            }
            else if (e.Property == MaxValueProperty)
            {
                _this._maxValue = Convert.ToDouble(e.NewValue);
            }
        }

        /// <summary>
        /// Change Event for Decimation
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void DecimationCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = d as NumericUpDown_Control;
            _this.SetDecimationBinding(Convert.ToUInt32(e.NewValue));

            
                _this.Value++;
                _this.Value--;
        }

        /// <summary>
        /// Handles the Loaded event of the ucNumericUpDown control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ucNumericUpDown_Loaded(object sender, RoutedEventArgs e)
        {
            SetDecimationBinding(Decimation);
        }

        private void tbValue_KeyDown(object sender, KeyEventArgs e)
        {
            NumTextBoxUtil.TextBox_Number_KeyDown(sender, e);
        }

        private void tbValue_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Value > _maxValue)
            {
                Value = _maxValue;
            }
            if (Value < _minValue)
            {
                Value = _minValue;
            }
        }
    }

    /// <summary>
    /// Value Conversion Class for the button height.
    /// Divides the Hight of the control by two to get the height for one button.
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class BtnHeightConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value / 2.0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Value Converter for the Button Show Property.
    /// Converts from <see cref="bool"/> to <see cref="System.Windows.Visibility"/>
    /// </summary>
    [ValueConversion(typeof(bool), typeof(System.Windows.Visibility))]
    public class BtnShowConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToBoolean(value))
            {
                return System.Windows.Visibility.Visible;
            }
            else
            {
                return System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Value Converter for the Button Show Property.
    /// Converts from <see cref="bool"/> to <see cref="System.Windows.Visibility"/>
    /// </summary>
    [ValueConversion(typeof(bool), typeof(System.Windows.Visibility))]
    public class BtnShowGridConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToBoolean(value))
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for the Text in the Value Text Box.
    /// Makes sure that the correct decimation is displayed
    /// </summary>
    [ValueConversion(typeof(double), typeof(string))]
    public class DecimationConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dataValue = System.Convert.ToDouble(value);

            return dataValue.ToString("F" + ((uint)parameter).ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return System.Convert.ToDouble(value);
            }
            catch (Exception)
            {
                return value;
            }
        }
    }
}