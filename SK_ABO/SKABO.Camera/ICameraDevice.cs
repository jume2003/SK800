using SKABO.Camera.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Camera
{
    public class BitmapGotEventArgs
    {
        public BitmapGotEventArgs(Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }
        public Bitmap bitmap { get; set; }
    }
     public delegate void BitGotHandle(object sender, BitmapGotEventArgs e);
    public interface ICameraDevice
    {
        /// <summary>
        /// 打开设备
        /// </summary>
        /// <returns></returns>
        bool Open();
        /// <summary>
        /// 关闭设备
        /// </summary>
        /// <returns></returns>
        bool Close();
        /// <summary>
        /// 采集
        /// </summary>
        void Play();
        /// <summary>
        /// 停止采集
        /// </summary>
        void Stop();
        /// <summary>
        /// 获取图像
        /// </summary>
        /// <returns></returns>
        Bitmap GetBitmap();
        /// <summary>
        /// 连续采集的回调事件
        /// </summary>
        event BitGotHandle BitmapGot;
        /// <summary>
        /// 设置曝光时间
        /// </summary>
        /// <param name="time"></param>
        void SetExposureTime(double time);
        /// <summary>
        /// 获取曝光时间
        /// </summary>
        /// <returns></returns>
        double GetExposureTime();
        /// <summary>
        /// 最小曝光时间
        /// </summary>
        /// <returns></returns>
        double GetMinExposureTime();
        /// <summary>
        /// 最大曝光时间
        /// </summary>
        /// <returns></returns>
        double GetMaxExposureTime();
        /// <summary>
        /// 设置增益
        /// </summary>
        /// <param name="gain"></param>
        void SetGain(double gain);
        /// <summary>
        /// 获取当前增益
        /// </summary>
        /// <returns></returns>
        double GetGain();
        /// <summary>
        /// 最小增益
        /// </summary>
        /// <returns></returns>
        double GetMinGain();
        /// <summary>
        /// 最大增益
        /// </summary>
        /// <returns></returns>
        double GetMaxGain();
        void SetBalanceWhiteAuto(BalanceWhiteAutoStatusEnum balanceWhiteAutoStatusEnum);

        void SetBalanceRatio(BalanceWhiteChanelEnum balanceWhiteChanelEnum, double val);

        double GetBalanceRatio(BalanceWhiteChanelEnum balanceWhiteChanelEnum);

        double GetMinBalanceRatio(BalanceWhiteChanelEnum balanceWhiteChanelEnum);
        double GetMaxBalanceRatio(BalanceWhiteChanelEnum balanceWhiteChanelEnum);
        String[] SupportResolution();
    }
}
