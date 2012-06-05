using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.SqlQueryBuilder
{
    public interface IOrderBy: ISqlClause
    {

        new IOrderBy Clone();
        void Reverse();
    }
}
