using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.SqlQueryBuilder
{

    public interface IWhereItem : IWhere
    {
        string Field { get; set; }
        ComparisonType ComparisonType { get; set; }
        object Value { get; set; }
        bool Parameterize { get; set; }
        new IWhereItem Clone();
    }
}
