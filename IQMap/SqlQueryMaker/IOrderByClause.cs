using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.SqlQueryBuilder
{
    public interface IOrderByClause: IEnumerable<IOrderByItem>,IOrderBy 
    {
        IOrderByClause Add(IOrderBy item);
        IOrderByClause Add(string orderClause);
        IOrderByClause Add(string field, SortOrder order);
        IOrderByClause AddAlways(IOrderBy item);
        IOrderByClause AddAlways(string orderClause);
        IOrderByClause AddAlways(string field, SortOrder order);

        IOrderByClause AddIfNotPresent(IOrderBy item);
        IOrderByClause AddIfNotPresent(string orderClause);
        IOrderByClause AddIfNotPresent(string field, SortOrder order);

        IOrderByClause Set(string orderClause);

        void Touch();
        new IOrderByClause Clone();
    }
}
