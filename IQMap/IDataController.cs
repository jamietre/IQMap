using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace IQMap
{
    public interface IDataController
    {
        IDbConnection GetConnection(string connectionString);
        /// <summary>
        /// Saves (inserts or updates) a record.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>
        /// A "false" return value means no data was changed. This can only happen for an update
        /// query. An sql failure (e.g. couldn't be inserted) or any other problem 
        /// will cause an error to be thrown.
        /// </returns>
        bool Save(IDbConnection connection, object obj);

        T LoadPK<T>(IDbConnection connection, IConvertible primaryKeyValue) where T : new();

        T Load<T>(IDbConnection connection, string query, params object[] parameters);
        
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
        bool TryLoad<T>(IDbConnection connection, string query, out T obj, params object[] parameters);
        bool TryLoadPK<T>(IDbConnection connection, IConvertible primaryKeyValue, out T obj) where T : new();

        IEnumerable<T> LoadMultiple<T>(IDbConnection connection, string query, bool buffered, params object[] parameters);

        int DeletePK<T>(IDbConnection connection, IConvertible primaryKeyValue) where T : new();
        int Delete<T>(IDbConnection connection, string query, params object[] parameters);

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
        // TODO: This should extend IEnumerble?
        ///int Merge<T>(string query, string mergeKey, IEnumerable<T> data, bool destructive=false);

        IDataReader Query(IDbConnection connection, string query, params object[] parameters);
        int QueryScalar(IDbConnection connection, string query, params object[] parameters);




    }
}