using IBatisNet.DataMapper;
using SKABO.Common.Models.Communication;
using SKABO.DAL.IDAO.IPcl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.DAO.Plc
{
    class PlcParameterDao : IPlcParameterDao
    {
        public PlcParameterDao(ISqlMapper mapper)
        {
            this.mapper = mapper;
        }
        ISqlMapper mapper { get; set; }
        bool IPlcParameterDao.SavePLCParameter(T_PlcParameter PP)
        {
            var count = mapper.Insert("SavePLCParameter", PP) as Int32?;
            return count.HasValue && count.Value > 0;
        }
    }
}
