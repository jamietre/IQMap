using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace IQMap.Implementation
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
            IDbTransaction transaction)
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

            OnQueryComplete();
        }
        public virtual SqlDataReader RunStoredProcedureDataset(IDbConnection connection, 
            string procedureName, 
            IEnumerable<IDataParameter> queryParameters,
            IDbTransaction transaction)
        {
            return RunStoredProcedureDataset(connection,procedureName, queryParameters, null,transaction);
        }
        public virtual SqlDataReader RunStoredProcedureDataset(IDbConnection connection, 
            string procedureName, IEnumerable<IDataParameter> queryParameters, IEnumerable<IDataParameter> outputParameters,
            IDbTransaction transaction)
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
                    reader = cmd.ExecuteReader(CurrentCommandBehavior(transaction));
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
        public virtual int RunStoredProcedureScalar(IDbConnection conn, string procedureName, SqlParameter[] parameters, IDbTransaction transaction=null)
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
            if (CurrentCommandBehavior(transaction)== CommandBehavior.CloseConnection)
            {
                conn.Close();
            }
            OnQueryComplete();
            return Convert.ToInt32(ReturnParameter.Value);
        }


        #endregion





        protected override string GetQueryForRows(string query, int firstRow, int totalRows)
        {
            throw new NotImplementedException();
        }

        protected override int InsertAndReturnNewID(IDbConnection conn, string query, IEnumerable<IDataParameter> parameters = null, IDbTransaction transaction = null)
        {
            int result = 0;
            string sql = query + "; SET @ID=SCOPE_IDENTITY();";


            using (SqlCommand cmd = (SqlCommand)GetCommand(conn,sql,parameters,transaction))
            {

                SqlParameter ID = cmd.Parameters.Add("@ID", System.Data.SqlDbType.Int);
                ID.Direction = ParameterDirection.Output;
                ID.Value = -1;

                ExecuteSqlFinal(new Action(() =>
                {
                    cmd.ExecuteScalar();
                }));

                result = Convert.ToInt32(ID.Value.ToString());
                cmd.Parameters.Clear();
                cmd.Dispose();
            }
            OnQueryComplete();
            return result;
        }
    }
}