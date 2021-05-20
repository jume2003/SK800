using IBatisNet.DataMapper;
using SKABO.BLL.IServices.IJudger;
using SKABO.Common.Models.Judger;
using SKABO.DAL.DAO.Judger;
using SKABO.DAL.IDAO.IJudger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.BLL.Services.Judger
{
    public class JudgerParamerService: IJudgerParamerService
    {
        ISqlMapper mapper;
        private IJudgerParamerDAO JDAO;
        public JudgerParamerService(ISqlMapper mapper)
        {
            this.mapper = mapper;
            JDAO = new JudgerParamerDAO(mapper);
        }
        public IList<T_JudgeParamer> QueryALlParamerByMSN(String MSN)
        {
            return JDAO.QueryALlParamerByMSN(MSN);
        }
        public int InsertT_JudgeParamer(T_JudgeParamer t_JudgeParamer)
        {
            return JDAO.InsertT_JudgeParamer(t_JudgeParamer);
        }
        public bool UpdateT_JudgeParamer(T_JudgeParamer t_JudgeParamer)
        {
            return JDAO.UpdateT_JudgeParamer(t_JudgeParamer);
        }
        public bool InsertT_JudgeParamer(IList<T_JudgeParamer> t_JudgeParamerList)
        {
            bool result = false;
            try
            {
                var session = mapper.BeginTransaction();
                foreach (var item in t_JudgeParamerList)
                {
                    JDAO.InsertT_JudgeParamer(item);
                }
                mapper.CommitTransaction();
                result = true;
            }
            catch(Exception ex) {
            }
            return result;
        }
        public bool UpdateT_JudgeParamer(IList<T_JudgeParamer> t_JudgeParamerList)
        {
            bool result = false;
            try
            {
                var session = mapper.BeginTransaction();
                foreach (var item in t_JudgeParamerList)
                {
                    if(item.ID==0)
                        JDAO.InsertT_JudgeParamer(item);
                    else
                        JDAO.UpdateT_JudgeParamer(item);
                }
                mapper.CommitTransaction();
                result = true;
            }
            catch(Exception ex) {
                mapper.RollBackTransaction();
            }
            return result;
        }

        public T_Camera QueryCamera(string MSN)
        {
            return JDAO.QueryCamera(MSN);
        }

        public bool UpdateOrInsertCamera(T_Camera t_Camera)
        {
            return JDAO.UpdateOrInsertCamera(t_Camera);
        }

        public T_ParseLEDParameter LoadPLP()
        {
            return JDAO.LoadPLP();
        }

        public bool SavePLP(T_ParseLEDParameter PLP)
        {
            return JDAO.SavePLP(PLP);
        }

        public T_ParseTubeParameter LoadPTP()
        {
            return JDAO.LoadPTP();
        }

        public bool SavePTP(T_ParseTubeParameter PTP)
        {
            return JDAO.SavePTP(PTP);
        }
    }
}
