using SKABO.BLL.IServices.ILogic;
using SKABO.Common.Models.Logic;
using SKABO.DAL.IDAO.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.BLL.Services.Logic
{
    public class LogicService : ILogicService
    {
        private ILogicDAO LogicDAO;
        public LogicService(ILogicDAO logicDAO)
        {
            this.LogicDAO = logicDAO;
        }

        public bool DeleteT_LogicStep(int id)
        {
            if (id == 0) return true;
            return LogicDAO.DeleteT_LogicStep(id);
        }

        public IList<T_LogicTest> QueryT_LogicTest()
        {
            return LogicDAO.QueryT_LogicTest();
        }

        public bool SaveT_LogicTest(T_LogicTest t_LogicTest)
        {
            return LogicDAO.SaveT_LogicTest(t_LogicTest);
        }
        public bool DeleteT_LogicTest(int id)
        {
            return LogicDAO.DeleteT_LogicTest(id);
        }

        public T_LogicTest QueryT_LogicTestById(int id)
        {
            return LogicDAO.QueryT_LogicTestById(id);
        }
    }
}
