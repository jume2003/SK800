using SKABO.Common;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK_ABO.Views
{
    class PauseViewModel:Screen
    {
        public void DoContinue()
        {
            Constants.UserAbort = false;
            Constants.PauseResetEvent.Set();
            this.RequestClose(true);
        }
        public void Cancel()
        {
            Constants.UserAbort = true;
            Constants.PauseResetEvent.Set();
            this.RequestClose(false);
        }
    }
}
