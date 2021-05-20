using SKABO.Common.Models.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.BLL.IServices.ILogic
{
    public interface ILogicService
    {
        IList<T_LogicTest> QueryT_LogicTest();
        bool SaveT_LogicTest(T_LogicTest t_LogicTest);
        bool DeleteT_LogicStep(int id);
        bool DeleteT_LogicTest(int id);
        T_LogicTest QueryT_LogicTestById(int id);
    }
}
