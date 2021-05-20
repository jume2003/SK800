using IBatisNet.DataMapper;
using SKABO.Common.Models.BJ;
using SKABO.Common.Utils;
using SKABO.DAL.IDAO.IDevice;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SKABO.DAL.DAO.Device
{
    public class BJDAO : IBJDAO
    {
        ISqlMapper mapper { get; set; }
        public BJDAO(ISqlMapper mapper)
        {
            this.mapper = mapper;
        }
        public IList<VBJ> QueryBJ(String BJTable)
        {
            return mapper.QueryForList<VBJ>("Query"+ BJTable, "");
        }
        public bool InsertBJ(VBJ BJ)
        {
            String BJTable = BJ.GetType().Name;
            mapper.Insert(String.Format("Insert{0}", BJTable), BJ);
            return BJ.ID > 0;
        }
        public bool UpdateBJ(VBJ BJ)
        {
            String BJTable = BJ.GetType().Name;
            var res=mapper.Update(String.Format("Update{0}", BJTable), BJ);
            return res > 0;
        }
        public bool SaveBJ(IList<VBJ> BJList)
        {
            if (BJList == null || BJList.Count == 0) return true;
            mapper.BeginTransaction();
            try
            {
                foreach (var item in BJList)
                {
                    if (item.ID == 0)
                    {
                        InsertBJ(item);
                    }
                    else
                    {
                        UpdateBJ(item);
                    }
                }
                mapper.CommitTransaction();
                return true;
            }
            catch(Exception ex)
            {
                Tool.AppLogError(ex);
                mapper.RollBackTransaction();
                return false;
            }
        }
        public bool DeleteBJ(VBJ BJ)
        {

            Hashtable ht = new Hashtable();
            ht.Add("TableName", GetPgTable(BJ));//BJ.GetType().Name);
            ht.Add("ID", BJ.ID);
            return mapper.Delete("DeleteBJ", ht) >0;
        }
        private String GetPgTable(VBJ BJ)
        {
            var name = BJ.GetType().Name;
            Regex reg = new Regex("([a-z\\d]+)([A-Z]{1})");

            name = name.Replace("T_", "");
            name = reg.Replace(name, "$1_$2");

            return name;

        }
    }
}
