using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap
{
    /// <summary>
    /// Defines a compound clause
    /// </summary>
    public interface IWhereClause : IWhere
    {
        JoinType JoinType { get; set; }
        int Count { get; }
        IWhereClause Add(IWhere criterion);
        IWhereClause Add(params IWhere[] criteria);
        IWhereClause Add(string field, object value);
        IWhereClause Add(string field, object value, ComparisonType comparisonType);
    }
}
