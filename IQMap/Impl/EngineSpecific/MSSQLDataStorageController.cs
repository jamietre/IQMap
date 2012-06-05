using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using IQMap.SqlQueryBuilder.Impl;
using IQMap.SqlQueryBuilder;

namespace IQMap.Impl
{
    public class MSSQLDataStorageController: SqlDataStorageController
    {

        #region public methods

        public override IDbConnection GetConnection(string connectionString)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            return conn;
            
        }


        /// Runs a stored procedure
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="QueryParameters"></param>
        /// <param name="OutputParameters"></param>
        /// <param name="DB"></param>
        public virtual void RunStoredProcedure(IDbConnection connection, string procedureName,
            IEnumerable<IDataParameter> inputParameters, 
            IEnumerable<IDataParameter> outputParameters,
            IDbTransaction transaction,
            CommandBehavior commandBehavior)
        {

            SqlCommand cmd = new SqlCommand(procedureName, (SqlConnection)connection);
            cmd.CommandType = CommandType.StoredProcedure;

            ProcessSql(procedureName, inputParameters);

            foreach (var parm in inputParameters)
            {
                cmd.Parameters.Add(parm);
            }
            if (outputParameters != null)
            {
                foreach (SqlParameter parm in outputParameters)
                {
                    parm.Direction = ParameterDirection.Output;
                    //OutputParameters[i].Value = -1;
                    cmd.Parameters.Add(parm);
                }
            }

            ExecuteSqlFinal(new Action(() =>
            {
                cmd.ExecuteScalar();
            }));
            if (commandBehavior == CommandBehavior.CloseConnection)
            {
                connection.Close();
            }
            OnQueryComplete();
        }
        public virtual SqlDataReader RunStoredProcedureDataset(IDbConnection connection, 
            string procedureName, 
            IEnumerable<IDataParameter> queryParameters,
            IDbTransaction transaction,
            CommandBehavior commandBehavior)
        {
            return RunStoredProcedureDataset(connection,procedureName, queryParameters, null,transaction, commandBehavior);
        }
        public virtual SqlDataReader RunStoredProcedureDataset(IDbConnection connection, 
            string procedureName, IEnumerable<IDataParameter> queryParameters, IEnumerable<IDataParameter> outputParameters,
            IDbTransaction transaction,
            CommandBehavior commandBehavior = CommandBehavior.Default)
        {

            SqlDataReader reader=null;
            using (SqlCommand cmd = new SqlCommand(procedureName, (SqlConnection)connection))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                ProcessSql(procedureName, null);

                foreach (var parm in queryParameters)
                {
                    cmd.Parameters.Add(parm);
                }
                if (outputParameters != null)
                {
                    foreach (SqlParameter parm in outputParameters)
                    {
                        parm.Direction = ParameterDirection.Output;
                        //OutputParameters[i].Value = -1;
                        cmd.Parameters.Add(parm);
                    }
                }
                if (transaction != null)
                {
                    cmd.Transaction = (SqlTransaction)transaction;
                }

                ExecuteSqlFinal(new Action(() =>
                {
                    reader = cmd.ExecuteReader(commandBehavior);
                }));

                cmd.Parameters.Clear();
            }
            OnQueryComplete();
            return reader;
        }

        /// <summary>
        ///  Run a stored procedure, and return a single scalar value
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="QueryParameters"></param>
        /// <param name="DB"></param>
        /// <returns></returns>
        public virtual int RunStoredProcedureScalar(IDbConnection conn, string procedureName, SqlParameter[] parameters, 
            IDbTransaction transaction=null,
            CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            ProcessSql(procedureName, parameters);

            SqlParameter ReturnParameter = new SqlParameter("RETURN_VALUE", SqlDbType.Int);
            ReturnParameter.Value = -1;

            using (SqlCommand cmd = new SqlCommand(procedureName, (SqlConnection)conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                for (int i = 0; i <= parameters.GetUpperBound(0); i++)
                {
                    cmd.Parameters.Add(parameters[i]);
                }

                ReturnParameter.Direction = ParameterDirection.ReturnValue;

                cmd.Parameters.Add(ReturnParameter);

                ExecuteSqlFinal(new Action(() =>
                {
                    cmd.ExecuteScalar();
                }));

                cmd.Parameters.Clear();
            }
            if (commandBehavior == CommandBehavior.CloseConnection)
            {
                conn.Close();
            }
            OnQueryComplete();
            return Convert.ToInt32(ReturnParameter.Value);
        }


        #endregion




        /// <summary>
        /// Note that FirstRow is zero-indexed, so 0 means ignore (or the first row should be the usual first row).
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override string GetQueryForRows(ISqlQuery query)
        {
            if (query.FirstRow == 0)
            {
                SqlClause clause = SqlClause.From | SqlClause.Where |
                    SqlClause.Having | SqlClause.GroupBy | SqlClause.OrderBy;

                if (query is ISqlQueryMaker)
                {
                    var comp = (ISqlQueryMaker)query;
                    return String.Format("SELECT TOP {0} {1} {2}",
                        comp.TotalRows,
                        comp.Select,
                        comp.GetQuery( clause));
                } else {
                    string sql = query.GetQuery();
                    int pos = sql.ToLower().LastIndexOf("order by");
                    string order="";
                    if (pos>0) {
                        order = sql.Substring(pos);
                        sql = sql.Substring(0,pos-1);
                    }
                    return String.Format("SELECT TOP {0} * FROM ({1}) __subquery {2}",
                        query.TotalRows,
                        sql,
                        order);
                    }
            }
            else
            {
                if (query is ISqlQueryMaker)
                {
                    var comp = (ISqlQueryMaker)query;
                    string querySQL = "";
                    string where = comp.Where.ToString();
                    
                    // always us PK as the final sort to eliminate possible paging problems

                    string orderByClause  = " ORDER BY " + comp.OrderBy.Clone().AddIfNotPresent(comp.PrimaryKey).ToString();


                    // Add TablePrimaryKey to sort phrase to ensure that there is a consistent sort order, if the user-defined order is not explicit for every record.
                    // Pagination can be wrong otherwise.

                    //SortPhrase += (SortOrder==""?"":",")+TablePrimaryKey;


                    if (comp.TotalRows > 0)
                    {
                        querySQL = @"SELECT " + comp.Select + " FROM " + comp.From + " WHERE " + comp.PrimaryKey + " IN " +
                            "(SELECT TOP " + comp.TotalRows + " " + comp.PrimaryKey + " FROM " + comp.From + " WHERE [WhereSubstitution1]" + comp.PrimaryKey + " NOT IN " +
                            "(SELECT TOP " + comp.FirstRow + " " + comp.PrimaryKey + " FROM " + comp.From + "[WhereSubstitution2]" + orderByClause + ")" +
                            orderByClause + ")" +
                            orderByClause;
                    }
                    else
                    {
                        querySQL = @"SELECT " + comp.Select + " FROM " + comp.From + " WHERE " + comp.PrimaryKey + " NOT IN " +
                         "(SELECT TOP " + comp.FirstRow + " " + comp.PrimaryKey + " FROM " + comp.From + "[WhereSubstitution2]" + orderByClause + ")" +
                         orderByClause;

                    }

                    querySQL = querySQL.Replace("[WhereSubstitution1]", where!="" ?
                        where + " AND " : "");
                    querySQL = querySQL.Replace("[WhereSubstitution2]", where != "" ? 
                        " WHERE " + where : "");


                    return querySQL;
                }
                else
                {
                    throw new InvalidOperationException("I can't do paging in an ad-hoc query.");
                }

            }
        }

        protected override int InsertAndReturnNewID(IDbConnection conn,
            ISqlQuery query,
            IDbTransaction transaction = null,
            CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            int result = 0;
            BeforeRunQuery(query);
            var newQuery= new SqlQueryDef(query.GetQuery() + "; SET @___ID=SCOPE_IDENTITY();", query.Parameters);

            
            using (IDbCommand cmd = GetCommand(conn, newQuery, transaction))
            {

                IDataParameter ID = cmd.CreateParameter();
                ID.ParameterName = "@___ID";
                ID.DbType  =  System.Data.DbType.Int32;
                ID.Direction = ParameterDirection.Output;
                ID.Value = -1;
                cmd.Parameters.Add(ID);

                ExecuteSqlFinal(new Action(() =>
                {
                    cmd.ExecuteScalar();
                }));

                result = Convert.ToInt32(ID.Value.ToString());
                cmd.Parameters.Clear();
                cmd.Dispose();
            }
            if (commandBehavior == CommandBehavior.CloseConnection)
            {
                conn.Close();
            }
            OnQueryComplete();
            return result;
        }
    }
}