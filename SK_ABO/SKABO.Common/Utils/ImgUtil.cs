using SKABO.Common.Models.Image;
using SKABO.Common.Models.Judger;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SKABO.Common.Utils
{
    public class ImgUtil
    {
        public static Bitmap Rotate(Bitmap SourceBitmap, float Angle)
        {
            try
            {
                //获取当前窗口的中心点
                Rectangle rect = new Rectangle(0, 0, SourceBitmap.Width, SourceBitmap.Height);
                PointF center = new PointF(rect.Width / 2, rect.Height / 2);

                float offsetX = 0;
                float offsetY = 0;
                offsetX = center.X - SourceBitmap.Width / 2;
                offsetY = center.Y - SourceBitmap.Height / 2;
                //构造图片显示区域:让图片的中心点与窗口的中心点一致
                RectangleF picRect = new RectangleF(offsetX, offsetY, SourceBitmap.Width, SourceBitmap.Height);
                PointF Pcenter = new PointF(picRect.X + picRect.Width / 2,
                    picRect.Y + picRect.Height / 2);
                Bitmap bmpSrc = new Bitmap(SourceBitmap.Width, SourceBitmap.Height);//旋转后的图像
                using (Graphics g = Graphics.FromImage(bmpSrc))
                {
                    g.TranslateTransform(Pcenter.X, Pcenter.Y);
                    g.RotateTransform(Angle);//旋转图像,参数为角度
                    //恢复绘图平面在水平和垂直方向的平移
                    g.TranslateTransform(-Pcenter.X, -Pcenter.Y);

                    g.DrawImage(SourceBitmap, 0, 0);
                    g.Dispose();
                }
                return bmpSrc;
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {

            }

            return null;
        }
        /// <summary>
        /// 增亮
        /// </summary>
        /// <param name="b"></param>
        /// <param name="nBrightness">-255 to 255</param>
        /// <returns></returns>
        public static bool Brightness(Bitmap b, int nBrightness)
        {

            if (nBrightness < -255 || nBrightness > 255)

                return false;

            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width,

                                              b.Height), ImageLockMode.ReadWrite,

                                              PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;

            System.IntPtr Scan0 = bmData.Scan0;

            int nVal = 0;

            unsafe
            {

                byte* p = (byte*)(void*)Scan0;

                int nOffset = stride - b.Width * 3;

                int nWidth = b.Width * 3;

                for (int y = 0; y < b.Height; ++y)
                {

                    for (int x = 0; x < nWidth; ++x)
                    {

                        nVal = (int)(p[0] + nBrightness);

                        if (nVal < 0) nVal = 0;

                        if (nVal > 255) nVal = 255;

                        p[0] = (byte)nVal;

                        ++p;

                    }

                    p += nOffset;

                }

            }

            b.UnlockBits(bmData);

            return true;

        }
        #region 灰阶
        /// <summary>   
        /// 灰阶   
        /// </summary>   
        /// <param name="b">Bitmap对象</param>   
        /// <returns></returns>    
        public static Bitmap Gray(Bitmap b)
        {
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int stride = bmData.Stride;
            System.IntPtr Scan0 = bmData.Scan0;
            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - b.Width * 3;
                byte red, green, blue;
                for (int y = 0; y < b.Height; ++y)
                {
                    for (int x = 0; x < b.Width; ++x)
                    {
                        blue = p[0];
                        green = p[1];
                        red = p[2];
                        //if((red>=blue && red>=green) || (red<blue && red<green&& Math.Abs(red-blue)<=10 && Math.Abs(red-green)<=10)){
                        //    p[0] = p[1] = (byte)(.299 * red + .587 * green + .114 * blue);
                        //    p[2] = p[0] < 80 ? (byte)Math.Min(p[0] + (byte)10, (byte)255) : p[0];
                        //}else{
                        p[0] = p[1] = p[2] = (byte)(.299 * red + .587 * green + .114 * blue);
                        //}
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            b.UnlockBits(bmData);
            return b;
        }
        #endregion

        #region 固定阈值法二值化模块

        public static Bitmap Threshoding(Bitmap b, byte threshold)
        {
            int width = b.Width;
            int height = b.Height;
            BitmapData data = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* p = (byte*)data.Scan0;
                int offset = data.Stride - width * 4;
                byte R, G, B, gray;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        R = p[2];
                        G = p[1];
                        B = p[0];
                        gray = (byte)((R * 19595 + G * 38469 + B * 7472) >> 16);
                        if (gray >= threshold)
                        {
                            p[0] = p[1] = p[2] = 255;
                        }
                        else
                        {
                            p[0] = p[1] = p[2] = 0;
                        }
                        p += 4;
                    }
                    p += offset;
                }
                b.UnlockBits(data);
                return b;
            }

        }
        #endregion

        #region Otsu阈值法二值化模块
        /// <summary>   
        /// Otsu阈值   
        /// </summary>   
        /// <param name="b">位图流</param>   
        /// <returns></returns>   
        public static Bitmap OtsuThreshold(Bitmap b)
        {
            // 图像灰度化   
            // b = Gray(b);   
            int width = b.Width;
            int height = b.Height;
            byte threshold = 0;
            int[] hist = new int[256];

            int AllPixelNumber = 0, PixelNumberSmall = 0, PixelNumberBig = 0;
            double MaxValue, AllSum = 0, SumSmall = 0, SumBig, ProbabilitySmall, ProbabilityBig, Probability;

            BitmapData data = b.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* p = (byte*)data.Scan0;
                int offset = data.Stride - width * 4;

                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        hist[p[0]]++;


                        p += 4;
                    }
                    p += offset;
                }
                b.UnlockBits(data);

            }
            //计算灰度为I的像素出现的概率   
            for (int i = 0; i < 256; i++)
            {

                AllSum += i * hist[i];     //   质量矩   
                AllPixelNumber += hist[i];  //  质量       

            }

            MaxValue = -0;
            for (int i = 0; i < 256; i++)
            {
                PixelNumberSmall += hist[i];
                PixelNumberBig = AllPixelNumber - PixelNumberSmall;
                if (PixelNumberBig == 0)
                {
                    break;
                }

                SumSmall += i * hist[i];
                SumBig = AllSum - SumSmall;
                ProbabilitySmall = SumSmall / PixelNumberSmall;
                ProbabilityBig = SumBig / PixelNumberBig;
                Probability = PixelNumberSmall * ProbabilitySmall * ProbabilitySmall + PixelNumberBig * ProbabilityBig * ProbabilityBig;
                if (Probability > MaxValue)
                {
                    MaxValue = Probability;
                    threshold = (byte)i;
                }

            }

            return ImgUtil.Threshoding(b, threshold);
        } // end of OtsuThreshold 2  
        #endregion
        /**/
        /**/
        /// <summary>
        /// 锐化
        /// </summary>
        /// <param name="b">原始Bitmap</param>
        /// <param name="val">锐化程度。取值[0,1]。值越大锐化程度越高</param>
        /// <returns>锐化后的图像</returns>
        public static Bitmap KiSharpen(Bitmap b, float val)
        {
            if (b == null)
            {
                return null;
            }

            int w = b.Width;
            int h = b.Height;

            try
            {

                Bitmap bmpRtn = new Bitmap(w, h, PixelFormat.Format24bppRgb);

                BitmapData srcData = b.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                BitmapData dstData = bmpRtn.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                unsafe
                {
                    byte* pIn = (byte*)srcData.Scan0.ToPointer();
                    byte* pOut = (byte*)dstData.Scan0.ToPointer();
                    int stride = srcData.Stride;
                    byte* p;

                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            //取周围9点的值。位于边缘上的点不做改变。
                            if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                            {
                                //不做
                                pOut[0] = pIn[0];
                                pOut[1] = pIn[1];
                                pOut[2] = pIn[2];
                            }
                            else
                            {
                                int r1, r2, r3, r4, r5, r6, r7, r8, r0;
                                int g1, g2, g3, g4, g5, g6, g7, g8, g0;
                                int b1, b2, b3, b4, b5, b6, b7, b8, b0;

                                float vR, vG, vB;

                                //左上
                                p = pIn - stride - 3;
                                r1 = p[2];
                                g1 = p[1];
                                b1 = p[0];

                                //正上
                                p = pIn - stride;
                                r2 = p[2];
                                g2 = p[1];
                                b2 = p[0];

                                //右上
                                p = pIn - stride + 3;
                                r3 = p[2];
                                g3 = p[1];
                                b3 = p[0];

                                //左侧
                                p = pIn - 3;
                                r4 = p[2];
                                g4 = p[1];
                                b4 = p[0];

                                //右侧
                                p = pIn + 3;
                                r5 = p[2];
                                g5 = p[1];
                                b5 = p[0];

                                //右下
                                p = pIn + stride - 3;
                                r6 = p[2];
                                g6 = p[1];
                                b6 = p[0];

                                //正下
                                p = pIn + stride;
                                r7 = p[2];
                                g7 = p[1];
                                b7 = p[0];

                                //右下
                                p = pIn + stride + 3;
                                r8 = p[2];
                                g8 = p[1];
                                b8 = p[0];

                                //自己
                                p = pIn;
                                r0 = p[2];
                                g0 = p[1];
                                b0 = p[0];

                                vR = (float)r0 - (float)(r1 + r2 + r3 + r4 + r5 + r6 + r7 + r8) / 8;
                                vG = (float)g0 - (float)(g1 + g2 + g3 + g4 + g5 + g6 + g7 + g8) / 8;
                                vB = (float)b0 - (float)(b1 + b2 + b3 + b4 + b5 + b6 + b7 + b8) / 8;

                                vR = r0 + vR * val;
                                vG = g0 + vG * val;
                                vB = b0 + vB * val;

                                if (vR > 0)
                                {
                                    vR = Math.Min(255, vR);
                                }
                                else
                                {
                                    vR = Math.Max(0, vR);
                                }

                                if (vG > 0)
                                {
                                    vG = Math.Min(255, vG);
                                }
                                else
                                {
                                    vG = Math.Max(0, vG);
                                }

                                if (vB > 0)
                                {
                                    vB = Math.Min(255, vB);
                                }
                                else
                                {
                                    vB = Math.Max(0, vB);
                                }

                                pOut[0] = (byte)vB;
                                pOut[1] = (byte)vG;
                                pOut[2] = (byte)vR;

                            }

                            pIn += 3;
                            pOut += 3;
                        }// end of x

                        pIn += srcData.Stride - w * 3;
                        pOut += srcData.Stride - w * 3;
                    } // end of y
                }

                b.UnlockBits(srcData);
                bmpRtn.UnlockBits(dstData);

                return bmpRtn;
            }
            catch
            {
                return null;
            }

        } // end of KiSharpen

        /// <summary>
        /// 中值滤波 窗口大小必须为奇数
        /// </summary>
        /// <param name="srcBmp"></param>
        /// <param name="winSize"></param>
        /// <returns></returns>
        public static Bitmap medianFilter(Bitmap srcBmp, int winSize)
        {
            Bitmap tarBmp = new Bitmap(srcBmp.Width, srcBmp.Height); //生成的图像

            int height = srcBmp.Height;
            int width = srcBmp.Width;

            Color[,] pixels = new Color[height, width];
            //拷贝颜色
            for (int row = 0; row < height; row++)
                for (int col = 0; col < width; col++)
                    pixels[row, col] = srcBmp.GetPixel(col, row);

            //处理图像
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    if (col < winSize / 2 || row < winSize / 2 ||
                        col >= width - winSize / 2 || row >= height - winSize / 2)
                    { //假如是边缘的话，不改变
                        tarBmp.SetPixel(col, row, pixels[row, col]);
                        continue;
                    }

                    //将窗格内数据存入数组
                    Color[] array = new Color[winSize * winSize];
                    int index = 0;
                    for (int m = 0; m < winSize; m++)
                        for (int n = 0; n < winSize; n++)
                            array[index++] = pixels[row - winSize / 2 + m, col - winSize / 2 + n];

                    //数组排序，找出中位数，然后设定新图像
                    tarBmp.SetPixel(col, row, findMedianColor(array));
                }
            }

            return tarBmp;
        }
        //可以后期进行改造，如单独对3原色进行排序
        private static Color findMedianColor(Color[] src)
        {
            int[] arr = new int[src.Length];
            for (int i = 0; i < src.Length; i++)
                arr[i] = src[i].ToArgb();

            Array.Sort(arr);
            int color = arr[arr.Length / 2];
            return Color.FromArgb(color);
        }
        public static void medianFilter1(Bitmap boxTwo)
        {

            //boxTwo是要处理的图片(bitmap对象)
            Bitmap srcBmp = (Bitmap)boxTwo.Clone();
            BitmapData bmData = boxTwo.LockBits(new Rectangle(0, 0, boxTwo.Width, boxTwo.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData bmSrcData = srcBmp.LockBits(new Rectangle(0, 0, srcBmp.Width, srcBmp.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            int stride2 = bmData.Stride * 2;
            System.IntPtr scan0 = bmData.Scan0;
            System.IntPtr srcScan0 = bmSrcData.Scan0;

            unsafe
            {
                byte* p = (byte*)scan0;
                byte* pSrc = (byte*)srcScan0;
                int nWidth = boxTwo.Width - 2;
                int nHeight = boxTwo.Height - 2;
                int nOffset = stride - boxTwo.Width * 3;

                int nPixel;
                List<int> array = new List<int>();
                for (int y = 0; y < nHeight; y++)
                {
                    for (int x = 0; x < nWidth; x++)
                    {
                        /*清空数组*/
                        array.Clear();
                        array.Add(pSrc[2]);
                        array.Add(pSrc[5]);
                        array.Add(pSrc[8]);
                        array.Add(pSrc[2 + stride]);
                        array.Add(pSrc[5 + stride]);
                        array.Add(pSrc[8 + stride]);
                        array.Add(pSrc[2 + stride2]);
                        array.Add(pSrc[5 + stride2]);
                        array.Add(pSrc[8 + stride2]);
                        /*对数据进行大小排序*/
                        array.Sort();
                        nPixel = array[array.Count / 2];
                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        /*对像素进行赋值*/
                        pSrc[5 + stride] = (byte)nPixel;

                        array.Clear();
                        array.Add(pSrc[1]);
                        array.Add(pSrc[4]);
                        array.Add(pSrc[7]);
                        array.Add(pSrc[1 + stride]);
                        array.Add(pSrc[4 + stride]);
                        array.Add(pSrc[7 + stride]);
                        array.Add(pSrc[1 + stride2]);
                        array.Add(pSrc[4 + stride2]);
                        array.Add(pSrc[7 + stride2]);

                        /*对数据进行大小排序*/
                        array.Sort();
                        nPixel = array[array.Count / 2];
                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        /*对像素进行赋值*/
                        pSrc[4 + stride] = (byte)nPixel;

                        array.Clear();
                        array.Add(pSrc[0]);
                        array.Add(pSrc[3]);
                        array.Add(pSrc[6]);
                        array.Add(pSrc[0 + stride]);
                        array.Add(pSrc[3 + stride]);
                        array.Add(pSrc[6 + stride]);
                        array.Add(pSrc[0 + stride2]);
                        array.Add(pSrc[3 + stride2]);
                        array.Add(pSrc[6 + stride2]);

                        /*对数据进行大小排序*/
                        array.Sort();
                        nPixel = array[array.Count / 2];
                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        /*对像素进行赋值*/
                        pSrc[3 + stride] = (byte)nPixel;

                        p += 3;
                        pSrc += 3;
                    }/*inner for*/
                    p += nOffset;
                    p += nOffset;
                }/*outer for*/
            }/*unsafe*/
            boxTwo.UnlockBits(bmData);
            srcBmp.UnlockBits(bmSrcData);
            boxTwo = srcBmp;

        }
        public static Bitmap Cut( Bitmap Img, System.Drawing.Point StartPoint, int rectW, int rectH)
        {
            Bitmap bmSmall = new Bitmap(rectW, rectH, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            using (Graphics grSmall = Graphics.FromImage(bmSmall))
            {
                grSmall.DrawImage(Img,
                                  new System.Drawing.Rectangle(new System.Drawing.Point(0, 0), new System.Drawing.Size(bmSmall.Width, bmSmall.Height)),
                                  new System.Drawing.Rectangle(StartPoint, new System.Drawing.Size(bmSmall.Width, bmSmall.Height)),
                                  GraphicsUnit.Pixel);
                grSmall.Dispose();
            }
            return bmSmall;
        }
        /// <summary>
        /// ImageSource --> Bitmap
        /// </summary>
        /// <param name="imageSource"></param>
        /// <returns></returns>        
        public static System.Drawing.Bitmap ImageSourceToBitmap(System.Windows.Media.ImageSource imageSource)
        {
            BitmapSource m = (BitmapSource)imageSource;

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(m.PixelWidth, m.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb); // 坑点：选Format32bppRgb将不带透明度

            System.Drawing.Imaging.BitmapData data = bmp.LockBits(
            new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            m.CopyPixels(System.Windows.Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);

            return bmp;
        }
        /// <summary>
        /// 解析数码管中的数字
        /// </summary>
        /// <param name="Src">原始图片</param>
        /// <param name="TJList">分析管所在的位置参数</param>
        /// <param name="TH">图像二值华阀值</param>
        /// <param name="BrushW">LED笔画的宽度</param>
        /// <returns></returns>
        public static string AnalsyAreaDigitalTube(Bitmap Src,IEnumerable<Models.Judger.T_JudgeParamer> TJList,byte TH, int BrushW,out int[] vals)
        {
            vals = new int[TJList.Count()];
            try
            {
                string str = string.Empty;
                for (int i = 0; i < TJList.Count(); i++)
                {
                    T_JudgeParamer j = TJList.ElementAt(i);
                    Bitmap bm = Cut(Src, new Point(j.StartX, j.StartY), j.EndX - j.StartX, j.EndY - j.StartY);
                    bm = Threshoding(bm, TH);
                    int area0 = 0, area1 = 0, area2 = 0, area3 = 0, area4 = 0, area5 = 0, area6 = 0;

                    int width = bm.Width;
                    int height = bm.Height;
                    BitmapData data = bm.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                    unsafe
                    {
                        byte* p = (byte*)data.Scan0;
                        int offset = data.Stride - width * 4;
                        int MiddX = width / 2,MiddY = height / 2;
                        for (int y = 0; y < height; y++)
                        {
                            bool IsBreaked = false;
                            for (int x = 0; x < width; x++)
                            {

                                if (p[0] == 255)
                                {
                                    ///分布在垂直中线附近
                                    if (Math.Abs(x - MiddX) < BrushW/2)
                                    {
                                        if (Math.Abs(y - MiddY) <= BrushW / 2)
                                        {
                                            area3++;
                                        }else if (y > MiddY && x<MiddX)
                                        {
                                            area6++;
                                        }
                                        else if (y < MiddY && x > MiddX)
                                        {
                                            area0++;
                                        }
                                    }else if (x > MiddX)//分布在右边
                                    {
                                        int YDiff = Math.Abs(y - MiddY);
                                        if (YDiff >= BrushW && YDiff<=BrushW*1.5)
                                        {
                                             if (y > MiddY)
                                            {
                                                area5++;
                                            }
                                            else
                                            {
                                                area2++;
                                            }
                                        }
                                       

                                    }
                                    else//分布在左边
                                    {
                                        int YDiff = Math.Abs(y - MiddY);
                                        if (YDiff >= BrushW && YDiff <= BrushW * 1.5)
                                        {
                                            if (y > MiddY)
                                            {
                                                area4++;
                                            }
                                            else
                                            {
                                                area1++;
                                            }
                                        }
                                        
                                    }

                                    
                                }
                                p += 4;
                            }
                            if (IsBreaked)
                                break;
                            p += offset;
                        }
                        bm.UnlockBits(data);
                    }
                    bm.Dispose();
                    int MinCount = 5;
                    if(area0>MinCount && area1>MinCount && area2 > MinCount && area3 <= MinCount && area4 > MinCount && area5 > MinCount && area6 > MinCount)
                    {
                        vals[i]= 0;
                    }else if(area0 <= MinCount && area1 <= MinCount && area2 > MinCount && area3<= MinCount && area4 <= MinCount && area5 > MinCount && area6 <= MinCount)
                    {
                        vals[i]= 1;
                    }
                    else if (area0 > MinCount && area1 <= MinCount && area2 > MinCount && area3 > MinCount && area4 > MinCount && area5 <= MinCount && area6 > MinCount)
                    {
                        vals[i]= 2;
                    }
                    else if (area0 > MinCount && area1 <= MinCount && area2 > MinCount && area3 > MinCount && area4 <= MinCount && area5 > MinCount && area6 > MinCount)
                    {
                        vals[i]= 3;
                    }
                    else if (area0 <= MinCount && area1 > MinCount && area2 > MinCount && area3 > MinCount && area4 <= MinCount && area5 > MinCount && area6 <= MinCount)
                    {
                        vals[i]= 4;
                    }
                    else if (area0 > MinCount && area1 > MinCount && area2 <= MinCount && area3 > MinCount && area4 <= MinCount && area5 > MinCount && area6 > MinCount)
                    {
                        vals[i]= 5;
                    }
                    else if (area0 > MinCount && area1 > MinCount && area2 <= MinCount && area3 > MinCount && area4 > MinCount && area5 > MinCount && area6 > MinCount)
                    {
                        vals[i]= 6;
                    }
                    else if (area0 > MinCount && area1 <= MinCount && area2 > MinCount && area3 <= MinCount && area4 < MinCount && area5 > MinCount && area6 <= MinCount)
                    {
                        vals[i]= 7;
                    }
                    else if (area0 > MinCount && area1 > MinCount && area2 > MinCount && area3 > MinCount && area4 > MinCount && area5 > MinCount && area6 > MinCount)
                    {
                        vals[i]= 8;
                    }
                    else if (area0 > MinCount && area1 > MinCount && area2 > MinCount && area3 > MinCount && area4 <= MinCount && area5 > MinCount && area6 > MinCount)
                    {
                        vals[i]= 9;
                    }
                    str += vals[i];
                }
                
                return str;
            }
            catch (Exception exception)
            {
                Tool.AppLogError(exception);
                return "FFF";
            }
        }
        // Bitmap --> BitmapImage
        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png); // 坑点：格式选Bmp时，不带透明度

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }
        
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource BitmapToBitmapSource(Bitmap bmp)
        {
            try
            {
                var ptr = bmp.GetHbitmap();
                var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr, IntPtr.Zero, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(ptr);
                return source;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Convert RGB colorspace to HSI colorspace
        /// </summary>
        /// <param name="rgb">Input RGB pixel</param>
        /// <returns>HSI colorspace pixel</returns>
        public static HSI RGB2HSI(RGB rgb)
        {
            var c = rgb.Color;
            HSI hsi = new HSI(c.GetHue(),c.GetSaturation(),c.GetBrightness());
            
            //double r = (rgb.Red / 255.0);
            //double g = (rgb.Green / 255.0);
            //double b = (rgb.Blue / 255.0);

            //double theta = Math.Acos(0.5 * ((r - g) + (r - b)) / Math.Sqrt((r - g) * (r - g) + (r - b) * (g - b))) / (2 * Math.PI);

            //hsi.Hue = (b <= g) ? theta : (1 - theta);

            //hsi.Saturation = 1 - 3 * Math.Min(Math.Min(r, g), b) / (r + g + b);

            //hsi.Intensity = (r + g + b) / 3;

            return hsi;
        }

        /// <summary>
        /// Convert HSI colorspace to RGB colorspace
        /// </summary>
        /// <param name="hsi">Input HSI pixel</param>
        /// <returns>RGB colorspace pixel</returns>
        public static RGB HSI2RGB( HSI hsi)
        {
            double r, g, b;

            double h = hsi.Hue;
            double s = hsi.Saturation;
            double i = hsi.Intensity;

            h = h * 2 * Math.PI;

            if (h >= 0 && h < 2 * Math.PI / 3)
            {
                b = i * (1 - s);
                r = i * (1 + s * Math.Cos(h) / Math.Cos(Math.PI / 3 - h));
                g = 3 * i - (r + b);
            }
            else if (h >= 2 * Math.PI / 3 && h < 4 * Math.PI / 3)
            {
                r = i * (1 - s);
                g = i * (1 + s * Math.Cos(h - 2 * Math.PI / 3) / Math.Cos(Math.PI - h));
                b = 3 * i - (r + g);
            }
            else //if (h >= 4 * Math.PI / 3 && h <= 2 * Math.PI)
            {
                g = i * (1 - s);
                b = i * (1 + s * Math.Cos(h - 4 * Math.PI / 3) / Math.Cos(5 * Math.PI / 3 - h));
                r = 3 * i - (g + b);
            }

            return new RGB((byte)(r * 255.0 + .5), (byte)(g * 255.0 + .5), (byte)(b * 255.0 + .5));
        }
        public static byte[] BitmapToBytes(Bitmap bm)
        {
            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream();
                bm.Save(ms, ImageFormat.Jpeg);
                byte[] byteImage = new Byte[ms.Length];
                byteImage = ms.ToArray();
                return byteImage;
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            finally
            {
                ms.Close();
            }
        }
        public static Bitmap BytesToBitmap(byte[] Bytes)
        {
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream(Bytes);
                return new Bitmap(stream);
            }
            catch (ArgumentNullException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            finally
            {
                stream.Close();
            }
        }
        }
}
