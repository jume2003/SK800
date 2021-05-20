using Ihardware.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Hardware.Step
{
    /// <summary>
    /// 加载GEL卡
    /// </summary>
    public class LoadGelStep : IStep
    {
        public LoadGelStep(String StepName)
        {
            this.Name = StepName;
        }
        private readonly String Name;
        public string StepName => Name;

        public bool Execute(object paramObj)
        {
            throw new NotImplementedException();
        }
    }
}
