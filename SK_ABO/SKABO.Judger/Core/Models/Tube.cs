using SKABO.Common.Models.Image;
using SKABO.Common.Models.Judger;
using SKABO.Common.Utils;
using SKABO.Judger.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Judger.Core.Models
{
    /// <summary>
    /// 单微胶柱
    /// </summary>
    public class Tube
    {
        private Bitmap _Bm;
        public int MaxY { get; set; }

        public Bitmap Bm { get => _Bm; set => _Bm = value; }

        public Tube(Bitmap Bm)
        {
            this.Bm = Bm;
            Width = Bm.Width;
            Height = Bm.Height;
        }
        public int c1 , c2, c3 , c4 , c5 , c6 ;
        private int GuanBi;
        public int Width { get; set; }
        public int Height { get; set; }
        public ResultEnum Result { get; set; }
        private IList<RedBloodCell> RBCells;
        /// <summary>
        /// 分析微胶柱阴性还是阳性
        /// </summary>
        /// <param name="PTP">分析微胶柱的参数</param>
        /// <returns>分析结果</returns>
        public ResultEnum Judge(T_ParseTubeParameter PTP)
        {
           
            ResultEnum resultEnum = ResultEnum.BadSample_Ambiguous;
            if (Bm.Width <= PTP.TestWidth)
                return resultEnum;
            GuanBi = (this.Bm.Width - PTP.TestWidth) / 2;
            
            using (var bm = ImgUtil.Threshoding(Bm.Clone() as Bitmap,Convert.ToByte(PTP.Threshold)))
            {
                resultEnum = DoDetermine(bm, PTP);
                
            }
            Result = resultEnum;
            return resultEnum;
        }
        /// <summary>
        /// 非二值化方法
        /// </summary>
        /// <param name="PTP"></param>
        /// <returns></returns>
        public ResultEnum RedCellJudge(T_ParseTubeParameter PTP)
        {

            ResultEnum resultEnum = ResultEnum.BadSample_Ambiguous;
            if (Bm.Width <= PTP.TestWidth)
                return resultEnum;
            GuanBi = (this.Bm.Width - PTP.TestWidth) / 2;
            resultEnum = DoRedCellDetermine(Bm, PTP);
            Result = resultEnum;
            return resultEnum;
        }
        private int FindBottomY(T_ParseTubeParameter PTP)
        {
            int BottomY = 0;
            using (var bm = ImgUtil.Threshoding(Bm.Clone() as Bitmap, Convert.ToByte(PTP.Threshold)))
            {
                BitmapData data = bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                unsafe
                {
                    byte* p = (byte*)data.Scan0;
                    int offset = data.Stride - Width * 3;
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++)
                        {
                            if (y > Height * 0.7)
                            {
                                if (x > GuanBi && x < Width - GuanBi)
                                {
                                    if (p[0] == 0)
                                    {
                                        BottomY = y;

                                    }
                                }
                            }
                            p += 3;
                        }
                        p += offset;
                    }
                    bm.UnlockBits(data);
                }
               
                return BottomY;

            }
        }
        private ResultEnum DoRedCellDetermine(Bitmap bm, T_ParseTubeParameter PTP)
        {
            //var dd=ImgUtil.Threshoding(bm.Clone() as Bitmap, PTP.Threshold);
            //DoDetermine(dd, PTP);

            ResultEnum resultEnum = ResultEnum.BadSample_Ambiguous;
            MaxY = FindBottomY(PTP);
            RBCells = new List<RedBloodCell>();
            BitmapData data = bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                
                byte* p = (byte*)data.Scan0;
                int offset = data.Stride - Width * 3;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (x > GuanBi && x < Width - GuanBi)
                        {
                            byte R = p[2], G = p[1], B = p[0];


                            RedBloodCell cell = new RedBloodCell()
                            {
                                Position = new int[] { x, y },
                                B = B,
                                G = G,
                                R = R
                            };
                            RGB rgb = new RGB(R, G, B);
                            HSI hsi = ImgUtil.RGB2HSI(rgb);
                            var h = hsi.Hue;// / Math.PI * 180;
                        if ((h <= Convert.ToDouble(PTP.HueMinThreshold) || h >=Convert.ToDouble(PTP.HueMaxThreshold))
                            &&hsi.Saturation>=Convert.ToDouble(PTP.SMinThreshold) && hsi.Saturation<=Convert.ToDouble(PTP.SMaxThreshold)
                            && hsi.Intensity >= Convert.ToDouble(PTP.BMinThreshold) && hsi.Saturation <= Convert.ToDouble(PTP.BMaxThreshold)
                            )// h >= 0.85 ||
                        {
                                RBCells.Add(cell);
                            }

                        }
                        p += 3;
                    }
                    p += offset;
                }
                bm.UnlockBits(data);
            }
            //bm.Dispose();
            resultEnum = DoDetermine(PTP);
            return resultEnum;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bm">二值化后的图像</param>
        /// <param name="PTP">分析微胶柱的参数</param>
        /// <returns>分析结果</returns>
        private ResultEnum DoDetermine(Bitmap bm,T_ParseTubeParameter PTP)
        {
            ResultEnum resultEnum = ResultEnum.BadSample_Ambiguous;
            
            RBCells = new List<RedBloodCell>();
            BitmapData data = bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* p = (byte*)data.Scan0;
                int offset = data.Stride - Width * 4;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (x > GuanBi && x < Width - GuanBi)
                        {
                            if (p[0] == 0)
                            {
                                RedBloodCell cell = new RedBloodCell()
                                {
                                    Position = new int[] { x, y }
                                };
                                RBCells.Add(cell);
                                MaxY = y;
                            }
                        }
                        p += 4;
                    }
                    p += offset;
                }
                bm.UnlockBits(data);
            }
            bm.Dispose();
            resultEnum = DoDetermine( PTP);
            return resultEnum;
        }
        private ResultEnum DoDetermine( T_ParseTubeParameter PTP)
        {
            
            ResultEnum resultEnum = ResultEnum.BadSample_Ambiguous;
            if (RBCells.Count == 0) return resultEnum;

            

            RBCells=RemoveBottom(RBCells, PTP, MaxY);

            RemoveLittle(RBCells);
            if (RBCells.Count < 10)
            {
                return resultEnum;
            }
            //int MinY = RBCells.Min(c => c.Position[1]);
            //int MaxX = RBCells.Max(c => c.Position[0]);
            //int MinX = RBCells.Min(c => c.Position[0]);
            //MaxY = RBCells.Max(c => c.Position[1]);

            List<RedBloodCell> layer1, layer2, layer3, layer4, layer5, layer6;
            layer1 = new List<RedBloodCell>();
            layer2 = new List<RedBloodCell>();
            layer3 = new List<RedBloodCell>();
            layer4 = new List<RedBloodCell>();
            layer5 = new List<RedBloodCell>();
            layer6 = new List<RedBloodCell>();
            int valHeight = MaxY - PTP.BottomHeight;
            foreach(var item in RBCells)
            {
                int y = item.Position[1];
                if (y <= PTP.Layer1* valHeight/100)
                {
                    layer1.Add(item);
                }else if (y <= (PTP.Layer1 + PTP.Layer2) * valHeight/100)
                {
                    layer2.Add(item);
                }
                else if (y <= (PTP.Layer1 + PTP.Layer2+ PTP.Layer3) * valHeight/100)
                {
                    layer3.Add(item);
                }
                else if (y <= (PTP.Layer1 + PTP.Layer2 + PTP.Layer3 + PTP.Layer4) * valHeight/100)
                {
                    layer4.Add(item);
                }
                else if (y <= (PTP.Layer1 + PTP.Layer2 + PTP.Layer3 + PTP.Layer4+PTP.Layer5) * valHeight/100)
                {
                    layer5.Add(item);
                }
                else if (y <= (1) * valHeight)
                {
                    layer6.Add(item);
                }
                else
                {

                }
            }
            //RBCells = null;


            //if (MaxY <= height / 4) //全部集中在上层
            //{
            //    resultEnum = ResultEnum.Positive4;
            //}else if (MinY > height / 4 * 3)//全部集中在下层
            //{
            //    resultEnum = ResultEnum.Negative;
            //}
            resultEnum = Final(layer1, layer2, layer3, layer4, layer5, layer6);

            return resultEnum;
        }
        private ResultEnum Final(List<RedBloodCell> layer1, List<RedBloodCell> layer2, List<RedBloodCell> layer3, List<RedBloodCell> layer4, List<RedBloodCell> layer5, List<RedBloodCell> layer6)
        {
            ResultEnum resultEnum = ResultEnum.BadSample_Ambiguous;
            c1 = layer1.Count; c2 = layer2.Count; c3 = layer3.Count; c4 = layer4.Count; c5 = layer5.Count; c6 = layer6.Count;
            if ((c1 > 0 && c2==0 && c3==0 && c4==0 && c5==0&& c6==0)||(c1 == 0 && c2 > 0 && c3 == 0 && c4 == 0 && c5 == 0 && c6 == 0))
            {
                resultEnum = ResultEnum.Positive4;
            }else if (c1 == 0 && c2 == 0 && c3 == 0 && c4 == 0 && c5 == 0 && c6 > 0)
            {
                resultEnum = ResultEnum.Negative;
            }else if ((c1 > 0 && c2 > 0 && c3 == 0 && c4 == 0 && c5 == 0 && c6 == 0)|| (c1 == 0 && c2 > 0 && c3 > 0 && c4 == 0 && c5 == 0 && c6 == 0))
            {
                resultEnum = ResultEnum.Positive3;
            }else if ((c1 > 0 && c2 > 0 && c3 > 0 && c4 == 0 && c5 == 0 && c6 == 0)|| (c1 == 0 && c2 > 0 && c3 > 0 && c4 > 0 && c5 == 0 && c6 == 0))
            {
                resultEnum = ResultEnum.Positive2;
            }else if (c1 == 0 && c2 == 0 && c3 == 0 && c4 > 0 && c5 > 0 )
            {
                resultEnum = ResultEnum.Positive1;
            }
            else if (c1 > 0 && c2 == 0 && c3 == 0 && c4 == 0 && c5 > 0 && c6 > 0)
            {
                resultEnum = ResultEnum.Positive;
            }else if ((c1 > 0 && c2 == 0 && c3 == 0 && c4 == 0 && c5 == 0 && c6 > 0)|| (c1 == 0 && c2 > 0 && c3 == 0 && c4 == 0 && c5 == 0 && c6 > 0))
            {
                resultEnum = ResultEnum.BadSample_DCP;
            }else if (c1 == 0 && c2 == 0 && c3 == 0 && c4 == 0 && c5 == 0 && c6 == 0)
            {
                resultEnum = ResultEnum.BadSample_H;
            }else if (c1 == 0 && c2 == 0 && c3 == 0 && c4 == 0 && c5 == 0 && c6 == 0)
            {
                resultEnum = ResultEnum.BadSample_PH;
            }
            return resultEnum;
        }
        /// <summary>
        /// 排除锥形底部的干拢
        /// </summary>
        /// <param name="RBCells"></param>
        /// <param name="PTP"></param>
        /// <param name="MaxY"></param>
        /// <returns></returns>
        private IList<RedBloodCell>  RemoveBottom(IList<RedBloodCell> RBCells, T_ParseTubeParameter PTP, int MaxY)
        {
            double TanAngle = Math.Tan(Math.PI / 180 * PTP.Angle);
            int AngleY = Convert.ToInt32(TanAngle * PTP.TestWidth / 2);
            return  RBCells.Where(c =>
            c.Position[1] <= MaxY - PTP.BottomHeight - AngleY
            || (c.Position[1] < MaxY - PTP.BottomHeight &&
            ((MaxY - PTP.BottomHeight - c.Position[1] + 10) / (c.Position[0] == Width / 2 ? 0.001 : Math.Abs(c.Position[0] - Width / 2)) > TanAngle))
            ).ToList();
        }
        /// <summary>
        /// 排除同一水平线上的干拢细胞,少于10个视为干扰
        /// </summary>
        /// <param name="RBCells"></param>
        private void RemoveLittle(IList<RedBloodCell> RBCells)
        {
            IEnumerable<IGrouping<int, RedBloodCell>> query =
                    RBCells.GroupBy(cell => cell.Position[1]);
            foreach (IGrouping<int, RedBloodCell> cellGroup in query)
            {
                int count = cellGroup.Count();
                if (cellGroup.Count() < 5)//原来为10
                {
                    foreach (var cell in cellGroup)
                    {
                        RBCells.Remove(cell);
                    }
                }
            }
        }
        public void PaintColor(Bitmap Bm,T_ParseTubeParameter PTP)
        {
            //Bm = ImgUtil.Threshoding(Bm, PTP.Threshold);
            BitmapData bmData = Bm.LockBits(new Rectangle(0, 0, Bm.Width, Bm.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - Bm.Width * 3;

                for (int y = 0; y < Bm.Height; ++y)
                {

                    for (int x = 0; x < Bm.Width; ++x)
                    {
                        
                            if (RBCells.Where(c => c.Position[0] == x && c.Position[1] == y).Count() > 0)
                            {
                                p[2] = (byte)255; p[1] = p[0] = (byte)0;
                            }
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            Bm.UnlockBits(bmData);
        }
    }
}
