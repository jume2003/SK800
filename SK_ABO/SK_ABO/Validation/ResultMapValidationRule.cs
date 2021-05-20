using SKABO.Common.Models.GEL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace SK_ABO.Validation
{
    public class ResultMapValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is BindingGroup)
            {
                BindingGroup group = (BindingGroup)value;
                foreach (var item in group.Items)
                {
                    var tr = item as T_ResultMap;
                    if (tr.ResultDesc==null || tr.ResultMap==null ||string.IsNullOrEmpty(tr.ResultDesc.Trim()) || string.IsNullOrEmpty(tr.ResultMap.Trim())
                        )
                    {
                        return new ValidationResult(false, "结果、表达式不能空！");
                    }
                    Regex reg = new Regex(@"^[\*\-\+]{1,}$");
                    if (!reg.IsMatch(tr.ResultMap))
                    {
                        return new ValidationResult(false, "表达式中只能含有【*-+】！");
                    }
                }
            }
            return ValidationResult.ValidResult;
        }
    }
}
