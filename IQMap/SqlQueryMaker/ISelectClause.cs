using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.SqlQueryBuilder
{
    public interface ISelectClause: IEnumerable<ISelectItem>, ISelect
    {
        void Add(ISelectItem item);
        void Add(string clause);
        void Add(string field, string alias=null);
    }
}
