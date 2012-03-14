using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using System.Data;
using System.Reflection;
using IQMap.Implementation;

namespace IQMap
{
    public static class IQ
    {
        static IQ()
        {
            PopulateDbTypeMap();

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
        
        private static ConcurrentDictionary<object, IDBObjectData> ObjectMetadata =
            new ConcurrentDictionary<object, IDBObjectData>();
        private static Config _Config;
        public static Config Config
        {
            get
            {
                if (_Config == null)
                {
                    _Config = new Config();
                    _Config.GCTime += new EventHandler(_Config_GCTime);
                }
                return _Config;
            }

        }

        #endregion

        #region public methods

        private static void _Config_GCTime(object sender, EventArgs e)
        {
            foreach (KeyValuePair<object, IDBObjectData> kvps in ObjectMetadata)
            {
                if (kvps.Value.Orphaned)
                {
                    RemoveFromDict(kvps.Key);
                }
            }
        }

        public static bool Save(object obj, params object[] options)
        {
            return Config.DataController.Save(obj);
        }

        public static bool Save(object obj)
        {
     
            bool result = Config.DataController.Save( obj);
            return result;
        }


        /// <summary>
        /// Load a record expecting a single matching result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T Single<T>(object query, params object[] parameters)
        {
     
            return Config.DataController.Single<T>(query,parameters);
        }

        /// <summary>
        /// Load a record, returning either the single matching value or the default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T SingleOrDefault<T>(object query, params object[] parameters)
        {
            return Config.DataController.SingleOrDefault<T>(query, parameters);
        }

        /// <summary>
        /// Load a record expecting a single matching result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T First<T>(object query, params object[] parameters)
        {
            return Config.DataController.First<T>(query, parameters);
        }

        /// <summary>
        /// Load a record, returning either the single matching value or the default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(object query, params object[] parameters)
        {
            return Config.DataController.FirstOrDefault<T>(query, parameters);

        }
        /// <summary>
        /// Return the number of records matching the condition or query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static int Count<T>(object query, params object[] parameters)
        {
            return Config.DataController.Count<T>(query, parameters);
        }

        /// <summary>
        /// Try to load a single record for field/value combination. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="obj"></param>
        /// <returns>
        /// If missing, return false. If more than one match is found, throw an exception.
        /// </returns>
        public static bool TrySingle<T>(out T obj, object query, params object[] parameters)
        {
            return Config.DataController.TrySingle<T>(query, out obj, parameters);
        }
        /// <summary>
        /// Try to load a single record into an existing object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="obj"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static bool TrySingle<T>(T obj, object query, params object[] parameters)
        {
            return Config.DataController.TrySingle<T>(query, obj, parameters);
        }

        public static IEnumerable<T> Select<T>(object query, params object[] parameters)
        {
            return Config.DataController.Select<T>(query, parameters);
        }

     
        /// <summary>
        /// Delete record matching criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static int Delete<T>(object query, params object[] parameters)
        {
            return Config.DataController.Delete<T>( query, parameters);
        }

        /// <summary>
        /// Adds or updates a list of T objects 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="foreignKey"></param>
        /// <param name="fkValue"></param>
        /// <param name="data"></param>
        /// <param name="destructive"></param>
        /// <returns></returns>
        /// //
        /// 

        public static IDataReader Query(string query, params object[] parameters)
        {
            return Config.DataController.Query(query, parameters);
        }
        public static int QueryScalar(string query, params object[] parameters)
        {
            return Config.DataController.QueryScalar(query, parameters);
        }

        public static IEnumerable<T> RunStoredProcedure<T>(string spName, params object[] parameters)
        {
            return RunStoredProcedure<T>(spName,parameters);
        }
        public static ISqlQuery GetQuery<T>(object query, params object[] parameters)
        {
            var wrapper = new SqlQueryParser<T>(QueryType.Select, query, parameters);
            return wrapper.GetQuery();
        }
        
     

        #region Utility methods

        /// <summary>
        /// Copy the database bound properties of one object to another. If the target object is dirty, will throw
        /// an error. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void CopyTo<T>(T source, T destination)
        {
            CopyTo(source, destination);
        }
        /// <param name="destination"></param>
        public static void CopyTo(object source, object destination)
        {
            DBData(source).CopyTo(destination);
        }
        public static T Clone<T>(T source)
        {
            return (T)Clone(source);
        }
        public static object Clone(object source)
        {
            return DBData(source).Clone();
        }
        #endregion

        #region metadata accessors

        public static IDBClassInfo GetClassInfo<T>()
        {
            return GetClassInfo(typeof(T));
        }
        public static IDBClassInfo GetClassInfo(Type type)
        {
            return DBObjectData.GetClassInfo(type);
        }
        public static IDBObjectData DBData(object obj)
        {
            IDBObjectData dbData;
            if (ObjectMetadata.TryGetValue(obj, out dbData))
            {
                if (ReferenceEquals(dbData.Owner, obj))
                {
                    return dbData;
                }
            }
            // Not found - must check everything in the DB since GetHashCode isn't guaranteeed to be unique or to stay the same
            // There are probably ways to optimize this, in fact, it may not even be necessary, but it should be pretty
            // inexpensive unless dealing with huge numbers of objects

            foreach (KeyValuePair<object, IDBObjectData> kvps in ObjectMetadata)
            {
                if (ReferenceEquals(kvps.Value.Owner, obj))
                {
                    return kvps.Value;
                }
                else if (kvps.Value.Orphaned)
                {
                    RemoveFromDict(kvps.Key);
                }
            }

            // Definitely not in the dictionary - create it


            return CreateDBData(obj);
        }
        public static IDBObjectData CreateDBData(object obj)
        {
            if (obj is IDictionary<string, object>)
            {
                return null;
            }
            IDBObjectData newObjectData = new DBObjectData(obj);
            ObjectMetadata[obj] = newObjectData;
            return newObjectData;
        }

        #endregion

        internal static ISqlQuery CreateQuery()
        {
            return CreateQuery(QueryType.Select);
        }
        internal static ISqlQuery CreateQuery(QueryType queryType)
        {
            SqlQuery query = new SqlQuery(queryType);
            query.OptimizeParameterNames = Config.OptimizeParameterNames;
            return query;
        }
        #endregion 

       

        #region private methods


        private static void PopulateDbTypeMap()
        {
            
            Config.DbTypeMap = new Dictionary<Type, DbType>();
            var typeMap = Config.DbTypeMap;
            typeMap[typeof(bool)] = DbType.Boolean;
            typeMap[typeof(string)] = DbType.String;
            typeMap[typeof(double)] = DbType.Double;
            typeMap[typeof(decimal)] = DbType.Decimal;
            typeMap[typeof(float)] = DbType.Single;
            typeMap[typeof(int)] = DbType.Int32;
            typeMap[typeof(long)] = DbType.Int64;
            typeMap[typeof(short)] = DbType.Int16;
            typeMap[typeof(ulong)] = DbType.UInt64;
            typeMap[typeof(uint)] = DbType.UInt32;
            typeMap[typeof(ushort)] = DbType.UInt16;
            typeMap[typeof(DateTime)] = DbType.DateTime;
            typeMap[typeof(Guid)] = DbType.Guid;
            typeMap[typeof(byte[])] = DbType.Binary;
        }
       
        private static void RemoveFromDict(object obj)
        {
            IDBObjectData removed;
            ObjectMetadata.TryRemove(obj, out removed);
        }

        #endregion
    }
}