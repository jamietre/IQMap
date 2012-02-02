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
                    _Connection = Config.DefaultConnection;
                }
                return _Connection;
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
        
        public static bool Save(object obj)
        {
            return Config.DataController.Save(IQ.Config.DefaultConnection,obj);
        }

        public static T LoadPK<T>(IConvertible primaryKeyValue) where T : new()
        {
            return Config.DataController.LoadPK<T>(IQ.Config.DefaultConnection, primaryKeyValue);
        }

        public static T Load<T>(string query, params object[] parameters) 
        {
            return Config.DataController.Load<T>(IQ.Config.DefaultConnection, query, parameters);

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
        public static bool TryLoad<T>(string query, out T obj, params object[] parameters) 
        {
            return Config.DataController.TryLoad<T>(IQ.Config.DefaultConnection,query, out obj, parameters);
        }


        public static IEnumerable<T> LoadMultiple<T>(string query, params object[] parameters) 
        {
            return Config.DataController.LoadMultiple<T>(IQ.Config.DefaultConnection,query,true, parameters);
        }

        public static bool TryLoadPK<T>(IConvertible primaryKeyValue, out T obj) where T : new()
        {
            return Config.DataController.TryLoadPK<T>(IQ.Config.DefaultConnection, primaryKeyValue, out obj);
        }

        /// <summary>
        /// Delete a record from the bound table associated with type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryKeyValue"></param>
        /// <returns></returns>
        public static int DeletePK<T>(IConvertible primaryKeyValue) where T: new()
        {
            return Config.DataController.DeletePK<T>(IQ.Config.DefaultConnection, primaryKeyValue);
        }

        public static int Delete<T>(string query, params object[] parameters)
        {
            return Config.DataController.Delete<T>(IQ.Config.DefaultConnection, query, parameters);
        }
        public static int QueryScalar(string query, params object[] parameters)
        {
            return Config.DataController.QueryScalar(IQ.Config.DefaultConnection, query, parameters);
        }

        public static IDataReader Query(string query, params object[] parameters)
        {
            return Config.DataController.Query(IQ.Config.DefaultConnection, query, parameters);
        }

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

            IDBObjectData newObjectData = new DBObjectData(obj);
            ObjectMetadata[obj] = newObjectData;
            return newObjectData;

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