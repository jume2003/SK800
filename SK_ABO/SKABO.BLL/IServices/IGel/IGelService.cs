using SKABO.Common.Models.GEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.BLL.IServices.IGel
{
    public interface IGelService
    {
        bool InserOrUpdateGEL(T_Gel t_Gel);
        IList<T_Gel> QueryAllGel();
        IList<T_StepDefine> QueryAllStepDefine();
        IList<T_GelStep> QueryGelStepByGelId(int GelID);
        IList<T_StepDefine> QueryStepDefineByClass(int StepClass);
        IList<T_ResultMap> QueryResMapByGelId(int GelID);
        

        T_Gel CopyGel(T_Gel SourceGel);
        bool DeleteGel(T_Gel Gel);
        bool DeleteGelStep(T_GelStep t_GelStep);
        bool DeleteGelResutMap(T_ResultMap t_ResultMap);
    }
}
