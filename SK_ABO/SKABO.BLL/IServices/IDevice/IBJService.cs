using SKABO.Common.Models.BJ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.BLL.IServices.IDevice
{
    public interface IBJService
    {
        IList<VBJ> QueryBJ<T>();
        IList<VBJ> QueryBJ(String BJTable);

        bool SaveBJ(IList<VBJ> BJList);
        bool DeleteBJ(VBJ BJ);
    }
}
