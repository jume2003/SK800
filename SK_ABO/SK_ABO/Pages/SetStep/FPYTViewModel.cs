using SKABO.Common.Enums;
using SKABO.Common.Models.TestStep;
using SKABO.Common.Utils;
using SKABO.ResourcesManager;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SK_ABO.Pages.SetStep
{
    public class FPYTViewModel:Screen
    {
        public static ObservableCollection<string> FpytTypeList = new ObservableCollection<string>();
        public FPYTStepParameter Param { get; set; }
        public FPYTViewModel()
        {
            FpytTypeList.Clear();
            FpytTypeList.Add("病人血清");
            FpytTypeList.Add("病人红细胞");
            FpytTypeList.Add("献血员血清");
            FpytTypeList.Add("献血员红细胞");
            foreach(var agent in ResManager.getInstance().agentiawa_list)
            {
                foreach(var code in agent.Values)
                {
                    if(code!=null)
                    FpytTypeList.Add(code.ToString());
                }
            }
        }
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
                    Param = new FPYTStepParameter();
                }
                else
                {
                    try
                    {
                        Param = value.ToInstance<FPYTStepParameter>();
                    }
                    catch (Exception ex)
                    {
                        Tool.AppLogWarn(ex);
                        Param = new FPYTStepParameter();
                    }
                }

            }
        }

        public void Page_Loaded(object sender, RoutedEventArgs e)
        {
            FPYTView view = (FPYTView)sender;
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
