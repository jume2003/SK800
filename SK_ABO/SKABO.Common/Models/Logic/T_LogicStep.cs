using SKABO.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Logic
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class T_LogicStep
    {
        public int ID { get; set; }
        public String Name { get; set; }
        public LogicStepEnum StepEnum { get; set; }
        public String Parameters { get; set; }
        public int OrderIndex { get; set; }
        public int StepID { get; set; }
        public int ProgramID { get; set; }
        
    }
}
