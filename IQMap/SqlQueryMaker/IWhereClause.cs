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
    public interface IWhereClause : IWhere
    {
        JoinType JoinType { get; set; }
        int Count { get; }

        IWhereClause Set(string clause);
        IWhereClause Add(IWhere condition);
        IWhereClause Add(IWhere condition, JoinType joinType);
        IWhereClause Add(IEnumerable<IWhere> criteria);
        IWhereClause Add(string field, object value, bool parameterize = true);
        IWhereClause Add(string condition, params IDataParameter[] parameters);
        IWhereClause Add(string condition, JoinType joinType, params IDataParameter[] parameter);
        IWhereClause Add(string field, object value, ComparisonType comparisonType, bool parameterize = true);
        IWhereClause Add(string field, object value, ComparisonType comparisonType, JoinType joinType, bool parameterize = true);
       
        void Touch();
        IEnumerable<IWhere> Items { get; }
        new IWhereClause Clone();
        
    }
}
