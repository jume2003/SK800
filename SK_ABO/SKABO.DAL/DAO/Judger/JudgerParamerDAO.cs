using IBatisNet.DataMapper;
using SKABO.Common.Models.Judger;
using SKABO.DAL.IDAO.IJudger;
using System;
using System.Collections.Generic;

namespace SKABO.DAL.DAO.Judger
{
    
    public class JudgerParamerDAO: IJudgerParamerDAO
    {
        ISqlMapper mapper;
        public JudgerParamerDAO(ISqlMapper mapper)
        {
            this.mapper = mapper;
        }
        public IList<T_JudgeParamer> QueryALlParamerByMSN(String MSN)
        {
            return mapper.QueryForList<T_JudgeParamer>("QueryT_JudgeParamer", MSN);
        }
        public int InsertT_JudgeParamer(T_JudgeParamer t_JudgeParamer)
        {
            return Convert.ToInt32( mapper.Insert("InsertT_JudgeParamer", t_JudgeParamer));
        }
        public bool UpdateT_JudgeParamer(T_JudgeParamer t_JudgeParamer)
        {
            return mapper.Update("UpdateT_JudgeParamer", t_JudgeParamer)>=0;
        }

        public T_Camera QueryCamera(string MSN)
        {
            return mapper.QueryForObject<T_Camera>("QueryT_Camera", MSN);
        }

        public bool UpdateOrInsertCamera(T_Camera t_Camera)
        {
            bool result = false;
            var session = mapper.BeginTransaction();
            try
            {
                if (t_Camera.ID == 0)
                {
                    mapper.Insert("InsertT_Camera", t_Camera);
                    result = t_Camera.ID > 0;
                }
                else
                {
                    result=mapper.Update("UpdateT_Camera", t_Camera)>0;
                }
                mapper.CommitTransaction();
            }
            catch (Exception e)
            {
                result = false;
                mapper.RollBackTransaction();
            }
            return result;
        }

        public T_ParseLEDParameter LoadPLP()
        {
            return mapper.QueryForObject<T_ParseLEDParameter>("QueryT_ParseLEDParameter",null);
        }

        public bool SavePLP(T_ParseLEDParameter PLP)
        {
            bool result = false;
            var session = mapper.BeginTransaction();
            try
            {
                if (PLP.ID == 0)
                {
                    mapper.Insert("InsertT_ParseLEDParameter", PLP);
                    result = PLP.ID > 0;
                }
                else
                {
                    result = mapper.Update("UpdateT_ParseLEDParameter", PLP) > 0;
                }
                mapper.CommitTransaction();
            }catch(Exception e)
            {
                result = false;
                mapper.RollBackTransaction();
            }
            return result;
        }

        public T_ParseTubeParameter LoadPTP()
        {
            return mapper.QueryForObject<T_ParseTubeParameter>("QueryT_ParseTubeParameter", null);
        }

        public bool SavePTP(T_ParseTubeParameter PTP)
        {
            bool result = false;
            var session = mapper.BeginTransaction();
            try
            {
                if (PTP.ID == 0)
                {
                    mapper.Insert("InsertT_ParseTubeParameter", PTP);
                    result = PTP.ID > 0;
                }
                else
                {
                    result = mapper.Update("UpdateT_ParseTubeParameter", PTP) > 0;
                }
                mapper.CommitTransaction();
            }
            catch (Exception e)
            {
                result = false;
                mapper.RollBackTransaction();
            }
            return result;
        }
    }
}
