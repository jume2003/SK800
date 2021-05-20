using SKABO.Common.Models.GEL;
using SKABO.Common.Models.Judger;
using SKABO.Common.Parameters.Judger;
using SKABO.Judger.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.BLL.IServices.IJudger
{
    public interface IResultService
    {
        String FinishResult(T_Gel Gel, out String Color ,params ResultEnum[] Results);
        Judger.Enums.ResultEnum[] GetResultEnums(T_Picture pic, byte StartNo, byte Total);
        IList<T_Result> QueryT_Result(ResultParameter resultParameter);
        bool SaveT_Result(T_Result result);
        /// <summary>
        /// 更新修改后的结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        bool UpdateT_Result(T_Result result);
    }
}
