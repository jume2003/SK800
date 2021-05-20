using IBatisNet.DataMapper;
using SKABO.Common.Models.GEL;
using SKABO.Common.Utils;
using SKABO.DAL.IDAO.IGEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.DAO.GEL
{
    public class GELDAO : IGELDAO
    {
        ISqlMapper mapper { get; set; }
        public GELDAO(ISqlMapper mapper)
        {
            this.mapper = mapper;
        }
        private void InserOrUpdateCollect<T>(IEnumerable<T> Collect,String TableName,params KeyValuePair<String,Object>[] keyValue)
        {
            var ID_P = typeof(T).GetProperty("ID");
            foreach (var item in Collect)
            {
                if (keyValue != null)
                {
                    foreach (var kv in keyValue)
                    {
                        typeof(T).InvokeMember(kv.Key, System.Reflection.BindingFlags.SetProperty 
                            , null, item, new object[] { kv.Value });
                    }
                    var ID = (int)ID_P.GetValue(item);
                    //item.GelID = t_Gel.ID;
                    if (ID == 0)
                    {
                        mapper.Insert("Insert" + TableName, item);
                    }
                    else
                    {
                        mapper.Update("Update" + TableName, item);
                    }
                }
            }
        }
        public bool InserOrUpdateGEL(T_Gel t_Gel)
        {
            if (t_Gel.ID == 0)
            {
                var session=mapper.BeginTransaction();
                try
                {
                    mapper.Insert("InsertT_Gel", t_Gel);
                    if (t_Gel.ResultMaps != null)
                    {
                        InserOrUpdateCollect(t_Gel.ResultMaps, "T_ResultMap", new KeyValuePair<String, Object>("GelID", t_Gel.ID));
                    }
                    if (t_Gel.GelSteps != null)
                    {
                        InserOrUpdateCollect(t_Gel.GelSteps, "T_GelStep", new KeyValuePair<String, Object>("GelID", t_Gel.ID));
                    }
                    mapper.CommitTransaction();
                    return t_Gel.ID > 0;
                }
                catch(Exception ex)
                {
                    Tool.AppLogError(ex);
                    mapper.RollBackTransaction();
                    return false;
                }
                
            }
            else
            {
                var result = false;
                var session = mapper.BeginTransaction();
                try
                {
                    result = mapper.Update("UpdateT_Gel", t_Gel) > 0;
                    if (t_Gel.ResultMaps != null)
                    {
                        InserOrUpdateCollect(t_Gel.ResultMaps, "T_ResultMap", new KeyValuePair<String, Object>("GelID", t_Gel.ID));
                    }
                    if (t_Gel.GelSteps != null)
                    {
                        InserOrUpdateCollect(t_Gel.GelSteps, "T_GelStep", new KeyValuePair<String, Object>("GelID", t_Gel.ID));
                    }
                    mapper.CommitTransaction();
                    result = true;
                }
                catch(Exception ex)
                {
                    Tool.AppLogError(ex);
                    result = false;
                    mapper.RollBackTransaction();
                }
                
                return result;
            }
        }

        public IList<T_Gel> QueryAllGel()
        {
            return mapper.QueryForList<T_Gel>("QueryAllT_Gel", null);
        }

        public IList<T_StepDefine> QueryAllStepDefine()
        {
            return mapper.QueryForList<T_StepDefine>("QueryAllT_StepDefine", null);
        }

        public IList<T_StepDefine> QueryStepDefineByClass(int StepClass)
        {
            return mapper.QueryForList<T_StepDefine>("QueryStepDefineBySetpClass", StepClass);
        }

        public IList<T_GelStep> QueryGelStepByGelId(int GelID)
        {
            return mapper.QueryForList<T_GelStep>("QueryGelStepByGelId", GelID);
        }

        public IList<T_ResultMap> QueryResMapByGelId(int GelID)
        {
            return mapper.QueryForList<T_ResultMap>("QueryResMapByGelId", GelID);
        }


        public bool DeleteGel(T_Gel Gel)
        {
            bool result = false;
            var session = mapper.BeginTransaction();
            try
            {
                if (Gel.GelSteps != null)
                {
                    String Ids = "";
                    foreach(var item in Gel.GelSteps)
                    {
                        Ids += (Ids == "" ? "" : ",") + item.ID.ToString();
                    }
                    if(Ids!="")
                        mapper.Delete("DeleteT_GelStepByIDs", Ids);
                }
                if (Gel.ResultMaps != null)
                {
                    String Ids = "";
                    foreach (var item in Gel.ResultMaps)
                    {
                        Ids += (Ids == "" ? "" : ",") + item.ID.ToString();
                    }
                    if (Ids != "")
                        mapper.Delete("DeleteT_ResultMapByIDs", Ids);
                }
                mapper.Delete("DeleteT_Gel", Gel);
                mapper.CommitTransaction();
                result = true;
            }
            catch (Exception ex)
            {
                Tool.AppLogError(ex);
                result = false;
                mapper.RollBackTransaction();
            }
            return result;
        }
        public bool DeleteGelStep(T_GelStep t_GelStep)
        {
            return DeleteGelStepByIDs(t_GelStep.ID.ToString());
        }
        public bool DeleteGelResutMap(T_ResultMap t_ResultMap)
        {
            return DeleteGelResutMapByIDs(t_ResultMap.ID.ToString());
        }
        private bool DeleteGelStepByIDs(String Ids)
        {
            return mapper.Delete("DeleteT_GelStepByIDs", Ids)>0;
        }
        private bool DeleteGelResutMapByIDs(String Ids)
        {
            return mapper.Delete("DeleteT_ResultMapByIDs", Ids)>0;
        }
    }
}
