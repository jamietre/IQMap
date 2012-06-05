using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap
{
    public enum QueryType
    {
        Select = 1,
        Insert = 2,
        Update = 3,
        Delete = 4,
        Where = 5,
        Invalid = 6
    }
}
