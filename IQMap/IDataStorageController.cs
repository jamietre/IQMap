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
        IDataReader RunQuery(IDbConnection connection, string query, 
            IEnumerable<IDataParameter> parameters = null, 
            int firstRow=-1, 
            int lastRow=-1,
            IDbTransaction transaction = null);
        
        /// <summary>
        /// Run a select query and also return the number of rows
        /// </summary>
        /// <param name="query"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        IDataReader RunQuery(IDbConnection connection, string query,
            out int rows,
            IEnumerable<IDataParameter> parameters = null,
            int firstRow = -1,
            int lastRow = -1,
            IDbTransaction transaction = null);
        /// <summary>
        /// Run a select query, but only return the number of rows
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int Count(IDbConnection connection, string query, IEnumerable<IDataParameter> parameters = null);
        
        /// <summary>
        /// Run a query, and return the first numeric value of the first row, or records affected
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int RunQueryScalar(IDbConnection connection, string query, IEnumerable<IDataParameter> parameters = null, IDbTransaction transaction = null);
        /// <summary>
        /// Returns a new auto-generated primary key value for an insert
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int RunQueryInsert(IDbConnection connection, string query, IEnumerable<IDataParameter> parameters = null, IDbTransaction transaction = null);
      
    }
}