using IBatisNet.DataMapper;
using SKABO.Common.Models.Logic;
using SKABO.Common.Utils;
using SKABO.DAL.IDAO.ILogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.DAO.Logic
{
    public class LogicDAO : ILogicDAO
    {
        ISqlMapper mapper { get; set; }
        public LogicDAO(ISqlMapper mapper)
        {
            this.mapper = mapper;
        }
        IList<T_LogicTest> ILogicDAO.QueryT_LogicTest()
        {
            return mapper.QueryForList<T_LogicTest>("QueryT_LogicTest",null);
        }

        bool ILogicDAO.SaveT_LogicTest(T_LogicTest logicTest)
        {
            bool result = false;
            mapper.BeginTransaction();
            try
            {
                if (logicTest.ID == 0)
                {
                    mapper.Insert("InsertT_LogicTest", logicTest);
                }
                else
                {
                    mapper.Update("UpdateT_LogicTest", logicTest);
                }
                if (logicTest.ID > 0)
                {
                    if(logicTest.LogicSteps!=null && logicTest.LogicSteps.Count > 0)
                    {
                        foreach(var item in logicTest.LogicSteps)
                        {
                            item.ProgramID = logicTest.ID;
                            if (item.ID == 0)
                            {
                                mapper.Insert("InsertT_LogicStep", item);
                            }
                            else
                            {
                                mapper.Update("UpdateT_LogicStep", item);
                            }
                        }
                    }
                    mapper.CommitTransaction();
                    result = true;
                }
                else
                {
                    mapper.RollBackTransaction();
                }
            }catch(Exception ex)
            {
                mapper.RollBackTransaction();
                Tool.AppLogError(ex);
            }
            return result;
        }

        public bool DeleteT_LogicStep(int id)
        {
            return mapper.Delete("DeleteT_LogicStep", id)>0;
        }

        public bool DeleteT_LogicStepByProgramId(int id)
        {
            return mapper.Delete("DeleteT_LogicStepByProgramId", id) > 0;
        }

        public bool DeleteT_LogicTest(int id)
        {
            var result = false;
            mapper.BeginTransaction();
            try
            {
                DeleteT_LogicStepByProgramId(id);
                result= mapper.Delete("DeleteT_LogicTest", id) > 0;
                mapper.CommitTransaction();
            }catch(Exception ex)
            {
                Tool.AppLogError(ex);
                mapper.RollBackTransaction();
            }

            return result;
        }

        public T_LogicTest QueryT_LogicTestById(int id)
        {
            return mapper.QueryForObject<T_LogicTest>("QueryT_LogicTestById", id);
        }
    }
}
