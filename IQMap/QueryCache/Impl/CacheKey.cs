using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;

namespace IQMap.QueryCache.Impl
{
    class CacheKey : ICacheKey
    {
        public CacheKey(ISqlQuery query)
        {
            // The key must always be made from a clone - otherwise, if the original query is changed & rerun, it will still
            // match the cached value (being the same object)
            Query = SqlQueryUtility.QueryAsSql(query.GetQuery(), query.Parameters);
            ISqlQueryMaker comp = query as ISqlQueryMaker;
            if (comp != null)
            {
                TableName = comp.TableName;
            }
        }

        public string TableName { get; protected set; }
        public string Query { get; protected set; }
        public override int GetHashCode()
        {
            return Query.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            CacheKey key = obj as CacheKey;
            return key != null &&
                key.Query.Equals(Query);
        }
        public override string ToString()
        {
            return Query.ToString();
        }
    }
}
