using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.SqlQueryBuilder
{
    public enum SqlDataFormat
    {
        String = 1,
        Numeric = 2,
        DateTime = 3,
        Null = 4,
        Parameter = 5,
        Binary = 6,
        Unsupported = 7
    }
}
