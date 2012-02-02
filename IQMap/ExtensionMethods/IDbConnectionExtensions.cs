using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Data;
using IQMap.Implementation;

namespace IQMap
{
    public static class IDbConnectionExtensions
    {
        public static int QueryScalar(this IDbConnection connection, string query, params object[] parameters)
        {
            return IQ.Config.DataController.QueryScalar(connection, query, parameters);
        }

        public static IDataReader Query(this IDbConnection connection,string query, params object[] parameters)
        {
            return IQ.Config.DataController.Query(connection, query, parameters);
        }
        public static T LoadPK<T>(this IDbConnection connection, IConvertible primaryKeyValue) where T : new()
        {
            return IQ.Config.DataController.LoadPK<T>(IQ.Config.DefaultConnection, primaryKeyValue);
        }

        public static T Load<T>(this IDbConnection connection, string query, params object[] parameters)
        {
            return IQ.Config.DataController.Load<T>(IQ.Config.DefaultConnection, query, parameters);

        }


        public static IEnumerable<T> LoadMultiple<T>(this IDbConnection connection, string query, params object[] parameters)
        {
            return IQ.Config.DataController.LoadMultiple<T>(IQ.Config.DefaultConnection, query,true,parameters);
        }



        //public static T Load<T>(this IDbConnection conn, IConvertible primaryKeyValue) where T : IDatabaseBoundObject, new()
        //{
        //    return DataController.Load<T>(primaryKeyValue);
        //}
        //public static T Load<T>(this IDbConnection conn, IConvertible primaryKeyValue) where T : IDatabaseBoundObject, new()
        //{
        //    return DataController.Load<T>(
        //}

        //public T Load<T>(string fieldName, IConvertible value) where T : IDatabaseBoundObject, new()
        //{

        //    T obj = new T();
        //    string error = LoadSingleInto<T>(obj, fieldName, value);
        //    if (!string.IsNullOrEmpty(error))
        //    {
        //        throw new Exception(error);
        //    }
        //    return obj;
        //}

        //public bool TryLoad<T>(string fieldName, IConvertible value, out T obj) where T : IDatabaseBoundObject, new()
        //{

        //    T target = new T();
        //    string error = LoadSingleInto<T>(target, fieldName, value);
        //    if (!string.IsNullOrEmpty(error))
        //    {
        //        obj = default(T);
        //        return false;
        //    }
        //    else
        //    {
        //        obj = target;
        //        return true;
        //    }
        //}

        //public bool TryLoad<T>(IConvertible primaryKeyValue, out T obj) where T : IDatabaseBoundObject, new()
        //{
        //    T target = new T();
        //    string error = LoadSingleInto<T>(target, target.DBData().ClassInfo.PrimaryKey.Name, primaryKeyValue);
        //    if (!string.IsNullOrEmpty(error))
        //    {
        //        obj = default(T);
        //        return false;
        //    }
        //    else
        //    {
        //        obj = target;
        //        return true;
        //    }
        //}

        //public IEnumerable<T> LoadMultiple<T>(IQuery query) where T : IDatabaseBoundObject, new()
        //{
        //    T target = new T();
        //    IDBObjectData dbData = target.DBData();
        //    DBClassInfo info = dbData.ClassInfo;

        //    query.AddFieldMap(info.FieldNameMap);
        //    query.From = dbData.TableName;
        //    return LoadMultiple<T>(target, dbData, query.ToString());
        //}

        //public IEnumerable<T> LoadMultiple<T>(string query) where T : IDatabaseBoundObject, new()
        //{
        //    T target = new T();
        //    IDBObjectData dbData = target.DBData();

        //    return LoadMultiple<T>(target, dbData, query);
        //}


        //public IDataReader RunQuery(IQuery query)
        //{
        //    return RunQuery(query.GetQuery());
        //}

        //public IDataReader RunQuery(string query)
        //{
        //    return StorageController.Select(query);
        //}

        //public int RunQueryScalar(IQuery query)
        //{
        //    return RunQueryScalar(query.ToString());
        //}

        //public int RunQueryScalar(string query)
        //{
        //    return StorageController.Scalar(query.ToString());
        //}

    }
}
