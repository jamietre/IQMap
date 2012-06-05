using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Data;
using System.Dynamic;
using IQMap.Impl;

namespace IQMap
{
    public static class IDbConnectionExtensions
    {
        public static IDbContext GetDbContext(this IDbConnection connection)
        {
            return IQ.GetDbContext(connection);
        }
        public static IQuery<T> From<T>(this IDbConnection connection, params object[] options) where T : class
        {
            return IQ.From<T>(connection);
        }
        public static IQuery<T> From<T>(this IDbConnection connection, string where, params object[] options) where T : class
        {
            return IQ.From<T>(where, options, connection);
        }

        public static bool Save(this IDbConnection connection,object obj, params object[] options)
        {
            return IQ.Save(obj, options, connection);
        }

        public static IDataReader RunSql(this IDbConnection connection, string query, params object[] parameters)
        {
            return IQ.GetDbContext(connection).RunSql(query, parameters);
        }
        public static IEnumerable<T> Query<T>(string where, params object[] parameters)
        {
            return IQ.Query<T>(where, parameters);
        }
        public static IEnumerable<dynamic> Query(string query, params object[] parameters)
        {
            return IQ.Query(query, parameters);
        }
        public static int QueryScalar(this IDbConnection connection, string query, params object[] parameters)
        {
            return IQ.QueryScalar(query, parameters, connection);
        }

    }
}
