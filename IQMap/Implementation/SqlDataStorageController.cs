using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace IQMap.Implementation
{
    public abstract class SqlDataStorageController: IDataStorageController
    {
        #region private properties


        protected virtual IDbCommand GetCommand(IDbConnection connection,string query, IEnumerable<IDataParameter> parameters, IDbTransaction transaction)
        {
            LastQuery = query;
            var lastParameters = new List<IDataParameter>();
            IDbCommand cmd = connection.CreateCommand();
            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    if (HasParameter(query, item.ParameterName)) {
                        IDataParameter parm;
                        if (item is QueryParameter)
                        {
                            parm = cmd.CreateParameter();
                            parm.ParameterName = item.ParameterName;
                            parm.Value = item.Value == null ? DBNull.Value : item.Value;
                            parm.DbType = item.DbType;
                            parm.Direction = item.Direction;
                        }
                        else
                        {
                            parm = item;
                        }
                        cmd.Parameters.Add(parm);
                        lastParameters.Add(parm);
                    }
                }
            }
            if (transaction != null) {
                cmd.Transaction = transaction;
            }
            LastParameters = lastParameters;
            return cmd;
        }
        
        protected abstract string GetQueryForRows(string query, int firstRow, int totalRows);
        protected abstract int InsertAndReturnNewID(IDbConnection conn, string sql, IEnumerable<IDataParameter> parameters = null, IDbTransaction transaction=null);

        protected virtual CommandBehavior CurrentCommandBehavior(IDbTransaction trans)
        {
            return trans == null ? CommandBehavior.CloseConnection : CommandBehavior.Default;

        }
        #endregion

        #region DataStorageController methods

        public abstract IDbConnection GetConnection(string connectionString);

        public virtual IDataReader RunQuery(IDbConnection connection, string query, 
            IEnumerable<IDataParameter> parameters = null, 
            int firstRow=-1, 
            int totalRows=-1, 
            IDbTransaction transaction=null) 
        {
            
            IDataReader dataReader = null;
            string sql;
            if (firstRow >= 0 || totalRows >= 0)
            {
                sql = GetQueryForRows(query, firstRow, totalRows);
            } else {
                sql = query;
            }
            sql = ProcessSql(sql,parameters);

            using (IDbCommand cmd = GetCommand(connection,query,parameters,transaction))
            {

                cmd.CommandText = sql;

                ExecuteSqlFinal(new Action(() =>
                {
                    dataReader = cmd.ExecuteReader(CurrentCommandBehavior(transaction));
                }));

                cmd.Parameters.Clear();
            }

            //RemoveCommandParameters(cmd);
            OnQueryComplete();
            return dataReader;
        }

        public virtual IDataReader RunQuery(IDbConnection connection, string query, out int rows,
          IEnumerable<IDataParameter> parameters = null,
          int firstRow = -1,
          int totalRows = -1,
          IDbTransaction transaction = null)
        {
            rows = Count(connection, query, parameters);
            return RunQuery(connection, query, parameters,firstRow, totalRows, transaction);
        }
        public virtual int Count(IDbConnection connection, string query, IEnumerable<IDataParameter> parameters = null)
        {
            ValidateQueryType(query, "select");
            string countQuery = "SELECT COUNT(*) FROM (" + query + ") q";
            return RunQueryScalar(connection,countQuery, parameters);
        }

        public virtual int RunQueryScalar(IDbConnection connection, string query, IEnumerable<IDataParameter> parameters = null, IDbTransaction transaction = null)
        {

            int result = 0;
            using (IDataReader reader =  RunQuery(connection,query,parameters,transaction: transaction)) 
            {
                if (reader.Read())
                {
                    result = reader.GetInt32(0);
                }
                else
                {
                    result = reader.RecordsAffected;
                }
            }
            return result;
        }

        public virtual int RunQueryInsert(IDbConnection connection, 
            string query, 
            IEnumerable<IDataParameter> queryParameters = null, 
            IDbTransaction transaction = null)
        {
            return InsertAndReturnNewID(connection,query, queryParameters,transaction);
        }      

        #endregion

        #region public methods

        public virtual string LastQuery { get; protected set; }

        /// <summary>
        /// Only works for MSSQL - trys to map the query replacing parameter values with actual values
        /// </summary>
        /// <returns></returns>
        public virtual string LastQueryAudit()
        {
            return Utils.QueryAsSql(LastQuery, LastParameters.Select(item=>(System.Data.SqlClient.SqlParameter)item));

        }

        public virtual IEnumerable<IDataParameter> LastParameters { get; protected set; }


        #endregion

        #region private methods

        protected void ValidateQueryType(string query, string match)
        {
            string q = query.Trim();
            int space = q.IndexOf(" ");
            string qType;
            if (space >= 0)
            {
                qType = q.Substring(0, space);
            }
            else
            {
                qType = q;
            }
            if (qType.ToLower() != match.ToLower())
            {
                throw new Exception("The query passed was not an " + match + " query.");
            }
        }

        protected string CleanSql(string sql)
        {
            string cleanSql = sql.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");
            while (cleanSql.IndexOf("  ") > 0)
            {
                cleanSql = cleanSql.Replace("  ", " ");
            }
            return cleanSql;
        }

        /// <summary>
        /// This wraps all SQL database calls so that descendant objects can override it
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        protected virtual void ExecuteSqlFinal(Action function)
        {
            function();
        }


        // Map @@paramaters directly (e.g. for "select in")
        protected string ProcessSql(string querySql, IEnumerable<IDataParameter> parameters)
        {
            // Get rid of whitespace
            string sql = CleanSql(querySql);
            if (parameters != null)
            {
                List<Tuple<string,int,int>> parmList = new List<Tuple<string,int,int>>(Utils.GetParameterNames(querySql));
                
                foreach (var parm in parameters)
                {
                    // turn @@x into the actual value
                    if (sql.Contains("@" + parm.ParameterName))
                    {
                        sql = querySql.Replace("@" + parm.ParameterName, parm.Value.ToString());
                    }
                    // Remove null-valued parameters
                    if (parm.Value == null || parm.Value == DBNull.Value)
                    {
                        // TODO: Look for expression on either side of parameter, replace = with IS


                    }

                }
            }

            return sql;
        }
        private bool HasParameter(string sql, string ParameterName)
        {
            return (sql.IndexOf(ParameterName) >= 0);
        }
        protected virtual void OnQueryComplete()
        {

        }
        #endregion




       
    }
}