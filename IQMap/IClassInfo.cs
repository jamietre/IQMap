using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap.Impl;
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;

namespace IQMap
{
    public interface IClassInfo: IQObjectMapper.IClassInfo
    {
        ISqlQueryMaker Query { get; set; }

        /// <summary>
        /// Return a clone of the query
        /// </summary>
        /// <returns></returns>
        ISqlQueryMaker GetQuery();
        ISqlQueryMaker GetQuery(QueryType query, IQueryOptions options = null);
        
        IDelegateInfo PrimaryKeyField { get; }
        object PrimaryKeyDefaultValue { get; }

        IDictionary<string,string> SqlNameMap { get; }

        bool DoEvent(object instance, IQEventType type, IDbContext query);
        /// <summary>
        /// When true, the class is bound to a database table
        /// </summary>
        bool IsBound { get; }
        bool Track { get; }
        


        // overridden members that return the local type

        bool TryGetValue(string fieldName, out IDelegateInfo info);
        new IList<IDelegateInfo> Fields { get; }
        //IEnumerable<KeyValuePair<string, string>> FieldNameMap { get; }
        new IDelegateInfo this[string fieldName] { get; }
        
    }
}
