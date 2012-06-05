using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.Impl
{
    public class IQException: Exception
    {
        public IQException(string message)
            : base(message)
        {

        }
    }
}
