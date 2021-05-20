using SKABO.Common.Models.Judger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.DAL.IDAO.IJudger
{
    public interface IJudgerParamerDAO
    {
        IList<T_JudgeParamer> QueryALlParamerByMSN(String MSN);
        int InsertT_JudgeParamer(T_JudgeParamer t_JudgeParamer);
        bool UpdateT_JudgeParamer(T_JudgeParamer t_JudgeParamer);

        T_Camera QueryCamera(String MSN);

        bool UpdateOrInsertCamera(T_Camera t_Camera);
        /// <summary>
        /// 加载LED解析参数
        /// </summary>
        /// <returns></returns>
        T_ParseLEDParameter LoadPLP();
        /// <summary>
        /// 保存LED解析参数
        /// </summary>
        /// <param name="PLP"></param>
        /// <returns></returns>
        bool SavePLP(T_ParseLEDParameter PLP);
        /// <summary>
        /// 加载微胶柱解析参数
        /// </summary>
        /// <returns></returns>
        T_ParseTubeParameter LoadPTP();
        /// <summary>
        /// 保存微胶柱解析参数
        /// </summary>
        /// <param name="PLP"></param>
        /// <returns></returns>
        bool SavePTP(T_ParseTubeParameter PTP);
    }
}
