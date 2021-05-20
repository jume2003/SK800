using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Models.Logic
{
    public class T_LogicTest
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public IList<T_LogicStep> LogicSteps{ get;set;}
    }
}
