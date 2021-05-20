using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.BLL.IServices.IDevice
{
    public interface IPlcBjParamService
    {
        T LoadFromJson<T>() where T:class;
        bool SaveAsJson<T>(T TObj) where T:class;
    }
}
