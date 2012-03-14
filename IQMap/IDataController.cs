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
        /// Load a record expecting a single matching result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        T Single<T>(object query, params object[] parameters);
        
        /// <summary>
        /// Load a record, returning either the single matching value or the default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        T SingleOrDefault<T>(object query, params object[] parameters);

        /// <summary>
        /// Load a record expecting a single matching result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        T First<T>(object query, params object[] parameters);
        /// <summary>
        /// Load a record, returning either the single matching value or the default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        T FirstOrDefault<T>(object query, params object[] parameters);
        
        /// <summary>
        /// Return the number of records matching the condition or query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int Count<T>(object query, params object[] parameters);

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
        bool TrySingle<T>(object query, out T obj, params object[] parameters);
        
        /// <summary>
        /// Try to load a single record for field/value combination into an existing object. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <param name="obj"></param>
        /// <returns>
        /// If missing, return false. If more than one match is found, throw an exception.
        /// </returns>
        bool TrySingle<T>(object query, T obj, params object[] parameters);

        IEnumerable<T> Select<T>(object query, params object[] parameters);

        /// <summary>
        /// Saves (inserts or updates) a record.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>
        /// A "false" return value means no data was changed. This can only happen for an update
        /// query. An sql failure (e.g. couldn't be inserted) or any other problem 
        /// will cause an error to be thrown.
        /// </returns>
        bool Save(object obj, params object[] parameters);

        /// <summary>
        /// Delete record matching criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int Delete<T>(object query, params object[] parameters);

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

        IDataReader Query(string query, params object[] parameters);
        int QueryScalar(string query, params object[] parameters);

        IEnumerable<T> RunStoredProcedure<T>(string spName, params object[] parameters);
       //int RunStoredProcedureScalar(string spName, params object[] parameters);




    }
}