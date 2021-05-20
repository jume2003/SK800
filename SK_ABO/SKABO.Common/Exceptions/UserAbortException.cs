using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Exceptions
{
    public class UserAbortException:Exception
    {
        public UserAbortException() : base()
        {

        }
        public UserAbortException(String Message) : base(Message)
        {

        }
    }
}
