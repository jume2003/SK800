using SKABO.Common;
using SKABO.Common.Enums;
using SKABO.Common.Models.GEL;
using SKABO.Common.Models.Judger;
using SKABO.Common.Models.NotDuplex;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SKABO.Hardware.Model
{ /// <summary>
  /// 测试包对象
  /// </summary>
    public class TestBag
    {
        /// <summary>
        /// 一个测试表最大卡数，12
        /// </summary>
        public static readonly byte MaxCount=12;
        public TestBag(TestLevelEnum testLevel)
        {
            this.TestLevel = testLevel;

        }
        public TestLevelEnum TestLevel { get; set; }
        /// <summary>
        /// 测试项目卡信息
        /// </summary>
        public T_Gel GelType { get; set; }
        private IList<ValueTuple<String, byte, byte, T_Result>> _SamplesInfo;
        /// <summary>
        /// T1:样本号，T2，载架号，T3，位置号，T4,测试结果
        /// </summary>
        public IList<ValueTuple<String, byte, byte, T_Result>> SamplesInfo
        {
            get
            {
                if (_SamplesInfo == null)
                {
                    _SamplesInfo = new List<ValueTuple<String, byte, byte, T_Result>>();
                }
                return _SamplesInfo;
            }
        }
        public void Add(String SampleBarcode, byte RackIndex, byte Index)
        {
            T_Result result = new T_Result();
            result.GelID = GelType.ID;
            result.SmpBarcode = SampleBarcode;
            result.TestUser = Constants.Login.LoginName;
            var vt = ValueTuple.Create<String, byte, byte, T_Result>(SampleBarcode, RackIndex, Index, result);
            this.SamplesInfo.Add(vt);
        }
        public (String SampleBarcode, byte RackIndex, byte Index, T_Result Result) this[byte i]
        {
            get
            {
                return SamplesInfo[i];
            }
        }
        /// <summary>
        /// 记录开始时间
        /// </summary>
        public void SetStartTime()
        {
            var startTime= DateTime.Now; 
            foreach (var item in SamplesInfo)
            {
                item.Item4.StartTime = startTime;
            }
        }
        public void SetStartTime(DateTime startTime)
        {
            foreach (var item in SamplesInfo)
            {
                item.Item4.StartTime = startTime;
            }
        }
        /*
        private void InvokeSetValue(VBJ vb, byte x, byte y, Object val)
        {
            Constants.MainWindow.Dispatcher.Invoke(new Action(() => {
                vb.SetValue(x, y, val);
            }));
        }
        public static IList<TestBag> GenerateTestBag(byte GelIndex, IEnumerable<SampleInfo> infos, SKABO.Common.Enums.TestLevelEnum testLevel)
        {
            if (infos.Count() == 0) return null;
            IList<TestBag> result = new List<TestBag>();
            var GelList = (this.View as ScanSampleView).GelList;
            var gp = infos.GroupBy(item => item.RackIndex);

            var count = gp.Count();
            T_Gel GelType = GelList[GelIndex];
            for (int i = 0; i < count; i++)
            {
                var RackIndex = gp.ElementAt(i).Key;
                var CurentSR = SampleRacks.Where(item => item.Index == RackIndex).FirstOrDefault();
                bool IsFinished = true;
                for (byte x = 0; x < CurentSR.Count; x++)
                {
                    if (CurentSR.Values[x, 0] == null || sr.Values[x, 0].ToString().EndsWith(",F"))
                    {
                        Constants
                    }
                }
                TestBag testBag = new TestBag(testLevel);
                testBag.GelType = GelType;
                foreach (var s in gp.ElementAt(i))
                {
                    testBag.Add(s.Barcode, s.RackIndex, s.Index);
                    CurentSR.SetValue(s.Index, 0, s.Barcode);
                    if (testBag.SamplesInfo.Count == TestBag.MaxCount)
                    {
                        result.Add(testBag);
                        testBag = new TestBag(testLevel);
                        testBag.GelType = GelType;
                    }
                }
                if (testBag.SamplesInfo.Count < TestBag.MaxCount)
                {
                    result.Add(testBag);
                }
            }



            return result;
        }*/
    }
}

