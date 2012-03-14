using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.Implementation
{
    public class IQException: Exception
    {
        public IQException(string message)
            : base(message)
        {

        }
    }
}
