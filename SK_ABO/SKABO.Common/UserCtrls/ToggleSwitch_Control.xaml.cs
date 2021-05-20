using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SKABO.Common.UserCtrls
{
    /// <summary>
    /// ToggleSwitch_Control.xaml 的交互逻辑
    /// </summary>
    public partial class ToggleSwitch_Control : UserControl
    {
        
        public ToggleSwitch_Control()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty IsCheckedProperty =
           DependencyProperty.Register("IsChecked", typeof(bool), typeof(ToggleSwitch_Control), new PropertyMetadata(default(bool), OnIsCheckedChanged));
        public static readonly DependencyProperty CanOpenProperty =
           DependencyProperty.Register("CanOpen", typeof(bool), typeof(ToggleSwitch_Control), new PropertyMetadata(true));

        public event RoutedEventHandler Checked;
        public event RoutedEventHandler UnChecked;

        public bool CanOpen
        {
            get => (bool)GetValue(CanOpenProperty);
            set => SetValue(CanOpenProperty, value);
        }
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        

        private static void OnIsCheckedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as ToggleSwitch_Control).OnIsCheckedChanged(args);
        }

        private void OnIsCheckedChanged(DependencyPropertyChangedEventArgs args)
        {
            //fillRectangle.Visibility = IsChecked ? Visibility.Visible : Visibility.Collapsed;
            //fillRectangle.SetResourceReference(IsChecked)
            slideBorder.HorizontalAlignment = IsChecked ? HorizontalAlignment.Right : HorizontalAlignment.Left;

            if (IsChecked)
            {
                Checked?.Invoke(this, new RoutedEventArgs());
            }

            if (!IsChecked)
            {
                UnChecked?.Invoke(this, new RoutedEventArgs());
            }
        }


        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs args)
        {
            if(CanOpen)
                IsChecked ^= true;
            base.OnMouseLeftButtonUp(args);
        }

    }
}
