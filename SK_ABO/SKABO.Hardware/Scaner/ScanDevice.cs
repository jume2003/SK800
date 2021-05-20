using SKABO.Common;
using SKABO.Common.Models.BJ;
using SKABO.Ihardware.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Hardware.Scaner
{
    public class ScanDevice
    {
        private  T_BJ_Scaner BJScaner(int purpose)
        {
            var   _BJScaner = Constants.BJDict[typeof(T_BJ_Scaner).Name].Where(item => (item as T_BJ_Scaner).Purpose == purpose).FirstOrDefault() as T_BJ_Scaner;
            return _BJScaner;
        }
        private T_BJ_Scaner _GelBJScaner;
        public T_BJ_Scaner GelBJScaner
        {
            get
            {
                _GelBJScaner = _GelBJScaner ??BJScaner(1);
                return _GelBJScaner;
            }
        }
        private T_BJ_Scaner _SampleBJScaner;
        public T_BJ_Scaner SampleBJScaner
        {
            get
            {
                _SampleBJScaner = _SampleBJScaner ?? BJScaner(0);
                return _SampleBJScaner;
            }
        }
        private  AbstractScaner _GelScaner;
        public  AbstractScaner GelScaner
        {
            get
            {
                if (_GelScaner == null && GelBJScaner!=null)
                {
                    _GelScaner = IoC.Get<AbstractScaner>(GelBJScaner.ScanerType);
                }
                return _GelScaner;
            }
        }
        private AbstractScaner _SampleScaner;
        public AbstractScaner SampleScaner
        {
            get
            {
                if (_SampleScaner == null && SampleBJScaner != null)
                {
                    _SampleScaner = IoC.Get<AbstractScaner>(SampleBJScaner.ScanerType);
                }
                return _SampleScaner;
            }
        }
        private static readonly Object scanerLock = new Object();
        public bool OpenGelScaner(bool IsWorkingModel = true)
        {
            return OpenScaner(1, IsWorkingModel);
        }
        /// <summary>
        /// 打开样本扫码器，会取消所有的事件
        /// </summary>
        /// <param name="IsWorkingModel"></param>
        /// <returns></returns>
        public bool OpenSampleScaner(bool IsWorkingModel = true)
        {
            return OpenScaner(0, IsWorkingModel);
        }
        internal bool OpenScaner(int purpose, bool IsWorkingModel = true)
        {
            var BJSc = purpose == 1 ? GelBJScaner : SampleBJScaner;
            var Sc = purpose == 1 ? GelScaner : SampleScaner;
            lock (scanerLock)
            {
                if (Sc.IsOpen)
                {
                    Sc.Stop();
                }
                Sc.CancelAllEvent();
                Sc.Open(BJSc.Port);
                return Sc.Start(IsWorkingModel);
            }
        }
    }
}
