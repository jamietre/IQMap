using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap
{
    public interface IQueryOptions
    {
        string TableName { get; set; }
        string PrimaryKey { get; set; }
    }
}
