using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using System.Data;
using System.Reflection;
using System.Dynamic;
using IQMap.Impl;
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;
using IQMap.Impl.Support;
using IQObjectMapper;

namespace IQMap
{
    public static class IQ
    {
        static IQ()
        {
            // this may not work perfectly because we have no way to be sure this constructor happens before the cache is otherwise
            // accessed. But worst case, the cache is just re-created once. The fix is to convert the ObjectMapper singleton to an instance
            // class.

            ObjectMapper.MapperCache = new MapperCache();

        }
        #region private properties

        private static IDbConnection _Connection;
        public static IDbConnection Connection
        {
            get
            {
                if (_Connection == null)
                {
                    return Config.GetConnection();
                } else {
                   return _Connection;
                }
            }
            set
            {
                _Connection = value;
            }
        }
        
      
        private static Config _Config;
        public static Config Config
        {
            get
            {
                if (_Config == null)
                {
                    _Config = new Config();
                }
                return _Config;
            }

        }

        #endregion

        #region public methods

        public static IDbContext GetDbContext(params object[] options)
        {
            return GetDbContextFromOptions(options);
        }

        public static IQueryBuilder<T> From<T>(params object[] options) where T: class
        {
            OptionParser parser;
            var query = GetQueryFromOptions<T>(out parser, options);
            if (parser.NonOptionParametersOrNull != null)
            {
                throw new ArgumentException("Unexpected parameters were passed to From. Only <string,object[]> for a query+parameters, and options are permitted.");
            }

            return query;
        }

        public static IQueryBuilder<T> From<T>(string where, params object[] options) where T : class
        {
            OptionParser parser;
            var query = GetQueryFromOptions<T>(out parser, options)
                .Where(where,parser.NonOptionParameters);

            return query;
        }
        public static IQueryBuilder<T> From<T>(int pkValue, params object[] options) where T : class
        {
            OptionParser parser;
            var query = GetQueryFromOptions<T>(out parser, options)
                .Where(pkValue)
                .Options(parser.QueryOptions);
                
            return query;
        }
        public static bool Save(object obj, params object[] options)
        {
            OptionParser parser;
            var context = GetDbContextFromOptions(out parser,options);
            return context.Save(obj,parser.QueryOptions);
        }
        public static bool Delete(object obj, params object[] options)
        {
            OptionParser parser;
            var context = GetDbContextFromOptions(out parser, options);
            if (parser.NonOptionParameters.Length > 0)
            {
                throw new ArgumentException("Unknown options were passed to Delete.");
            }
            return context.Delete(obj, parser.QueryOptions);
        }
        public static int Delete<T>(string where, params object[] parameters) where T : class
        {
            OptionParser parser;
            var query = GetQueryFromOptions<T>(out parser, parameters)
                .Where(where, parser.NonOptionParameters)
                .Options(parser.QueryOptions);
                
            return query.Delete();
        }

        /// <summary>
        /// Populate the instance obj from the record matching primaryKeyValue
        /// </summary>
        /// <param name="primaryKeyValue"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static object Load(object obj, int primaryKeyValue,  params object[] parameters) 
        {
            OptionParser parser;
            return GetDbContextFromOptions(out parser, parameters)
                .FromPrimaryKey(obj).Single();
        }
        public static IQuery<dynamic> From(string query, params object[] parameters)
        {
            OptionParser parser;
            return GetDbContextFromOptions(out parser, parameters)
                .From(query, parser.NonOptionParameters);
        }
        public static T Load<T>(string where, params object[] parameters) where T : class
        {
            return IQ.From<T>(where, parameters).Single();
        }

        public static T Load<T>(int primaryKeyValue, params object[] options) where T : class
        {
            return IQ.From<T>(primaryKeyValue,options).Single();
        }
       
        public static IDataReader RunSql(string query, params object[] parameters)
        {
            OptionParser parser;
            return GetDbContextFromOptions(out parser, parameters)
                .RunSql(query, parser.NonOptionParameters);
        }

        public static IEnumerable<T> Query<T>(string query, params object[] parameters)
        {
            OptionParser parser;
            return GetDbContextFromOptions(out parser, parameters)
                .RunSql(query, parser.NonOptionParameters)
                .MapAll<T>();

  
        }
        public static IEnumerable<dynamic> Query(string where, params object[] parameters)
        {
            OptionParser parser;
            var query = GetDbContextFromOptions(out parser, parameters)
                .RunSql(where, parser.NonOptionParameters)
                .MapAll<IDynamicMetaObjectProvider>();

            return query;
        }

