using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.Implementation
{
    /// <summary>
    /// Handles processing of a query + parameters into something that can be run. Extracts special parameter types 
    /// (CommandBehavior, Buffered, Transaction); turns a query that's a field + a single parameter into a select x=y query;
    /// turns a query that's a single field + a numeric parameter into a primary key selector
    /// </summary>
    public class SqlQueryParser<T>
    {
        #region constructors


        public SqlQueryParser(QueryType type, object query)
        {
            Initialize();

            ProcessQuery(type, query, null);

        }

        public SqlQueryParser(QueryType type, object query, object[] parameters)
        {
            Initialize();

            ProcessQuery(type, query, parameters);

        }

        protected void Initialize()
        {
            Buffered = true;

        }
        #endregion

        #region public properties

        protected IDbConnection _Connection;
        protected IDbTransaction _Transaction;
        public CommandBehavior CommandBehavior { get; protected set; }
        public IDbConnection Connection
        {
            get
            {
                return _Connection ?? IQ.Connection;
            }
            protected set
            {
                _Connection = value;
            }
        }

        public IDbTransaction Transaction { 
            get 
            {
                return _Transaction ?? null;
            }
            protected set 
            {
                _Transaction = value;
            } 
        }
        public IQuery Query { get; protected set; }
        public bool Buffered { get; protected set; }
        
        #endregion

        #region public methods

        public void ProcessQuery(QueryType type,object query, object[] parameters)
        {

            ParseQuery(type, query, parameters);
        }

        public void ExpectSelectQuery()
        {
            if (Query.QueryType != QueryType.Select)
            {
                throw new IQException("The query was not a SELECT query.");
            }
        }

        public IDataReader RunQuery(IDataStorageController controller)
        {
            return controller.RunQuery(Connection, Query.GetQuery(), Query.Parameters, transaction: Transaction, commandBehavior: CommandBehavior);
        }

        public int RunQueryScalar(IDataStorageController controller)
        {
            //if (Query.QueryType == QueryType.Delete && String.IsNullOrEmpty(Query.Where))
            //{
            //    throw new IQException("Delete query with no where criteria.");
            //}
            return controller.RunQueryScalar(Connection, Query.GetQuery(), Query.Parameters, transaction: Transaction, commandBehavior: CommandBehavior);
        }
        /// <summary>
        /// Throws an exception if this is not ISqlQuery
        /// </summary>
        /// <returns></returns>
        public ISqlQuery GetQuery()
        {
            if (Query is ISqlQuery)
            {
                return (ISqlQuery)Query;
            }
            else
            {
                throw new IQException("The query was generated from a raw query, can't be returned as ISqlQuery.");
            }
        }

        #endregion

        #region private methods

        protected void CopyDbOptions(ParameterParser pp)
        {
            Connection = pp.Connection;
            Transaction = pp.Transaction;
            Buffered = pp.Buffered;
            CommandBehavior = pp.CommandBehavior;
        }

        protected IQuery ParseComplexQuery(QueryType type, object query, IEnumerable<object> parms)
        {
            IQuery outputQuery=null;
            
            // We always want to parse the parameters. But if the thing passed to us as "query" is not a string, then 
            // just assume that all the parms are option type parameters and don't pass a query to ParameterParser
            string querySource = query is string ? 
                (string)query : 
                "";
            
            ParameterParser pp = new ParameterParser(querySource, parms);
            CopyDbOptions(pp);


            if (Utils.IsNumericType(query))
            {
                // It's a single numeric value - assume it's a primary key

                ExpectNoParameters(pp.Parameters);

                var classInfo = IQ.GetClassInfo<T>();

                ISqlQuery queryPK = classInfo.Query(type);
                queryPK.AddWhereParam(classInfo.PrimaryKey.Name, query);
                outputQuery = queryPK;
            }
            else if (query is string)
            {
                bool isMappable = Utils.IsMappableClass<T>();

                // First check if its a single named field

                if (isMappable)
                {
                    var classInfo = IQ.GetClassInfo<T>();
                    
                    // Try to create a valid raw query.. if it's not valid, assume it's a where
                    if (pp.QueryType == QueryType.Invalid)
                    {
                        ISqlQuery queryPK = classInfo.Query(type);

                        queryPK.AddWhere(pp.Query);
                        queryPK.AddParameter(pp.Parameters);
                        outputQuery = queryPK;
                    }
                    else
                    {
                        outputQuery = pp.GetIQuery();
                    }
                    
                }
                else
                {
                    // it's mapped to a primitive type - 
                    outputQuery = pp.GetIQuery();
                }
            }
            if (outputQuery.QueryType != type)
            {
                throw new IQException("Wrong type of query passed to method: was " + outputQuery.ToString() + ", expected " + type.ToString());
            }

            return outputQuery;
        }

        /// <summary>
        /// When passing type=0, it's never going to try to build a query - only fully formed query SQl will work.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        protected void ParseQuery(QueryType type,object query, IEnumerable<object> parameters)
        {
            IQuery outputQuery;

            if (query == null)
            {
                throw new IQMissingQueryException();
            }
            else if (query is IQuery)
            {
                ExpectNoParameters(parameters);
                if (type != 0 && ((IQuery)query).QueryType != type)
                {
                    throw new IQException("The query passed was not of type '" + type.ToString() + "' as required for this operation.");
                }
                outputQuery= (IQuery)query;
            }
            else if (type != 0)
            {
                outputQuery = ParseComplexQuery(type,query,parameters);
            }
            else if (query is string) {
                string queryString = (string)query;
                
                ParameterParser pp = new ParameterParser(queryString, parameters);
                CopyDbOptions(pp);

                outputQuery = new SqlQueryRaw(queryString, pp.Parameters);
            }
            else
            {
                throw new IQException("The type of object '" + query.GetType().ToString() + "' passed as a query isn't something I know how to handle.");
            }

            if (outputQuery == null)
            {
                throw new IQMissingQueryException();
            }

            if (outputQuery.QueryType == QueryType.Invalid)
            {
                throw new IQException("Couldn't make a valid query out of the mess you passed me. Check the SQL and the parameters.");
            }

            Query = outputQuery;

        }

        protected void ExpectNoParameters(IEnumerable<object> parameters)
        {
            if (parameters != null && parameters.Count() > 0)
            {
                throw new IQException("The query appeared to be a primary key type, but parameters were passed.");
            }
        }


        #endregion
    }
}
