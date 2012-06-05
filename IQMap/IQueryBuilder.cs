using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap.SqlQueryBuilder;

namespace IQMap
{
    public interface IQueryBuilder: IQuery
    {
        IClassInfo ClassInfo { get; }

        /// <summary>
        /// Returns the fields and properties that are members of T
        /// </summary>
        /// 
        IEnumerable<string> Properties { get; }

        /// <summary>
        /// Returns the columns which will be selected for this query. This will match the "select" clause.
        /// An exception will be thrown if "*" is the select clause.
        /// </summary>
        IEnumerable<string> Columns { get; }

        IQueryBuilder Where(string where, params object[] parameters);
        IQueryBuilder Where(int pkValue);

        IQueryBuilder Select(string select, params object[] parameters);
        IQueryBuilder OrderBy(string order);
        IQueryBuilder OrderByDescending(string order);
        IQueryBuilder ThenBy(string order);
        IQueryBuilder ThenByDescending(string order);
        IQueryBuilder GroupBy(string group);
        IQueryBuilder Having(string group);

        IQueryBuilder Reverse();
        IQueryBuilder Skip(int rows);
        IQueryBuilder Take(int rows);

        IQueryBuilder Options(string tableName = null, string primaryKey = null);
        IQueryBuilder Options(IQueryOptions options);

        bool Any(string where, params object[] parameters);
        bool One(string where, params object[] parameters);
        bool Contains(object item);

        int Count(string where, params object[] parameters);
        int Delete();
        int Delete(string where, params object[] parameters);

        object Single(string where, params object[] parameters);
        object SingleOrDefault(string where, params object[] parameters);
        object SingleOrNew(string where, params object[] parameters);

        object First(string where, params object[] parameters);
        object FirstOrDefault(string where, params object[] parameters);
        object FirstOrNew(string where, params object[] parameters);

        object Last(string where, params object[] parameters);
        object LastOrDefault(string where, params object[] parameters);
        object LastOrNew(string where, params object[] parameters);

        IQueryBuilder Clone();
        new SqlQueryBuilder.ISqlQueryMaker QueryDef { get; }

    }
}
