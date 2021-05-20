using SKABO.Common.Models.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.IDAO.IDevice
{
    public interface IPlcBjParamDAO
    {
        T_PlcParameter LoadPlcParameter(String KeyId);
        bool SavePLCParameter(T_PlcParameter Param);
    }
}
