using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.Impl
{
    public class QueryOptions: IQueryOptions
    {
        public string TableName { get; set; }
        public string PrimaryKey { get; set; }
    }
}
