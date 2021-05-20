using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;
using System.Windows;
using SKABO.Common.Models.TestStep;

namespace SK_ABO.Pages.SetStep
{
    public class FPBRXQViewModel:Screen
    {
        public FPBRXQStepParameter FPBRXQParam { get; set; }
        public String StepParameter
        {
            get
            {
                return FPBRXQParam == null ? "" : FPBRXQParam.ToString();
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    FPBRXQParam = new FPBRXQStepParameter(); ;
                }
                else
                {
                    try
                    {
                        FPBRXQParam = value.ToInstance<FPBRXQStepParameter>();
                    }
                    catch (Exception ex)
                    {
                        Tool.AppLogWarn(ex);
                        FPBRXQParam = new FPBRXQStepParameter();
                    }
                }

            }
        }
        public void Page_Loaded(object sender, RoutedEventArgs e)
        {
            FPBRXQView view = (FPBRXQView)sender;
            var list = view.GetControls<SKABO.Common.UserCtrls.NumericUpDown_Control>();
            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    item.BindingValue(this);
                }
            }
        }
    }
}
