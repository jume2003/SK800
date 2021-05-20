using SKABO.BLL.IServices.IDevice;
using SKABO.Common.Models.BJ;
using SKABO.DAL.IDAO.IDevice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.BLL.Services.Device
{
    public class BJService: IBJService
    {
        private IBJDAO bjDAO;
        public BJService(IBJDAO bjDAO)
        {
            this.bjDAO = bjDAO;
        }

        public IList<VBJ> QueryBJ<T>()
        {
            String BJTable = typeof(T).Name;
            return bjDAO.QueryBJ(BJTable);
        }

        public IList<VBJ> QueryBJ(string BJTable)
        {
            return bjDAO.QueryBJ(BJTable);
        }
        public bool SaveBJ(IList<VBJ> BJList)
        {
            return bjDAO.SaveBJ(BJList);
        }
        public bool DeleteBJ(VBJ BJ)
        {
            return bjDAO.DeleteBJ(BJ);
        }
    }
}
