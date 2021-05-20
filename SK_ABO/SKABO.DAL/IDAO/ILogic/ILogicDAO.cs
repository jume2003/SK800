using SKABO.Common.Models.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.IDAO.ILogic
{
    public interface ILogicDAO
    {
        IList<T_LogicTest> QueryT_LogicTest();
        bool SaveT_LogicTest(T_LogicTest logicTest);
        bool DeleteT_LogicStep(int id);
        bool DeleteT_LogicStepByProgramId(int id);
        bool DeleteT_LogicTest(int id);
        T_LogicTest QueryT_LogicTestById(int id);
    }
}
