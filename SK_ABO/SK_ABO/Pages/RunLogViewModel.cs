using SKABO.Common;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK_ABO.Pages
{
    public class RunLogViewModel : Screen
    {
        private static int LogCount;
        public static StringBuilder LogSB { get; set; } = new StringBuilder();

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
        }
    }
}
