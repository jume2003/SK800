using SKABO.BLL.IServices.IJudger;
using SKABO.Common.Enums;
using SKABO.Common.Models.GEL;
using SKABO.Common.Models.Judger;
using SKABO.Common.Parameters.Judger;
using SKABO.DAL.IDAO.IJudger;
using SKABO.Judger.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SKABO.BLL.Services.Judger
{
    public class ResultService : IResultService
    {
        private IResultDAO ResultDAO;
        public ResultService(IResultDAO resultDAO)
        {
            this.ResultDAO = resultDAO;
        }
        public ResultEnum[] GetResultEnums(T_Picture pic, byte StartNo, byte Total)
        {
            IList<ResultEnum> results = new List<ResultEnum>();
            for(var i = StartNo; i < StartNo + Total; i++)
            {
                int res = 0;
                switch (i)
                {
                    case 0:
                        {
                            res = pic.T1;
                            break;
                        }
                    case 1:
                        {
                            res = pic.T2;
                            break;
                        }
                    case 2:
                        {
                            res = pic.T3;
                            break;
                        }
                    case 3:
                        {
                            res = pic.T4;
                            break;
                        }
                    case 4:
                        {
                            res = pic.T5;
                            break;
                        }
                    case 5:
                        {
                            res = pic.T6;
                            break;
                        }
                    case 6:
                        {
                            res = pic.T7;
                            break;
                        }
                    case 7:
                        {
                            res = pic.T8;
                            break;
                        }
                }
                results.Add((ResultEnum)res);
            }
            return results.ToArray();
        }
        /// <summary>
        /// 最终的结果描述
        /// </summary>
        /// <param name="Gel"></param>
        /// <param name="Results">单管结果集</param>
        /// <returns>返回最终的结果描述</returns>
        public string FinishResult(T_Gel Gel, out String Color, params ResultEnum[] Results)
        {
            Color = null;
            if (Results == null) return "?";
            String Res = "";
            String ComRes = "";
            for(int i = 0; i < Results.Length; i++)
            {
                int ResInt = (int)Results[i];
                if (ResInt >= 0)
                {
                    ComRes += "+";
                }
                else if(ResInt==-1)
                {
                    ComRes += "-";
                }
                else
                {
                    Res = "?";
                    break;
                }
            }
            if (!String.IsNullOrEmpty(Res)) return Res;
            String Brush = null;
            GetResultStr(ComRes, Gel, BloodSystemEnum.ABO,ref Res,out Brush);
            if (!String.IsNullOrEmpty(Brush)) Color = Brush;
            if (Res != "?")
            {
                GetResultStr(ComRes, Gel, BloodSystemEnum.CDE,ref Res, out Brush);
                if (!String.IsNullOrEmpty(Brush)) Color = Brush;
            }
            if (Res != "?")
            {
                GetResultStr(ComRes, Gel, BloodSystemEnum.Globulin, ref Res, out Brush);
                if (!String.IsNullOrEmpty(Brush)) Color = Brush;
            }
            if (Res != "?")
            {
                GetResultStr(ComRes, Gel, BloodSystemEnum.Other, ref Res, out Brush);
                if (!String.IsNullOrEmpty(Brush)) Color = Brush;
            }
            if (Res == "") Res = "?";
            return Res;
        }
        private void GetResultStr(String ComRes, T_Gel Gel,BloodSystemEnum BE,ref String Res,out String Color)
        {
            Color = null;
            var ResultMaps = Gel.ResultMaps.Where(c => c.BloodSystem == BE).ToList();
            if (ResultMaps != null && ResultMaps.Count() > 0)
            {
                var map = GetResultMap(ComRes, ResultMaps);
                if (map == null)
                {
                    Res = "?";
                }
                else
                {
                    Res += map.ResultDesc;
                    Color = map.Color;
                }
            }
        }
        private T_ResultMap GetResultMap(String ComRes,List<T_ResultMap> ResultMaps)
        {
            T_ResultMap Res = null;
            foreach (var item in ResultMaps)
            {
                String map = item.ResultMap;
                bool IsMatch = true;
                if (map.Length == ComRes.Length)
                {
                    for (int i = 0; i < map.Length; i++)
                    {
                        if (map.ElementAt(i) != '*' && map.ElementAt(i) != ComRes.ElementAt(i))
                        {
                            IsMatch = false;
                            break;
                        }
                    }
                }
                else
                {
                    IsMatch = false;
                }
                if (IsMatch)
                {
                    Res = item;
                    break;
                }
            }
            return Res;
        }

        public IList<T_Result> QueryT_Result(ResultParameter resultParameter)
        {
            if (resultParameter.EndTime.HasValue)
                resultParameter.RealEndTime = resultParameter.EndTime.Value.AddDays(1);
            return ResultDAO.QueryT_Result(resultParameter);
        }

        public bool SaveT_Result(T_Result result)
        {
            return ResultDAO.SaveT_Result(result);
        }

        public bool UpdateT_Result(T_Result result)
        {
            return ResultDAO.UpdateT_Result(result);
        }
    }
}
