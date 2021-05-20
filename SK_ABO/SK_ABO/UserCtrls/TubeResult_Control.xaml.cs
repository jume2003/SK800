using SKABO.Judger.Enums;
using System;
using System.Collections.Generic;
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
using SKABO.Common.Utils;
using SKABO.Common.Models.Judger;

namespace SK_ABO.UserCtrls
{
    /// <summary>
    /// TubeResult_Control.xaml 的交互逻辑
    /// </summary>
    public partial class TubeResult_Control : UserControl
    {
        public static readonly DependencyProperty ResultStrProperty =
            DependencyProperty.Register("ResultStr", typeof(String), typeof(TubeResult_Control), new FrameworkPropertyMetadata(""));
        public static readonly DependencyProperty TubeTypeProperty =
            DependencyProperty.Register("TubeType", typeof(String), typeof(TubeResult_Control), new FrameworkPropertyMetadata("A"));
        public static readonly DependencyProperty TubeIndexProperty =
            DependencyProperty.Register("TubeIndex", typeof(byte), typeof(TubeResult_Control), new FrameworkPropertyMetadata((byte)1));
        public static readonly DependencyProperty TestResultProperty =
            DependencyProperty.Register("TestResult", typeof(T_Result), typeof(TubeResult_Control));
        public TubeResult_Control()
        {
            InitializeComponent();
        }
        public T_Result TestResult
        {
            get
            {
                return (T_Result)GetValue(TestResultProperty);
            }
            set
            {
                SetValue(TestResultProperty, value);
                if (value == null)
                {
                    this.pic.Source = null;
                    this.TubeType = "";
                    return;
                }
                switch (TubeIndex) {
                    case 1:
                        {
                            //if(value.)
                            if (value.Gel != null)
                            {
                                this.TubeType = value.Gel.Name1;
                            }
                            if (value.Picture.Tube1 != null)
                            {
                                this.pic.Source = ImgUtil.BitmapToBitmapSource( ImgUtil.BytesToBitmap(value.Picture.Tube1));
                            }
                            this.Value = (ResultEnum)value.Picture.T1;
                            break;
                        }
                    case 2:
                        {
                            if (value.Gel != null)
                            {
                                this.TubeType = value.Gel.Name2;
                            }
                            if (value.Picture.Tube2 != null)
                            {
                                this.pic.Source = ImgUtil.BitmapToBitmapSource(ImgUtil.BytesToBitmap(value.Picture.Tube2));
                            }
                            this.Value = (ResultEnum)value.Picture.T2;
                            break;
                        }
                    case 3:
                        {
                            if (value.Gel != null)
                            {
                                this.TubeType = value.Gel.Name3;
                            }
                            if (value.Picture.Tube3 != null)
                            {
                                this.pic.Source = ImgUtil.BitmapToBitmapSource(ImgUtil.BytesToBitmap(value.Picture.Tube3));
                            }
                            this.Value = (ResultEnum)value.Picture.T3;
                            break;
                        }
                    case 4:
                        {
                            if (value.Gel != null)
                            {
                                this.TubeType = value.Gel.Name4;
                            }
                            if (value.Picture.Tube4 != null)
                            {
                                this.pic.Source = ImgUtil.BitmapToBitmapSource(ImgUtil.BytesToBitmap(value.Picture.Tube4));
                            }
                            this.Value = (ResultEnum)value.Picture.T4;
                            break;
                        }
                    case 5:
                        {
                            if (value.Gel != null)
                            {
                                this.TubeType = value.Gel.Name5;
                            }
                            if (value.Picture.Tube5 != null)
                            {
                                this.pic.Source = ImgUtil.BitmapToBitmapSource(ImgUtil.BytesToBitmap(value.Picture.Tube5));
                            }
                            this.Value = (ResultEnum)value.Picture.T5;
                            break;
                        }
                    case 6:
                        {
                            if (value.Gel != null)
                            {
                                this.TubeType = value.Gel.Name6;
                            }
                            if (value.Picture.Tube6 != null)
                            {
                                this.pic.Source = ImgUtil.BitmapToBitmapSource(ImgUtil.BytesToBitmap(value.Picture.Tube6));
                            }
                            this.Value = (ResultEnum)value.Picture.T6;
                            break;
                        }
                    case 7:
                        {
                            if (value.Gel != null)
                            {
                                this.TubeType = value.Gel.Name7;
                            }
                            if (value.Picture.Tube7 != null)
                            {
                                this.pic.Source = ImgUtil.BitmapToBitmapSource(ImgUtil.BytesToBitmap(value.Picture.Tube7));
                            }
                            this.Value = (ResultEnum)value.Picture.T7;
                            break;
                        }
                    case 8:
                        {
                            if (value.Gel != null)
                            {
                                this.TubeType = value.Gel.Name8;
                            }
                            if (value.Picture.Tube8 != null)
                            {
                                this.pic.Source = ImgUtil.BitmapToBitmapSource(ImgUtil.BytesToBitmap(value.Picture.Tube8));
                            }
                            this.Value = (ResultEnum)value.Picture.T8;
                            break;
                        }
                }
                Cmb_Results.SelectedValue = this.Value;


            }
        }
        private static IOrderedEnumerable<KeyValuePair<String, ResultEnum>> _ResultDict;
        private static IOrderedEnumerable<KeyValuePair<String,ResultEnum>> Static_ResultDict
        {
            get
            {
                if (_ResultDict == null)
                {
                    IDictionary < String, ResultEnum > dic = new Dictionary<String, ResultEnum>();
                    foreach(var item in Enum.GetValues(typeof(ResultEnum)))
                    {
                        ResultEnum re = (ResultEnum)item;
                        dic.Add(re.GetDescription(), re);
                    }
                    _ResultDict=dic.OrderByDescending(c => c.Value);
                    dic = null;
                }
                return _ResultDict;
            }
        }
        public IOrderedEnumerable<KeyValuePair<String, ResultEnum>> ResultDict
        {
            get => Static_ResultDict;
        }
        public String ResultStr
        {
            get
            {
                return (String)GetValue(ResultStrProperty);
            }
            set
            {
                SetValue(ResultStrProperty, value);
            }
        }
        public ResultEnum Value { get => _Value; set { _Value = value; ResultStr = value.GetDescription(); } }
        private ResultEnum _Value;
        public byte TubeIndex
        {
            get
            {
                return (byte)GetValue(TubeIndexProperty);
            }
            set
            {
                SetValue(TubeIndexProperty, value);
            }
        }
        public String TubeType
        {
            get
            {
                return (String)GetValue(TubeTypeProperty);
            }
            set
            {
                SetValue(TubeTypeProperty, value);
            }
        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResultStr = Value.GetDescription();
            if (ResultChanged != null)
            {
                ResultChanged(this, null);
            }
        }
        public event RoutedEventHandler ResultChanged;
    }
}
