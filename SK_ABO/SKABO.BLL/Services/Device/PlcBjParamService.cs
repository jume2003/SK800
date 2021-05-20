using SKABO.BLL.IServices.IDevice;
using SKABO.Common.Models.Communication;
using SKABO.Common.Utils;
using SKABO.DAL.DAO.Device;
using SKABO.DAL.IDAO.IDevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.BLL.Services.Device
{
    public class PlcBjParamService : IPlcBjParamService
    {
        private IPlcBjParamDAO plcDAO;
        public PlcBjParamService(IPlcBjParamDAO plcDAO)
        {
            this.plcDAO = plcDAO;
        }
        T IPlcBjParamService.LoadFromJson<T>() 
        {
            var tp = this.plcDAO.LoadPlcParameter(typeof(T).Name);
            if (tp != null)
            {
                String jsonStr = tp.JsonVal;
                return JsonUtil.ToInstance<T>(jsonStr);
            }
            return default(T);
        }

       

        bool IPlcBjParamService.SaveAsJson<T>(T TObj)
        {
            var dbObj = new T_PlcParameter();
            dbObj.CreateAt = DateTime.Now;
            dbObj.KeyId = typeof(T).Name;
            dbObj.JsonVal= SKABO.Common.Utils.JsonUtil.ToJsonStr(TObj);
            return this.plcDAO.SavePLCParameter(dbObj);
        }
    }
}
