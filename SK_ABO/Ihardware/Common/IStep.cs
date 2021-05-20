using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ihardware.Common
{
    public interface IStep
    {
        String StepName { get; }
        bool Execute(Object paramObj);
    }
}
