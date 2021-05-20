using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FatekTest
{
    class SemaphoreSample
    {
         Semaphore sema = new Semaphore(5, 5);
        const int cycleNum = 9;
        public void Main(string[] args)
        {
            for (int i = 0; i < cycleNum; i++)
            {
                Thread td = new Thread(new ParameterizedThreadStart(testFun));
                td.Name = string.Format("编号{0}", i.ToString());
                td.Start(td.Name);
            }
        }
        public  void testFun(object obj)
        {
            sema.WaitOne();
            Console.WriteLine(obj.ToString() + "进洗手间：" + DateTime.Now.ToString());
            Thread.Sleep(2000);
            Console.WriteLine(obj.ToString() + "出洗手间：" + DateTime.Now.ToString());
            sema.Release();
        }
    }
}
