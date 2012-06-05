using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.SqlQueryBuilder
{
    public enum JoinType
    {
        And = 1,
        Or = 2
    }
    public interface IWhere: ISqlClause
    {
        ICollection<IDataParameter> Parameters { get; }
        /// <summary>
        /// This method lets a client know when the clause must be parenthesized. In other words,
        /// if this returns false, it is associative with the same join type, and does not need
        /// to be wrapped in parenthesis. If true, it is not associative.
        /// </summary>
        /// <param name="joinType"></param>
        /// <returns></returns>
        bool MustParenthesizeFor(JoinType joinType);
        new IWhere Clone();

    }
    
}
