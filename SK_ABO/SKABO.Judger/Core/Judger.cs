using SKABO.Common.Models.Judger;
using SKABO.Common.Utils;
using SKABO.Judger.Core.Models;
using SKABO.Judger.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Judger.Core
{
    public class Judger:IDisposable
    {
        public Judger(Bitmap OrgBmp ,bool IsRaw, bool IsMainCalling)
        {
            this.OrgBmp = OrgBmp;
            this.IsRaw = IsRaw;
            this.IsMainCalling = IsMainCalling;
        }
        private bool IsMainCalling;//是不是主程序调用
        private Bitmap _OrgBmp;
        private bool _isRaw;
        
        public Bitmap OrgBmp { get => _OrgBmp; set => _OrgBmp = value; }
        public bool IsRaw { get => _isRaw; set => _isRaw = value; }
        public Tube[] Tubes { get => _Tubes; set => _Tubes = value; }

        private Tube[] _Tubes;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bm">相机采集到的原始图</param>
        /// <param name="TJList">测试管分割的相关参数</param>
        /// <param name="PTP">Tube分析的参数</param>
        /// <returns>返回测试结果</returns>
        public  ResultEnum[] Judge(IEnumerable<T_JudgeParamer> TJList, T_ParseTubeParameter PTP)
        {
            ResultEnum[] results = new ResultEnum[TJList.Count()];
            Tubes = new Tube[results.Length];
            for (int i = 0; i < TJList.Count(); i++)
            {
                var t = TJList.ElementAt(i);
                Bitmap Bm = ImgUtil.Cut(OrgBmp, new Point(t.StartX, t.StartY), t.EndX - t.StartX, t.EndY - t.StartY);
                {
                    Tube tube = new Tube(Bm);
                    Tubes[i] = tube;
                    results[i] = 
                        tube.RedCellJudge(PTP);
                    //tube.Judge(PTP);
                    if (!IsMainCalling)
                    {
                        if (IsRaw)
                        {
                        }
                        else
                        {
                            tube.PaintColor(tube.Bm, PTP);
                        }
                    }
                }
            }
            return results;
        }
        

        public void Dispose()
        {
            if (IsMainCalling && OrgBmp != null)
            {
                try
                {
                    this.OrgBmp.Dispose();
                }catch(Exception e)
                {

                }
            }
            if(Tubes!=null)
            foreach(var tube in Tubes)
            {
                try
                {
                    tube.Bm.Dispose();
                }
                catch (Exception e)
                {

                }
            }
            Tubes = null;
        }
    }
}
