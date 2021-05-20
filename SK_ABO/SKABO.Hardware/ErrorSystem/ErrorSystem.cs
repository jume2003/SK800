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
using System.Windows.Forms;
using SKABO.ActionEngine;
using System.Runtime.InteropServices;

namespace SKABO.MAI.ErrorSystem
{
    public class ErrorSystem
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string IpClassName, string IpWindowName);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        /// <summary>
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        public const int WM_CLOSE = 0x10;
        public const int BM_CLICK = 0xF5;
        public static int wait_time = 0;
        public static string can_error_filename = "can_error.txt";
        public static string action_error_filename = "action_error.txt";
        public static string last_canerror = "";
        public static string last_acterror = "";
        public static Thread work_thread = null;
        public static string GetTimeYMDHMSStr()
        {
            System.DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;

            string timestr = string.Format("{0:0000}{1:00}{2:00}{3:00}{4:00}{5:00}", currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, currentTime.Second);
            return timestr;
        }
        public static void WriteCanError(string error)
        {
            if(error!= last_canerror)
            {
                last_canerror = error;
                FileStream fs = new FileStream(can_error_filename, FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(GetTimeYMDHMSStr() + error);
                sw.Close();
            }
        }
        public static bool WriteActError(string error,bool is_show=true,bool is_mulbutton=true,int waittime = 15)
        {
            if (error != last_acterror)
            {
                last_acterror = error;
                FileStream fs = new FileStream(action_error_filename, FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(GetTimeYMDHMSStr() + error);
                sw.Close();
            }
            if(is_show)
            {
                Task.Run(() =>
                {
                    Engine.getInstance().opDevice.LedRedBlink();
                });
                wait_time = waittime;
                Engine.getInstance().isskipdt = true;
                if(work_thread==null)
                {
                    work_thread = new Thread(() => {
                        while (true)
                        {
                            Thread.Sleep(1000);
                            var hwnd = FindWindow(null, "系统将于" + wait_time + "秒后重试");
                            wait_time--;
                            if (wait_time <= 0)
                            {
                                SendMessage(hwnd, WM_CLOSE, 0, 0);
                                wait_time = 0;
                                work_thread = null;
                                return;
                            }
                            SetWindowText(hwnd, "系统将于" + wait_time + "秒后重试");

                        }
                    });
                    work_thread.Start();
                }
                if (is_mulbutton)
                {
                    DialogResult dr;
                    dr = System.Windows.Forms.MessageBox.Show(error, "系统将于" + wait_time + "秒后重试", MessageBoxButtons.RetryCancel,
                             MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                    Task.Run(() =>
                    {
                        Engine.getInstance().opDevice.LedGreen();
                    });
                    return dr == DialogResult.Retry|| wait_time==0;
                }
                else
                {
                    DialogResult dr;
                    dr = System.Windows.Forms.MessageBox.Show(error, "系统提示!", MessageBoxButtons.OK,
                             MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                    Task.Run(() =>
                    {
                        Engine.getInstance().opDevice.LedGreen();
                    });
                   
                    return dr == DialogResult.Yes;
                }

            }
            return true;
        }
    }
}
