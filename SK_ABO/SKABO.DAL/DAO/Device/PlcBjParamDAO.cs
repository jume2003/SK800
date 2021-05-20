using IBatisNet.DataMapper;
using SKABO.Common.Models.Communication;
using SKABO.DAL.IDAO.IDevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.DAO.Device
{
    public class PlcBjParamDAO: IPlcBjParamDAO
    {
        ISqlMapper mapper { get; set; }
        public PlcBjParamDAO(ISqlMapper mapper)
        {
            this.mapper = mapper;
        }
        public T_PlcParameter LoadPlcParameter(String KeyId)
        {
            return this.mapper.QueryForObject("QueryPLCParameterByKey", KeyId) as T_PlcParameter;
        }
        public bool SavePLCParameter(T_PlcParameter Param)
        {
            var obj = this.mapper.Insert("SavePLCParameter", Param);
            //var rows=(Int16)obj;
            return true;
        }
    }
}
