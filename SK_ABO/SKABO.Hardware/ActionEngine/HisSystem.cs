using SKABO.BLL.IServices.IUser;
using SKABO.Common.Models.Duplex;
using SKABO.Common.Models.GEL;
using SKABO.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SKABO.Common.Models.Judger;
using SKABO.ResourcesManager;

namespace SKABO.ActionEngine
{
    public class RequestSample
    {
        public int rackindex;// 样本架号
        public int index;// 位置号
        public string barcode;//样本号
        public RequestSample(string barcodetem, int rackindextem, int indextem)
        {
            barcode = barcodetem;
            rackindex = rackindextem;
            index = indextem;
        }
    }

    public class TestItemSample
    {
        public int testcode;// 测试项目
        public int liscode;// LIS代码
        public string barcode;//样本号
        public int rackindex;// 样本架号
        public int index;// 位置号
        public TestItemSample(string barcodetem, int rackindextem, int indextem ,int testcodetem, int listcodetem)
        {
            barcode = barcodetem;
            testcode = testcodetem;
            liscode = listcodetem;
            rackindex = rackindextem;
            index = indextem;
        }
    }

    public class HisSystem
    {
        public string resultdir = "";
        public string duplexdir = "";
        public string request_filename = "adc-request.txt";
        public string testitem_filename = "adc-testitem.txt";
        public List<RequestSample> request_samples = new List<RequestSample>();
        public List<TestItemSample> testitem_sample = new List<TestItemSample>();
        public List<SKABO.Common.Models.NotDuplex.SampleInfo> work_sample = new List<SKABO.Common.Models.NotDuplex.SampleInfo>();
        public static HisSystem ineinstance = null;
        private Object mylock = new Object();
        public static HisSystem getInstance()
        {
            if (ineinstance == null)
            {
                ineinstance = new HisSystem();
            }
            return ineinstance;
        }
        public List<int> GetTestCode(SKABO.Common.Models.NotDuplex.SampleInfo sample)
        {
            List<int> testcodes = new List<int>();
            bool[] testitem = {sample.TestItem1, sample.TestItem2, sample.TestItem3, sample.TestItem4,
                               sample.TestItem5, sample.TestItem6, sample.TestItem7, sample.TestItem8, sample.TestItem9};
            for (int i = 0; i < 9; i++)
            {
                if (testitem[i])
                {
                    testcodes.Add(i);
                }
            }
            return testcodes;
        }
        public void SetDirs(string resultdirtem, string duplexdirtem)
        {
            resultdir = resultdirtem+"\\";
            duplexdir = duplexdirtem+"\\";
        }
        public void ClsReqSample()
        {
            lock (mylock)
            {
                request_samples.Clear();
            }
        }
        public bool AddReqSample(SKABO.Common.Models.NotDuplex.SampleInfo sample)
        {
            lock(mylock)
            {
                request_samples.Add(new RequestSample(sample.Barcode, sample.RackIndex, sample.Index));
            }
            return true;
        }
        public RequestSample GetReqSample(string barcode)
        {
            lock (mylock)
            {
                for (int i = 0; i < request_samples.Count; i++)
                {
                    if (request_samples[i].barcode == barcode)
                        return request_samples[i];
                }
            }
            return null;
        }
        public void ClsTestSample()
        {
            testitem_sample.Clear();
        }
        public bool AddTestSample(SKABO.Common.Models.NotDuplex.SampleInfo sample, IList<T_Gel> gellist)
        {
            List<int> testcodes = GetTestCode(sample);
            bool ispass = testcodes.Count != 0;
            if (ispass == false) testcodes.Add(1);
            for (int i = 0; i < testcodes.Count; i++)
            {
                int listcode = gellist[testcodes[i]].LisGelClass;
                testitem_sample.Add(new TestItemSample(sample.Barcode, sample.RackIndex, sample.Index, testcodes[i], listcode));
            }
            return ispass;
        }
        //得到系统时间
        public string GetTimeYMDHMSStr()
        {
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;

            string timestr = string.Format("{0:0000}{1:00}{2:00}{3:00}{4:00}{5:00}", currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, currentTime.Second);
            return timestr;
        }
        public string GetTestTimeStr(System.DateTime datetime)
        {
            string timestr = string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}", datetime.Year, datetime.Month, datetime.Day, datetime.Hour, datetime.Minute, datetime.Second);
            return timestr;
        }
        public string GetPicFileName(string rawfilename)
        {
            int index = rawfilename.LastIndexOf('\\')+1;
            if (index == -1) index = 0;
            string filename = rawfilename.Substring(index, rawfilename.Length- index);
            return filename;
        }
        //写待测试文本
        public void WriteRequest()
        {
            if (duplexdir == null|| duplexdir == "" || !Directory.Exists(duplexdir)) return;
            if (request_samples.Count!=0)
            {
                StreamWriter sw = new StreamWriter(duplexdir + request_filename);
                for (int i = 0; i < request_samples.Count; i++)
                {
                    sw.WriteLine($"{request_samples[i].barcode}\t{request_samples[i].rackindex}\t{request_samples[i].index}");
                }
                sw.Close();
            }
        }
        //假双工写测试文本
        public void WriteTestItem()
        {
            if (duplexdir == null || duplexdir == "" || !Directory.Exists(duplexdir)) return;
            if (testitem_sample.Count != 0)
            {
                string reqfilename = duplexdir + request_filename;
                if (System.IO.File.Exists(reqfilename))
                {
                    try { File.Delete(reqfilename); }
                    catch
                    {
                        MessageBox.Show("删除失败");
                    }
                }
                StreamWriter sw = new StreamWriter(duplexdir + testitem_filename);
                for (int i = 0; i < testitem_sample.Count; i++)
                {
                    sw.WriteLine($"{request_samples[i].barcode}\t{testitem_sample[i].testcode}\t{testitem_sample[i].liscode}");
                    break;
                }
                sw.Close();
            }
        }
        //得到工作样本
        public SKABO.Common.Models.NotDuplex.SampleInfo GetWorkSample(string barcode)
        {
            lock (mylock)
            {
                for (int i = 0; i < work_sample.Count; i++)
                {
                    if (work_sample[i].Barcode == barcode)
                        return work_sample[i];
                }
            }
            return null;
        }
        //得到工作样本列表
        public List<SKABO.Common.Models.NotDuplex.SampleInfo> GetWorkSampleList()
        {
            lock (mylock)
            {
                return work_sample;
            }
        }
        //清空
        public void ClsWorkSample()
        {
            lock (mylock)
            {
                work_sample.Clear();
            }
        }
        //试验侦测工作线程
        public void WorkSetp()
        {
            if (duplexdir == null || duplexdir == "" || !Directory.Exists(duplexdir)) return;
            string path = duplexdir;
            string filename = duplexdir + testitem_filename;
            if (System.IO.File.Exists(filename))
            {
                //读取item文件通过成试验
                Thread.Sleep(100);

                StreamReader sr = new StreamReader(filename, Encoding.GetEncoding("GB2312"));
                string filedata = sr.ReadLine();
                while (filedata != null)
                {
                    string[] datas = filedata.Split('\t');
                    var samplereq = GetReqSample(datas[0]);
                    if (samplereq != null)
                    {
                        var sampletem = GetWorkSample(samplereq.barcode);
                        if (sampletem == null)
                        {
                            sampletem = new SKABO.Common.Models.NotDuplex.SampleInfo();
                            sampletem.Barcode = samplereq.barcode;
                            sampletem.Index = samplereq.index;
                            sampletem.RackIndex = samplereq.rackindex;
                            work_sample.Add(sampletem);
                        }
                        sampletem.SetTestItem(byte.Parse(datas[1]));
                    }

                    filedata = sr.ReadLine();
                }
                sr.Close();
                //删除文件
                try { File.Delete(filename); }
                catch
                {
                    MessageBox.Show("删除失败");
                }
            }
        }
        //结果文件生成
        public void WriteResul(List<T_Result> result_list)
        {
            if (resultdir == null|| resultdir == "" || !Directory.Exists(resultdir)) return;
            string filename = resultdir + GetTimeYMDHMSStr() + ".txt";
            StreamWriter sw = new StreamWriter(filename);
            for(int i=0;i< result_list.Count;i++)
            {
                var result = result_list[i];
                if (result.Gel == null)
                result.Gel = ResManager.getInstance().GetGelTestByMask(result.GelBarcode);
                string picfilename = GetPicFileName(result.Picture.RawFile);
                string reactionstr = "";
                string[] reactionword = { "?", "阳性4+", "阳性3+", "阳性2+", "阳性1+", "弱阳±", "阴性-", "溶血H", "溶血PH", "混合DCP","双群" };
                int[] reactionmap = { 0, 9, 8, 7, 6, 5, 4, 3, 2, 1,10};
                int[] reaction = { result.Picture.T1, result.Picture.T2, result.Picture.T3, result.Picture.T4, result.Picture.T5, result.Picture.T6, result.Picture.T7, result.Picture.T8 };
                for (int j = 0; j < reaction.Length; j++)
                {
                    reaction[j] += 5;
                    int index = reactionmap[reaction[j]];
                    string strtem = $"{j + 1}/{reactionword[index]}/{index}";
                    if (j != 0) strtem = "|"+strtem;
                    reactionstr += strtem;
                }
                string listcode = null;
                if(result.Gel!=null)listcode = result.Gel.LisGelClass.ToString();
                sw.WriteLine($"{result.ID}\t{listcode}\t{result.GelName}\t{result.SmpBarcode}\t{result.DonorBarcode}\t{result.Result}\t{result.Remark}\t{result.ReportUser}\t{GetTestTimeStr(result.StartTime)}\t{picfilename}\t{result.TubeStartNo}\t{result.TubeCount}\t{result.GelBarcode}\t{reactionstr}\t{result.LED}");
            }
            sw.Close();
        }
    }
}
