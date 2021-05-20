using SKABO.Common.Models.BJ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.IDAO.IDevice
{
    public interface IBJDAO
    {
        IList<VBJ> QueryBJ(String BJTable);
        bool InsertBJ(VBJ BJ);
        bool UpdateBJ(VBJ BJ);
        bool SaveBJ(IList<VBJ> BJList);
        bool DeleteBJ(VBJ BJ);
    }
}
