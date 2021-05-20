using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Judger.Core.Models
{
    public class RedBloodCell : BloodCell
    {
        public RedBloodCell() { }
        private bool? _IsRed;
        public RedBloodCell(int R, int G, int B) : base(R, G, B)
        { }
        public int[] Position { get; set; }
        /// <summary>
        /// 正常红细胞
        /// </summary>
        public bool IsRedBooldCell
        {
            get
            {
                bool result = _IsRed.HasValue ? _IsRed.Value : IsRed();
                return result;
            }
            set
            {
                _IsRed = false;
            }
        }
        /// <summary>
        /// 溶血性红细胞
        /// </summary>
        //public bool IsHemolyticCell
        //{
        //    get
        //    {
        //        bool result = IsHemolytic();
        //        return result;
        //    }
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="MaxVal">最大的颜色值</param>
        /// <returns></returns>
        public bool IsRed(int MaxVal)
        {
            bool result = false;
            //result = AtRange(R, 10, 80) && AtRange(B, 10, 40) && AtRange(G, 10, 40) && R > G && R > B ;// && (R - G > 10 && R - B > 10);
            //because R=B=G
            result = AtRange(R, 10, MaxVal);// && AtRange(B, 10, 50) && AtRange(G, 10, 50);
            //result = R - G > 0;
            if (result)
            {
                //if (R > 40)
                //{
                //    result=R - G > 10 && R - B>10;
                //}
            }
            IsRedBooldCell = result;
            return result;
        }
        private bool IsRed()
        {
            return IsRed(50);
        }
        private bool AtRange(int num, int min, int max)
        {
            return num >= min && num <= max;
        }
        //public bool IsHemolytic(int MaxVal)
        //{
        //    bool result = false;
        //    result = AtRange(R, MaxVal + 1, MaxVal+30);
        //    //result = AtRange(R, 90, 130) && AtRange(G, 80, 130) && AtRange(B, 70, 110);
        //    IsRedBooldCell = result;
        //    return result;
        //}
        public bool IsHemolytic(int MaxVal)
        {
            bool result = false;
            result = AtRange(R, MaxVal - 30, MaxVal);
            //result = AtRange(R, 90, 130) && AtRange(G, 80, 130) && AtRange(B, 70, 110);
            IsRedBooldCell = result;
            return result;
        }
    }
}
