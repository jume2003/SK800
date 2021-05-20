using SKABO.Common.Models.TestStep;
using SKABO.Common.Utils;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SK_ABO.Pages.SetStep
{
    public class FPBRXSHXBViewModel:Screen
    {
        public FPBRXSHXBStepParameter Param { get; set; }
        public String StepParameter
        {
            get
            {
                return Param == null ? "" : Param.ToString();
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    Param = new FPBRXSHXBStepParameter(); ;
                }
                else
                {
                    try
                    {
                        Param = value.ToInstance<FPBRXSHXBStepParameter>();
                    }
                    catch (Exception ex)
                    {
                        Tool.AppLogWarn(ex);
                        Param = new FPBRXSHXBStepParameter();
                    }
                }

            }
        }

        public void Page_Loaded(object sender, RoutedEventArgs e)
        {
            FPBRXSHXBView view = (FPBRXSHXBView)sender;
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
