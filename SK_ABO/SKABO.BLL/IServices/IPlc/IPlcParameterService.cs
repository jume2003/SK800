using SKABO.Common.Models.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.BLL.IServices.IPlc
{
    interface IPlcParameterService
    {
        bool SavePLCParameter(T_PlcParameter PP);
    }
}
