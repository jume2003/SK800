using SKABO.Common.Models.TestStep;
using SKABO.Common.UserCtrls;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SK_ABO.Pages.SetStep
{
    public class ZKDFYViewModel:Screen
    {
        public ZKDFYStepParameter ZKDFYParam { get; set; }
        public String StepParameter
        {
            get
            {
                return ZKDFYParam == null ? "" : ZKDFYParam.ToString();
            }
            set
            {
                ZKDFYParam = new ZKDFYStepParameter(value);
            }
        }
        public void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //2018-04-13找到原因，在控件中设置了DataContext为self,所以不成功,现在页面可以正常绑定
            //ZKDFYView view = (ZKDFYView)sender;
            //view.NumZKDFYTime.BindingValue( this);
        }
    }
}
