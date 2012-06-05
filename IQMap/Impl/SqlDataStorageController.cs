using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using IQMap.Impl.Support;
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;
using IQMap.QueryCache;

namespace IQMap.Impl
{
    public abstract class SqlDataStorageController: IDataStorageController
    {
        #region private properties

        protected virtual void OnMapParameter(IDataParameter input, IDataParameter output)
        {

        }

        protected virtual IDbCommand GetCommand(IDbConnection connection,
            ISqlQuery query,
            IDbTransaction transaction)
        {
            LastQuery = query;

            var lastParameters = new List<IDataParameter>();
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = query.GetQuery();

            if (query.Parameters != null)
            {
                foreach (var item in query.Parameters)
                {
                    if (HasParameter(query.GetQuery(), item.ParameterName)) {
                        IDataParameter parm;
                        if (item is QueryParameter)
                        {
                            parm = cmd.CreateParameter();
                            parm.ParameterName = item.ParameterName;
                            parm.Value = item.Value == null ? DBNull.Value : item.Value;
                            parm.DbType = item.DbType;
                            parm.Direction = item.Direction;
                            OnMapParameter(item, parm);
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

            return cmd;
        }

        protected abstract string GetQueryForRows(ISqlQuery query);

        protected abstract int InsertAndReturnNewID(IDbConnection conn,
            ISqlQuery query,
            IDbTransaction transaction = null,
            CommandBehavior commandBehavior = CommandBehavior.Default);

        #endregion

        #region DataStorageController methods

        public abstract IDbConnection GetConnection(string connectionString);

        /// <summary>
        ///  Occurs right before any query is run
        /// </summary>
        /// <param name="query"></param>
        protected virtual  void BeforeRunQuery(ISqlQuery query)
        {
            // clear the cache before actually running the update query
            ClearCacheAfterChange(query);

        }
        public virtual IDataReader RunQuery(IDbConnection connection,
            ISqlQuery query,
            IDbTransaction transaction = null,
            CommandBehavior commandBehavior = CommandBehavior.Default) 
        {
            
            IDataReader dataReader = null;
            string sql;
            if (query.FirstRow > 0 || query.TotalRows > 0)
            {
                sql = GetQueryForRows(query);
            } else {
                sql = query.GetQuery();
            }
            var newQuery = new SqlQueryDef(sql, query.Parameters);
            
            
            BeforeRunQuery(query);

            using (IDbCommand cmd = GetCommand(connection, newQuery, transaction))
            { 

                ExecuteSqlFinal(new Action(() =>
                {
                    dataReader = cmd.ExecuteReader(commandBehavior);
                }));

                cmd.Parameters.Clear();
            }

            OnQueryComplete();
            
            return dataReader;
        }

        public virtual IDataReader RunQuery(IDbConnection connection,
           ISqlQuery query, out int rows,
          IDbTransaction transaction = null,
          CommandBehavior commandBehavior=CommandBehavior.Default)
        {
            rows = Count(connection, query);
            var reader = RunQuery(connection, query,transaction,commandBehavior);
            
            return reader;
        }

        protected virtual int Count(IDbConnection connection, 
            string tableName,
            string where,
            IEnumerable<IDataParameter> parameters,
            IDbTransaction transaction,
            CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            var countQuery = new SqlQueryDef(String.Format("SELECT COUNT(*) FROM {0}{1}", tableName, 
                (!String.IsNullOrEmpty(where) ? 
                    " WHERE " + where : 
                    "")),
                parameters);

            return RunQueryScalar(connection, countQuery, transaction, commandBehavior);
        }
        public virtual int Count(IDbConnection connection,
            ISqlQuery query,
            IDbTransaction transaction = null,
            CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            int count;
            if (query is ISqlQueryMaker)
            {
                var comp = (ISqlQueryMaker)query;
                count= Count(connection,comp.TableName,comp.Where.ToString(),comp.Parameters,transaction,commandBehavior);
            }
            else
            {
                ValidateQueryType(query.GetQuery(), "select");
                var countQuery = new SqlQueryDef("SELECT COUNT(*) FROM (" + query + ") q", query.Parameters);
                count= RunQueryScalar(connection, countQuery,transaction, commandBehavior);
            }
            count -= query.FirstRow;
            if (query.TotalRows > 0)
            {
                count = Math.Min(count, query.TotalRows);
            }
            return count;

        }
      
        public virtual int RunQueryScalar(IDbConnection connection,
            ISqlQuery query,
            IDbTransaction transaction = null,
            CommandBehavior commandBehavior = CommandBehavior.Default)
        {

            int result = 0;
            using (IDataReader reader =  RunQuery(connection,query,transaction: transaction, commandBehavior: commandBehavior)) 
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
            ISqlQuery query,
            IDbTransaction transaction = null,
            CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            return InsertAndReturnNewID(connection,query,transaction,commandBehavior);
        }

        public virtual IDataReader RunStoredProcedure(IDbConnection connection,
             ISqlQuery query,
           IDbTransaction transaction = null,
           CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            throw new NotImplementedException("Can't use SP.");
            //if (query.FirstRow >0 || query.TotalRows > 0)
            //{
            //    throw new NotImplementedException("Can't use row selection for SP results... yet.");
            //}

            //IDataReader reader = null;
            //using (IDbCommand cmd = GetCommand(connection, query, transaction))
            //{
            //    cmd.CommandType = CommandType.StoredProcedure;

            //    ExecuteSqlFinal(new Action(() =>
            //    {
            //        reader = cmd.ExecuteReader(commandBehavior);
            //    }));

            //    cmd.Parameters.Clear();
            //}

            //OnQueryComplete();
            //return reader;

        }

        #endregion

        #region public methods

        public virtual ISqlQuery LastQuery { get; protected set; }

        /// <summary>
        /// Only works for MSSQL - trys to map the query replacing parameter values with actual values
        /// </summary>
        /// <returns></returns>
        public virtual string LastQueryAudit()
        {
            return SqlQueryUtility.QueryAsSql(LastQuery.GetQuery(), LastQuery.Parameters.Select(item=>(System.Data.SqlClient.SqlParameter)item));

        }
        
        #endregion

        #region private methods
        private static HashSet<string> ValidQueryTypes = new HashSet<string>(new string[] {"select","insert","update","delete"});
        protected void ValidateQueryType(string query, string match)
        {

            string qType = GetQueryType(query);
            if (qType != match.ToLower())
            {
                throw new InvalidOperationException("The query passed was not a " + match + " query.");
            }
        }
        /// <summary>
        /// return the first word, basically
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public string GetQueryType(string query)
        {
            string q = query.Trim();
            int space = q.IndexOf(" ");
            string qType;
            if (space >= 0)
            {
                qType = q.Substring(0, space).ToLower();
            }
            else
            {
                qType = q.ToLower();
            }
            if (!ValidQueryTypes.Contains(qType))
            {
                return "";
            }
            else
            {
                return qType;
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
                List<string> parmList = new List<string>(SqlQueryUtility.GetParameterNames(querySql));
                
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
        protected void ClearCacheAfterChange(ISqlQuery query)
        {
            if (query.QueryType == QueryType.Delete ||
                query.QueryType == QueryType.Insert ||
                query.QueryType == QueryType.Update)
            {

                if (query is ISqlQueryMaker)
                {
                    SqlQueryBuilderConfig.RemoveAllForTable(((ISqlQueryMaker)query).TableName);
                }
                else
                {
                    SqlQueryBuilderConfig.Clear();
                }

            }

        }
        #endregion
       
    }
}