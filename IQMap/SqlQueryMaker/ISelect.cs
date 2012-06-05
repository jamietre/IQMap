using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.SqlQueryBuilder
{
    public interface ISelect : ISqlClause
    {
        ICollection<IDataParameter> Parameters { get; }
        new ISelect Clone();

    }
}
