using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKABO.Common.Exceptions
{
    public class MyException: Exception
    {
        public MyException() :base()
        {

        }
        public MyException(String Message) : base(Message)
        {

        }
    }
}
