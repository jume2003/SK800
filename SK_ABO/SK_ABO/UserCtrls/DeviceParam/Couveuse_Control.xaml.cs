﻿using SKABO.Common.Models.Communication.Unit;
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

namespace SK_ABO.UserCtrls.DeviceParam
{
    /// <summary>
    /// Couveuse_Control.xaml 的交互逻辑
    /// </summary>
    public partial class Couveuse_Control : UserControl
    {
        public static readonly DependencyProperty CouvProperty = DependencyProperty.Register("Couv", typeof(Couveuse), typeof(Couveuse_Control), null, null);
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(String), typeof(Couveuse_Control), null, null);
        public Couveuse_Control()
        {
            InitializeComponent();
        }
        public String Header
        {
            get
            {
                return GetValue(HeaderProperty) as String;
            }
            set
            {
                SetValue(HeaderProperty, value);
            }
        }
        public Couveuse Couv
        {
            get
            {
                return GetValue(CouvProperty) as Couveuse;
            }
            set
            {
                SetValue(CouvProperty, value);
            }
        }
    }
}
