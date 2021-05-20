using SKABO.Common.Models.Judger;
using SKABO.Common.Parameters.Judger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.IDAO.IJudger
{
    public interface IResultDAO
    {
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
