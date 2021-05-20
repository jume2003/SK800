using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Ihardware.Core
{
    public interface ICommunicater
    {
        void SetCoilOn(String addr);
        void SetCoilOn(params byte[] addr);
        void SetCoilOff(String addr);
        void SetCoilOff(params byte[] addr);
        void SetCoil(bool on, params byte[] addr);
        void SetBatchCoil(String startAddr, short len, params byte[] bitValsk);
        void SetBatchCoil(byte[] startAddr, short len, params byte[] bitValsk);
    }
}
