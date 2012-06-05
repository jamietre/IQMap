using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Dynamic;


namespace IQMap.SqlQueryBuilder
{
    /// <summary>
    /// When the namespace SqlQueryBuilder is included, this adds extension methods to use the lower-level querybuilder methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Adds the where clause to the current conditions
        /// </summary>
        /// <param name="queryBuilder"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static IQueryBuilder Where(this IQueryBuilder queryBuilder, IWhere where)
        {
            var qb = queryBuilder.Clone();
            qb.QueryDef.Where.Add(where);
            return qb;
        }
        /// <summary>
        /// REplaces the current where clause with the passed clause
        /// </summary>
        /// <param name="queryBuilder"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static IQueryBuilder WhereAll(this IQueryBuilder queryBuilder, IWhere where)
        {
            var qb = queryBuilder.Clone();
            qb.QueryDef.Where.Clear();
            qb.QueryDef.Where.Add(where);
            return qb;
        }
        /// <summary>
        /// Replaces the current OrderBy clause
        /// </summary>
        /// <param name="queryBuilder"></param>
        /// <param name="or"></param>
        /// <returns></returns>
        public static IQueryBuilder OrderBy(this IQueryBuilder queryBuilder, IOrderBy orderBy)
        {
            var qb = queryBuilder.Clone();
            qb.QueryDef.OrderBy.Clear();
            qb.QueryDef.OrderBy.Add(orderBy);
            return qb;
        }
        /// <summary>
        /// Adds the order clause to the current orders
        /// </summary>
        /// <param name="queryBuilder"></param>
        /// <param name="or"></param>
        /// <returns></returns>
        public static IQueryBuilder ThenBy(this IQueryBuilder queryBuilder, IOrderBy orderBy)
        {
            var qb = queryBuilder.Clone();
            qb.QueryDef.OrderBy.Add(orderBy);
            return qb;
        }

    }
}