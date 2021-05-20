using SKABO.Common.Models.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.IDAO.IPcl
{
    interface IPlcParameterDao
    {
        bool SavePLCParameter(T_PlcParameter PP);
    }
}
