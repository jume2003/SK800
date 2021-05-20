using IBatisNet.DataMapper;
using SKABO.Common.Models.Judger;
using SKABO.Common.Parameters.Judger;
using SKABO.Common.Utils;
using SKABO.DAL.IDAO.IJudger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.DAO.Judger
{
    public class ResultDAO : IResultDAO
    {
        private ISqlMapper Mapper;
        public ResultDAO(ISqlMapper mapper)
        {
            this.Mapper = mapper;
        }
        public IList<T_Result> QueryT_Result(ResultParameter resultParameter)
        {
            return Mapper.QueryForList<T_Result>("QueryT_Result", resultParameter);
        }

        public bool SaveT_Result(T_Result result)
        {
            bool res = false;
            if (result.ID == 0)
            {
                Mapper.BeginTransaction();
                try
                {
                    if (result.Picture != null)
                    {
                        Mapper.Insert("InsertT_Picture", result.Picture);
                        result.PictureID = result.Picture.ID;
                    }
                    Mapper.Insert("InsertT_Result", result);
                    Mapper.CommitTransaction();
                    res = true;
                }
                catch(Exception ex)
                {
                    Tool.AppLogError(ex);
                    Mapper.RollBackTransaction();
                }
            }
            return res;
        }

        public bool UpdateT_Result(T_Result result)
        {
            bool res = false;
            Mapper.BeginTransaction();
            try
            {
                if (result.Picture != null)
                {
                    Mapper.Update("UpdateT_Picture", result.Picture);
                }
                Mapper.Update("UpdateT_Result", result);
                Mapper.CommitTransaction();
                res = true;
            }
            catch (Exception ex)
            {
                Tool.AppLogError(ex);
                Mapper.RollBackTransaction();
            }
            return res;
        }
    }
}
