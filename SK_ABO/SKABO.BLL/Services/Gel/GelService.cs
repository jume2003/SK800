using SKABO.BLL.IServices.IGel;
using SKABO.Common.Models.GEL;
using SKABO.DAL.IDAO.IGEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SKABO.Common.Utils;

namespace SKABO.BLL.Services.Gel
{
    public class GelService : IGelService
    {
        private IGELDAO gelDAO;
        public GelService(IGELDAO gelDAO)
        {
            this.gelDAO = gelDAO;
        }

        public T_Gel CopyGel(T_Gel SourceGel)
        {
            T_Gel NewGel = new T_Gel();
            TransExpUtil<T_Gel, T_Gel>.CopyValue(SourceGel, NewGel);
            NewGel.ID = 0;
            if (SourceGel.ResultMaps != null) {
                NewGel.ResultMaps = new List<T_ResultMap>();
                foreach(var item in SourceGel.ResultMaps)
                {
                    var obj = TransExpUtil<T_ResultMap, T_ResultMap>.Trans(item);
                    obj.ID = 0;
                    NewGel.ResultMaps.Add(obj);
                }
                
            }
            if (SourceGel.GelSteps != null)
            {
                NewGel.GelSteps = new List<T_GelStep>();
                foreach (var item in SourceGel.GelSteps)
                {
                    var obj = TransExpUtil<T_GelStep, T_GelStep>.Trans(item);
                    obj.ID = 0;
                    NewGel.GelSteps.Add(obj);
                }

            }
            gelDAO.InserOrUpdateGEL(NewGel);
            return NewGel;
        }

        public bool DeleteGel(T_Gel Gel)
        {
            return gelDAO.DeleteGel(Gel);
        }
        public bool DeleteGelStep(T_GelStep t_GelStep)
        {
            return gelDAO.DeleteGelStep(t_GelStep);
        }
        public bool DeleteGelResutMap(T_ResultMap t_ResultMap)
        {
            return gelDAO.DeleteGelResutMap(t_ResultMap);
        }

        public bool InserOrUpdateGEL(T_Gel t_Gel)
        {
            return gelDAO.InserOrUpdateGEL(t_Gel);
        }

        public IList<T_Gel> QueryAllGel()
        {
            return gelDAO.QueryAllGel();
        }

        public IList<T_StepDefine> QueryAllStepDefine()
        {
            return gelDAO.QueryAllStepDefine();
        }

        public IList<T_StepDefine> QueryStepDefineByClass(int StepClass)
        {
            return gelDAO.QueryStepDefineByClass(StepClass);
        }

        public IList<T_GelStep> QueryGelStepByGelId(int GelID)
        {
            return gelDAO.QueryGelStepByGelId(GelID);
        }

        public IList<T_ResultMap> QueryResMapByGelId(int GelID)
        {
            return gelDAO.QueryResMapByGelId(GelID);
        }

    }
}