        public static int QueryScalar(string query, params object[] parameters)
        {
            OptionParser parser;
            return GetDbContextFromOptions(out parser, parameters)
                .QueryScalar(query, parser.NonOptionParameters);
        }

        public static IEnumerable<T> RunStoredProcedure<T>(string spName, params object[] parameters)
        {
            return RunStoredProcedure<T>(spName,parameters);
        }
        public static ISqlQuery GetQuery<T>(object query, params object[] parameters)
        {
            var wrapper = new SqlQueryParser<T>(QueryType.Select, query, parameters);
            return wrapper.QueryFull;
        }

        /// <summary>
        /// Start a new transaction and isolate the DataController from the default instance.
        /// </summary>
        /// <returns></returns>
        public static IDbContext BeginTransaction()
        {
            return GetDbContext().BeginTransaction();
        }

        /// <summary>
        /// Clears all cached class metadata
        /// </summary>
        public static void ClearCache()
        {
            MapperCache.ClearCache();

        }


        #endregion

        #region Utility methods

        /// <summary>
        /// Track changes to an object
        /// </summary>
        /// <param name="obj"></param>
        public static void Track(object obj)
        {
            IObjectData data = MapperCache.GetOrCreateObjectData(obj);
        }
        /// <summary>
        /// Zeroes out the primary key, disconnecting an item from the database
        /// </summary>
        /// <param name="obj"></param>
        public static void Disconnect(object obj)
        {
            MapperCache.GetClassInfo(obj.GetType()).PrimaryKeyField.SetValue(obj, 0);
        }
        public static void IsNew(object obj)
        {
            IQMap.Impl.ClassInfo.IsNew(obj);

        }

        #endregion

        #region metadata accessors

        public static IClassInfo ClassInfo<T>()
        {
            return ClassInfo(typeof(T));
        }
        public static IClassInfo ClassInfo(Type type)
        {
            return MapperCache.GetClassInfo(type,Config);
        }
        public static IQueryOptions GetQueryOptions(string tableName = null, string primaryKey = null)
        {
            return new QueryOptions
            {
                TableName = tableName,
                PrimaryKey = primaryKey 
            };
        }
        public static MapperCache MapperCache
        {
            get
            {
                return (MapperCache)ObjectMapper.MapperCache;
            }
        }

        #endregion
        
        #region private methods
        /// <summary>
        /// Return basic DB context info from option list, or default if not available
        /// </summary>
        internal static IDbContext GetDbContextFromOptions(params object[] options)
        {
           OptionParser  remaining;
           var context= GetDbContextFromOptions(out remaining, options);
           if (remaining.QueryOptions != null || remaining.NonOptionParametersOrNull != null)
           {
               throw new ArgumentException("There were non-applicable options passed when creating a DbContext.");
           }
           return context;
        }
        internal static DbContext GetDbContextFromOptions(out OptionParser parser, params object[] options)
        {

            parser = new OptionParser(options);
            var context = new DbContext(parser.DataStorageController, parser.Connection, parser.Transaction, parser.CommandOptions,
                parser.Buffering, parser.Reconnect);
            return context;
        }

        internal static QueryBuilder<T> GetQueryFromOptions<T>(out OptionParser parser, params object[] options) where T: class
        {
            var context = GetDbContextFromOptions(out parser, options);
            parser = new OptionParser(options);

            var query = new QueryBuilder<T>(context, parser.QueryOptions);
            return query;
        }
        internal static QueryBuilder<object> GetQueryFromOptions(object source, out OptionParser parser, params object[] options)
        {
            var context = GetDbContextFromOptions(out parser, options);
            parser = new OptionParser(options);

            var query = new QueryBuilder<object>(context,source,parser.QueryOptions);
            return query;
        }


        internal static ISqlQuery CreateQuery()
        {
            return CreateQuery(QueryType.Select);
        }
        internal static SqlQueryMaker CreateQuery(QueryType queryType)
        {
            var query = new SqlQueryMaker(queryType);
            return query;
        }

        #endregion

        
    }
}