using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.SqlQueryBuilder
{
    /// <summary>
    /// Defines a compound clause
    /// </summary>
    public interface IUpdateClause : ISqlClause
    {
        int Count { get; }

        IWhereClause Add(IWhereItem condition);
        IWhereClause Add(string field, object value, bool parameterize = true);

        void Touch();
        IEnumerable<IWhere> Items { get; }
        new IWhereClause Clone();
        
    }
}
