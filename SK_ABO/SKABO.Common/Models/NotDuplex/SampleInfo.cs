using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.NotDuplex
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class SampleInfo
    {
        public SampleInfo() { }
        public SampleInfo(string barcode, int index, int rackindex)
        {
            Index = index;
            RackIndex = rackindex;
            Barcode = barcode;
        }
        /// <summary>
        /// 样本架号
        /// </summary>
        public int RackIndex { get; set; }
        /// <summary>
        /// 位置号
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 样本号
        /// </summary>
        public String Barcode { get; set; }
        public bool TestItem1 { get; set; }
        public bool TestItem2 { get; set; }
        public bool TestItem3 { get; set; }

        public bool TestItem4 { get; set; }
        public bool TestItem5 { get; set; }
        public bool TestItem6 { get; set; }
        public bool TestItem7 { get; set; }
        public bool TestItem8 { get; set; }
        public bool TestItem9 { get; set; }

        public bool Normal { get; set; } = true;
        public bool Fast { get; set; }
        public bool Faster { get; set; }

        /// <summary>
        /// 是否为质控项目
        /// </summary>
        public bool IsQc { get; set; }
        private bool _IsExtraUrgent;
        public bool IsExtraUrgent { get => _IsExtraUrgent; set { _IsExtraUrgent = value; if (value) { _IsUrgent = false; } } }
        private bool _IsUrgent;
        public bool IsUrgent { get=>_IsUrgent; set{ _IsUrgent = value;if (value) { _IsExtraUrgent = false; } } }
        public List<int> GetTestList()
        {
            var test_list = new List<int>();
            if (TestItem1) test_list.Add(0);
            if (TestItem2) test_list.Add(1);
            if (TestItem3) test_list.Add(2);
            if (TestItem4) test_list.Add(3);
            if (TestItem5) test_list.Add(4);
            if (TestItem6) test_list.Add(5);
            if (TestItem7) test_list.Add(6);
            if (TestItem8) test_list.Add(7);
            if (TestItem9) test_list.Add(8);
            return test_list;
        }
        public int GetLever()
        {
            bool []lever_tem = { Normal, Fast , Faster };
            for(int i=0;i<lever_tem.Length;i++)
            {
                if (lever_tem[i]) return i;
            }
            return 0;
        }
        public bool GetTestItem(int GelIndex)
        {
            bool IsSel = false;
            switch (GelIndex + 1)
            {
                case 1:
                    {
                        IsSel = TestItem1;
                        break;
                    }
                case 2:
                    {
                        IsSel = TestItem2;
                        break;
                    }
                case 3:
                    {
                        IsSel = TestItem3;
                        break;
                    }
                case 4:
                    {
                        IsSel = TestItem4;
                        break;
                    }
                case 5:
                    {
                        IsSel = TestItem5;
                        break;
                    }
                case 6:
                    {
                        IsSel = TestItem6;
                        break;
                    }
                case 7:
                    {
                        IsSel = TestItem7;
                        break;
                    }
                case 8:
                    {
                        IsSel = TestItem8;
                        break;
                    }
                case 9:
                    {
                        IsSel = TestItem9;
                        break;
                    }
            }
            return IsSel;
        }
        public void SetTestItem(int GelIndex,bool IsSel=true)
        {
            switch (GelIndex + 1)
            {
                case 1:
                    {
                        TestItem1 = IsSel;
                        break;
                    }
                case 2:
                    {
                        TestItem2 = IsSel;
                        break;
                    }
                case 3:
                    {
                        TestItem3 = IsSel;
                        break;
                    }
                case 4:
                    {
                        TestItem4 = IsSel;
                        break;
                    }
                case 5:
                    {
                        TestItem5 = IsSel;
                        break;
                    }
                case 6:
                    {
                        TestItem6 = IsSel;
                        break;
                    }
                case 7:
                    {
                        TestItem7 = IsSel;
                        break;
                    }
                case 8:
                    {
                        TestItem8 = IsSel;
                        break;
                    }
                case 9:
                    {
                        TestItem9 = IsSel;
                        break;
                    }
            }
        }
    }
}
