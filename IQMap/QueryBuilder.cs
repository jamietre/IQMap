using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.ObjectModel;
using IQMap.Impl;
using IQMap.Impl.Support;
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;

namespace IQMap
{
    public static class QueryBuilder
    {
        #region public methods

        public static ISqlQueryMaker Create()
        {
            return new SqlQueryMaker();
        }
        public static ISqlQueryMaker Create(QueryType type)
        {
            return new SqlQueryMaker(type);
        }
        /// <summary>
        /// Build a where clause from a string
        /// </summary>
        /// <param name="clause"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static IWhere WhereParse(string clause, params object[] parameters)
        {
            ParameterParser parser = new ParameterParser(clause, parameters);
            return new WhereString(clause, parser.Parameters.ToArray());
        }
        /// <summary>
        /// Build a where clause for a field matching a value
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IWhereItem WhereEquals(string field, object value)
        {
            return new WhereItem(field,value);
        }
        public static IWhereItem Where(string field, object value, ComparisonType compare)
        {
            return new WhereItem(field, value,compare);
        }
        public static IWhereClause WhereAny(params IWhere[] criteria)
        {
            return new WhereClause(JoinType.Or,criteria);
        }

        public static IOrderByClause OrderBy(string clause){
            return new OrderByClause(clause);
        }
        public static IOrderByItem OrderBy(string field, SortOrder order) {
            return new OrderByItem(field,order);
        }
        #endregion

        #region internal properties & methods

        static QueryBuilder()
        {
            EmptyParameterCollection = new ReadOnlyCollection<IDataParameter>(new List<IDataParameter>());
        }
        internal static ReadOnlyCollection<IDataParameter> EmptyParameterCollection
        {
            get;
            private set;
        }

        #endregion
    }

    /// <summary>
    /// Where Clause factory class
    /// </summary>
    //public class WhereFactory
    //{

    //    /// <summary>
    //    /// Build a where clause from a string
    //    /// </summary>
    //    /// <param name="clause"></param>
    //    /// <param name="parameters"></param>
    //    /// <returns></returns>
    //    public IWhere Parse(string clause, params object[] parameters)
    //    {
    //        ParameterParser parser = new ParameterParser(clause, parameters);
    //        return new WhereString(clause, parser.Parameters.ToArray());
    //    }
    //    /// <summary>
    //    /// Return a clause requiring field to equal value
    //    /// </summary>
    //    /// <param name="field"></param>
    //    /// <param name="value"></param>
    //    /// <returns></returns>
    //    public IWhereItem Equals(string field, object value)
    //    {
    //        return new WhereItem(field,value);
    //    }
    //    /// <summary>
    //    /// Return a compound OR clause from two or more clauses
    //    /// </summary>
    //    /// <param name="criteria"></param>
    //    /// <returns></returns>
    //    public IWhereClause Or(params IWhere[] criteria)
    //    {
    //        return new WhereClause(JoinType.Or,criteria);
    //    }
    //}
    /// <summary>
    /// Where Clause factory class
    /// </summary>
    //public class OrderByFactory
    //{

    //    IOrderByClause Add(string orderClause);
    //    IOrderByClause Add(string field, SortOrder order);
    //    IOrderByClause AddAlways(IOrderByItem item);
    //    IOrderByClause AddAlways(string orderClause);
    //    IOrderByClause AddAlways(string field, SortOrder order);
    //    /// <summary>
    //    /// Build a where clause from a string
    //    /// </summary>
    //    /// <param name="clause"></param>
    //    /// <param name="parameters"></param>
    //    /// <returns></returns>
    //    public IOrderByClause Parse(string clause)
    //    {
    //        return new OrderByClause(clause);
    //    }
    //    public IOrderByClause Parse(string clause)
    //    /// <summary>
    //    /// Return a clause requiring field to equal value
    //    /// </summary>
    //    /// <param name="field"></param>
    //    /// <param name="value"></param>
    //    /// <returns></returns>
    //    public IWhereItem Equals(string field, object value)
    //    {
    //        return new WhereItem(field, value);
    //    }
    //    /// <summary>
    //    /// Return a compound OR clause from two or more clauses
    //    /// </summary>
    //    /// <param name="criteria"></param>
    //    /// <returns></returns>
    //    public IWhereClause Or(params IWhere[] criteria)
    //    {
    //        return new WhereClause(JoinType.Or, criteria);
    //    }
    //}
}
