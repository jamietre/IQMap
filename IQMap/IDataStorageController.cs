using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace IQMap
{
    /// <summary>
    /// A generic interface to a data control mechanism.
    /// </summary>
    public interface IDataStorageController
    {
        IDbConnection GetConnection(string connectionString);
        /// <summary>
        /// Run a select query, return the result dataset
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        IDataReader RunQuery(IDbConnection connection, 
            ISqlQuery query,
            IDbTransaction transaction = null,
            CommandBehavior commandBehavior = CommandBehavior.Default);
        
        /// <summary>
        /// Run a select query and also return the number of rows
        /// </summary>
        /// <param name="query"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        IDataReader RunQuery(IDbConnection connection, ISqlQuery query,
            out int rows,
            IDbTransaction transaction = null,
            CommandBehavior commandBehavior = CommandBehavior.Default);
        /// <summary>
        /// Run a select query, but only return the number of rows
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        //int Count(IDbConnection connection, 
        //    string tableName,
        //    string Where,
        //    IEnumerable<IDataParameter> parameters,
        //    CommandBehavior commandBehavior = CommandBehavior.Default);

        int Count(IDbConnection connection,
            ISqlQuery query,
            IDbTransaction transaction = null,
            CommandBehavior commandBehavior = CommandBehavior.Default);
        /// <summary>
        /// Run a query, and return the first numeric value of the first row, or records affected
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int RunQueryScalar(IDbConnection connection, ISqlQuery query,
            IDbTransaction transaction = null,
            CommandBehavior commandBehavior = CommandBehavior.Default);
        /// <summary>
        /// Returns a new auto-generated primary key value for an insert
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int RunQueryInsert(IDbConnection connection,
            ISqlQuery query,
            IDbTransaction transaction = null,
            CommandBehavior commandBehavior = CommandBehavior.Default);


        IDataReader RunStoredProcedure(IDbConnection connection,
            ISqlQuery query,
           IDbTransaction transaction = null,
           CommandBehavior commandBehavior = CommandBehavior.Default);
        
    }
}