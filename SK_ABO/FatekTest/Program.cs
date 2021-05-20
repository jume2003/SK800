using SKABO.Common.Models.Communication;
using SKABO.Hardware.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FatekTest
{
    class Program
    {
        
        private bool Connected;
        private Socket newclient;
        private bool backed;
        private bool stop;
        static void Main(string[] args)
        {
            Console.WriteLine(Convert.ToString(0x06, 2));
            Console.ReadLine();
            if (true) return;
            Stopwatch sw = new Stopwatch();
            long sum3 = 0;
            //int i = 0;
            sw.Start();
            for (int i=0;i< 1000;i++)
            {
                sum3 = sum3 + i;
            };
            sw.Stop();
            Console.WriteLine($"time={sw.ElapsedMilliseconds}");
            Console.WriteLine($"sum3={sum3}");
            long sum = 0;
            //int i = 0;
            Parallel.For(0, 1000, (i) =>
             {
                 Thread.Sleep(new Random(5).Next(3));
                 sum = sum + i;
                 //Console.WriteLine($"sum={sum}  i={i}");
             });
            sw.Stop();
            Console.WriteLine($"time={sw.ElapsedMilliseconds}");
            Console.WriteLine($"sum={sum}");

            long sum1 = 0;
            
            System.Threading.SpinLock spinLock = new SpinLock();
            //int i = 0;
            sw.Restart();
            Parallel.For(0, 1000, (i) =>
            {
                bool lockFlag = false;
                spinLock.Enter(ref lockFlag);
                try
                {
                    Thread.Sleep(new Random(5).Next(3));
                    sum1 = sum1 + i;
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if (lockFlag)
                        spinLock.Exit();
                }
                
            });
            sw.Stop();
            Console.WriteLine($"time={sw.ElapsedMilliseconds}");
            Console.WriteLine($"sum1={sum1}");
            long sum2 = 0;
            
            //System.Threading.SpinLock spinLock1 = new SpinLock();
            //int i = 0;
            sw.Restart();
            Parallel.For(0, 1000, (i) =>
            {
                bool lockFlag1 = false;
                var spinLock1 = new SpinLock();
                spinLock1.Enter(ref lockFlag1);
                try
                {
                    Thread.Sleep(new Random(5).Next(3));
                    sum2 = sum2 + i;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    if(lockFlag1)
                    spinLock1.Exit();
                }

            });
            sw.Stop();
            Console.WriteLine($"time={sw.ElapsedMilliseconds}");
            Console.WriteLine($"sum2={sum2}");
            SpinLockSample1();
            Console.ReadLine();
            if (true)
                return;
            SemaphoreSample ss = new SemaphoreSample();
            //ss.Main(null);
            MonitorSample obj = new MonitorSample();
            Thread tProduce = new Thread(new ThreadStart(obj.Produce));
            Thread tConsume = new Thread(new ThreadStart(obj.Consume));
            tProduce.Start();
            tConsume.Start();
            Console.ReadLine();
            
            
            if (true)
                return;
            
            
        }
        static void SpinLockSample1()
        {
            SpinLock sl = new SpinLock();

            StringBuilder sb = new StringBuilder();

            // Action taken by each parallel job.
            // Append to the StringBuilder 10000 times, protecting
            // access to sb with a SpinLock.
            Action action = () =>
            {
                bool gotLock = false;
                for (int i = 0; i < 10000; i++)
                {
                    gotLock = false;
                    try
                    {
                        sl.Enter(ref gotLock);
                        sb.Append((i % 10).ToString());
                    }
                    finally
                    {
                        // Only give up the lock if you actually acquired it
                        if (gotLock) sl.Exit();
                    }
                }
            };

            // Invoke 3 concurrent instances of the action above
            Parallel.Invoke(action, action, action);

            // Check/Show the results
            Console.WriteLine("sb.Length = {0} (should be 30000)", sb.Length);
            Console.WriteLine("number of occurrences of '5' in sb: {0} (should be 3000)",
                sb.ToString().Where(c => (c == '5')).Count());
        }

        private void send()
        {
            this.Start(false);
            if (true) return;
            this.setVal();
            this.Start(true);
            backed = true;
            int index = 0;
            while (!stop)
            {
                index++;
                if(backed)
                    AreYouOK(index);
                Thread.Sleep(5);
            }
            if (backed)
                this.Start(false);
            //readOutput();
            //readInput();
            //setOutPut(5);

        }
        private void setOutPut(byte index)
        {
            Console.WriteLine("设置output 05");
                                   //00 - 00 -  00 -   00 -  00 -   06 - 01 -   05 - 00 -  06 -    FF - 00
            var data = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x00, index, 0xFF, 0x00 };
            newclient.Send(data);
        }
        private void readOutput()
        {
            Console.WriteLine("读取output 01");
                                      //00     01    00    00    00    06    01    01    03   E8    00     07
            byte[] data = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x01, 0x00, 0X00, 0x00, 0x07 };

            //data = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x00, 0x02, 0xFF, 0x00 };
            newclient.Send(data);
        }
        private void readInput()
        {
            Console.WriteLine("读取input 01");
                                      //00     01    00    00    00    06    01    01    03   E8    00     07
            byte[] data = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x01, 0x03, 0XE8, 0x00, 0x07 };

            //data = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x00, 0x02, 0xFF, 0x00 };
            newclient.Send(data);
        }
        //读D 00 01 00 00 00 06 01 03 17 70 00 07

            private void Start(bool isStart)
        {
            Console.WriteLine("启动 01");
            byte[] data = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x07, 0XD0, (byte)(isStart?0xFF:0x00), 0x00 };

            //data = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x00, 0x02, 0xFF, 0x00 };
            newclient.Send(data);
        }
        private void setVal()
        {
            Console.WriteLine("启动 01");//1388
            byte[] data = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x06, 0x17, 0X72, 0x13, 0x10 };

            //data = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x00, 0x02, 0xFF, 0x00 };
            newclient.Send(data);
        }

        public void Connect()
        {
            byte[] data = new byte[1024];

            string ipadd = "192.168.1.53";//将服务器IP地址存放在字符串 ipadd中  
            int port = Convert.ToInt32("502");//将端口号强制为32位整型，存放在port中  

            //创建一个套接字   

            IPEndPoint ie = new IPEndPoint(IPAddress.Parse(ipadd), port);
            newclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            //将套接字与远程服务器地址相连  
            try
            {
                newclient.Connect(ie);
                Connected = true;

            }
            catch (SocketException e)
            {
                Console.Out.WriteLine("连接服务器失败  " + e.Message);
                return;
            }

            ThreadStart myThreaddelegate = new ThreadStart(ReceiveMsg);
            var myThread = new Thread(myThreaddelegate);
            myThread.Start();

        }
        private void AreYouOK(int index)
        {
             Console.WriteLine("询问 "+index);
            byte[] data = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x01, 0x07, 0XD1, 0x00, 0x01 };

            //data = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x01, 0x05, 0x00, 0x02, 0xFF, 0x00 };
            newclient.Send(data);
        }

        public void ReceiveMsg()
        {
            backed = false;
            while (true)
            {
                byte[] data = new byte[1024];//定义数据接收数组  
                newclient.Receive(data);//接收数据到data数组  
                backed = true;
                int length = data[5];//读取数据长度  
                Console.WriteLine(String.Format("长度={0}", length));
                Byte[] datashow = new byte[length + 6];//定义所要显示的接收的数据的长度  
                for (int i = 0; i <= length + 5; i++)//将要显示的数据存放到数组datashow中  
                    datashow[i] = data[i];
                string stringdata = BitConverter.ToString(datashow);//把数组转换成16进制字符串  
                showMsg(stringdata + "\r\n");

                
                if (data[7] == 0x01) {
                    if (data[9] == 0x01)
                    {
                        Console.WriteLine("到位");
                        this.stop = true;
                    }
                };
                /*
                if (data[7] == 0x02) { showMsg(stringdata + "\r\n"); };
                if (data[7] == 0x03) { showMsg(stringdata + "\r\n"); };
                if (data[7] == 0x05) { showMsg(stringdata + "\r\n"); };
                if (data[7] == 0x06) { showMsg(stringdata + "\r\n"); };
                if (data[7] == 0x0F) { showMsg(stringdata + "\r\n"); };
                if (data[7] == 0x10) { showMsg(stringdata + "\r\n"); };
                */
            }
        }
        public void showMsg(string msg)
        {

            Console.Out.Write(msg);

        }
    }
}
