using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap.SqlQueryBuilder.Impl;

namespace IQMap.SqlQueryBuilder
{
    public interface ISqlClause
    {

        bool IsEmpty {get;}
        ISqlQueryMaker Owner { get; set; }
        string GetSql();

        void Clear();
        //ISqlClause Set(string clause);
        ISqlClause Clone();
    }
}
