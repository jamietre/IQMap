using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap.SqlQueryBuilder;

namespace IQMap
{
    public interface IQueryBuilder<T> : IQuery<T>, IQueryBuilder
    {
        new IQueryBuilder<T> Where(string where, params object[] parameters);
        new IQueryBuilder<T> Where(int pkValue);

        new IQueryBuilder<T> Select(string select, params object[] parameters);
        new IQueryBuilder<T> OrderBy(string order);
        new IQueryBuilder<T> OrderByDescending(string order);
        new IQueryBuilder<T> ThenBy(string order);
        new IQueryBuilder<T> ThenByDescending(string order);
        new IQueryBuilder<T> GroupBy(string group);
        new IQueryBuilder<T> Having(string group);

        /// <summary>
        /// Reverse the sort order
        /// </summary>
        /// <returns></returns>
        new IQueryBuilder<T> Reverse();
        new IQueryBuilder<T> Skip(int rows);
        new IQueryBuilder<T> Take(int rows);

        new IQueryBuilder<T> Options(string tableName = null, string primaryKey = null);
        new IQueryBuilder<T> Options(IQueryOptions options);

        new T Single(string where, params object[] parameters);
        new T SingleOrDefault(string where, params object[] parameters);
        new T SingleOrNew(string where, params object[] parameters);

        new T First(string where, params object[] parameters);
        new T FirstOrDefault(string where, params object[] parameters);
        new T FirstOrNew(string where, params object[] parameters);

        new T Last(string where, params object[] parameters);
        new T LastOrDefault(string where, params object[] parameters);
        new T LastOrNew(string where, params object[] parameters);

        new IQueryBuilder<T> Clone();
    }
}
