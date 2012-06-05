using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.QueryCache
{
    interface ICacheKey
    {
        string Query { get; }
        string TableName { get; }
    }
}
