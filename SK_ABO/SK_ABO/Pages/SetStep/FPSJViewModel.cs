using SK_ABO.UserCtrls;
using SKABO.Common.Models.TestStep;
using SKABO.Common.UserCtrls;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;
using System.Windows;
using System.Windows.Data;

namespace SK_ABO.Pages.SetStep
{
    public class FPSJViewModel:Screen
    {
        public FPSJStepParameter FPSJParam { get; set; }
        public String StepParameter
        {
            get
            {
                return FPSJParam == null ? "" : FPSJParam.ToString();
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    FPSJParam = new FPSJStepParameter(); ;
                }
                else
                {
                    try
                    {
                        FPSJParam = value.ToInstance<FPSJStepParameter>();
                    }
                    catch(Exception ex)
                    {
                        Tool.AppLogWarn(ex);
                        FPSJParam = new FPSJStepParameter();
                    }
                }
                
            }
        }

        public void Page_Loaded(object sender, RoutedEventArgs e)
        {
            FPSJView view = (FPSJView)sender;
            var list=view.GetControls<SKABO.Common.UserCtrls.NumericUpDown_Control>();
            if(list!=null && list.Count > 0)
            {
                foreach(var item in list)
                {
                    item.BindingValue(this);
                }
            }
        }
    }
}
