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
        /// <summary>
        /// Load a record expecting a single matching result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T Single<T>(this IDbConnection connection, object query, params object[] parameters)
        {
            return IQ.Config.DataController.Single<T>(query, connection, parameters);
        }

        /// <summary>
        /// Load a record, returning either the single matching value or the default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T SingleOrDefault<T>(this IDbConnection connection, object query, params object[] parameters)
        {
            return IQ.Config.DataController.SingleOrDefault<T>(query, connection, parameters);
        }

        /// <summary>
        /// Load a record expecting a single matching result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T First<T>(this IDbConnection connection, object query, params object[] parameters)
        {
            return IQ.Config.DataController.First<T>(query, connection, parameters);
        }

        /// <summary>
        /// Load a record, returning either the single matching value or the default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T FirstOrDefault<T>(this IDbConnection connection, object query, params object[] parameters)
        {
            return IQ.Config.DataController.FirstOrDefault<T>(query, connection, parameters);
        }
        /// <summary>
        /// Return the number of records matching the condition or query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static int Count<T>(this IDbConnection connection, object query, params object[] parameters)
        {
            return IQ.Config.DataController.Count<T>(query,connection, parameters);
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
        public static bool TrySingle<T>(this IDbConnection connection, object query, out T obj, params object[] parameters)
        {
            return IQ.Config.DataController.TrySingle<T>(query,out obj, connection, parameters);
        }

        public static IEnumerable<T> Select<T>(this IDbConnection connection, object query, params object[] parameters)
        {
            return IQ.Config.DataController.Select<T>(query,connection, parameters);
        }


        /// <summary>
        /// Delete record matching criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static int Delete<T>(this IDbConnection connection, object query, params object[] parameters)
        {
            return IQ.Config.DataController.Delete<T>(query, connection, parameters);
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

        public static int QueryScalar(this IDbConnection connection, string query, params object[] parameters)
        {
            return IQ.Config.DataController.QueryScalar(query, connection,parameters);
        }

        public static IDataReader Query(this IDbConnection connection,string query, params object[] parameters)
        {
            return IQ.Config.DataController.Query(query, connection, parameters);
        }
     

    }
}
