using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;
using IQObjectMapper;

namespace IQMap.Impl.Support
{
    /// <summary>
    /// The purpose of this interface is to enable passing the parser object without a generic type. There's only
    /// one implementation though.
    /// </summary>
    public interface ISqlQueryParser
    {
        //IDataReader RunQuery(IDataStorageController controller);
    }

    /// <summary>
    /// Handles processing of a query + parameters into something that can be run. Extracts special parameter types 
    /// (CommandBehavior, Buffered, Transaction); turns a query that's a field + a single parameter into a select x=y query;
    /// turns a query that's a single field + a numeric parameter into a primary key selector
    /// </summary>
    public class SqlQueryParser<T> : ISqlQueryParser
    {
        #region constructors


        public SqlQueryParser(QueryType type, object query)
        {
            Initialize(type);

            ProcessQuery(query, null);

        }

        public SqlQueryParser(QueryType type, object query, params object[] parameters)
        {
            Initialize(type);
            ProcessQuery(query, parameters);

        }

        protected void Initialize(QueryType type)
        {
            QueryType = type;
        }
        #endregion

        #region private properties

        protected ISqlQueryMaker _SqlQuery;
        protected QueryType QueryType { get; set; }

        #endregion

        #region public properties

    
        public ISqlQuery Query { get; protected set; }


        #endregion

        #region public methods

        public void ProcessQuery(object query, object[] parameters)
        {

            ParseQuery(query, parameters);
        }

        public void ExpectSelectQuery()
        {
            if (Query.QueryType != QueryType.Select)
            {
                throw new IQException("The query was not a SELECT query.");
            }
        }
        public void ExpectWhereQuery()
        {
            if (Query.QueryType != QueryType.Where)
            {
                throw new IQException("The query was not a WHERE clause.");
            }
        }


        /// <summary>
        /// Throws an exception if this is not ISqlQuery
        /// TODO why do we have this?
        /// </summary>
        /// <returns></returns>
        public ISqlQueryMaker QueryFull
        {
            get
            {
                if (_SqlQuery == null)
                {
                    _SqlQuery = Query as ISqlQueryMaker;
                    if (_SqlQuery == null)
                    {
                        throw new IQException("The query was generated from a raw query, can't be returned as ISqlQuery.");
                    }
                }
                return _SqlQuery;
            }
        }

        #endregion

        #region private methods



        protected ISqlQuery ParseComplexQuery(object query, IEnumerable<object> parms)
        {
            ISqlQuery outputQuery = null;

            // We always want to parse the parameters. But if the thing passed to us as "query" is not a string, then 
            // just assume that all the parms are option type parameters and don't pass a query to ParameterParser
            string querySource = query is string ?
                (string)query :
                "";

            ParameterParser pp = new ParameterParser(querySource, parms);

            if (Types.IsNumericType(query))
            {
                // It's a single numeric value - assume it's a primary key

                ExpectNoParameters(pp.Parameters);

                var classInfo = IQ.ClassInfo<T>();

                ISqlQueryMaker queryPK = classInfo.GetQuery();
                queryPK.Where.Add(classInfo.PrimaryKeyField.Name, query);
                outputQuery = queryPK;
            }
            else if (query is string)
            {
                bool isMappable = Types.IsMappable(typeof(T));

                // First check if its a single named field

                if (isMappable)
                {
                    var classInfo = IQ.ClassInfo<T>();


                    // Try to create a valid raw query.. if it's not valid, assume it's a where
                    if (QueryType == QueryType.Where || pp.QueryType == QueryType.Invalid)
                    {
                        ISqlQueryMaker queryPK = classInfo.GetQuery();

                        //var whereString = new WhereString(pp.Query, 
                        //    pp.Parameters.Count > 0 ?
                        //        pp.Parameters.ToArray():
                        //        null);
                        queryPK.Where.Add(pp.GetWhereClause());
    
                        outputQuery = queryPK;
                    }
                    else
                    {
                        outputQuery = new SqlQueryDef(pp.GetQuery(QueryType), pp.Parameters);
                    }

                }
                else
                {
                    // it's mapped to a primitive type - 
                    outputQuery = new SqlQueryDef(pp.GetQuery(QueryType), pp.Parameters);
                }
            }
            if (outputQuery.QueryType != QueryType)
            {
                throw new IQException("Wrong type of query passed to method: was " + outputQuery.ToString() + ", expected " + QueryType.ToString());
            }

            return outputQuery;
        }


        /// <summary>
        /// When passing type=0, it's never going to try to build a query - only fully formed query SQl will work.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        protected void ParseQuery(object query, IEnumerable<object> parameters)
        {

            ISqlQuery outputQuery = null;

            if (query == null)
            {
                throw new IQMissingQueryException();
            }
            else if (query is SqlQueryMaker)
            {
                ExpectNoParameters(parameters);
                if (QueryType != 0 && ((SqlQueryMaker)query).QueryType != QueryType)
                {
                    throw new IQException("The query passed was not of type '" + QueryType.ToString() + "' as required for this operation.");
                }
                outputQuery = (SqlQueryMaker)query;
            }
            else if (QueryType != 0)
            {
                outputQuery = ParseComplexQuery(query, parameters);
            }
            else if (query is string)
            {
                string queryString = (string)query;

                ParameterParser pp = new ParameterParser(queryString, parameters);

                outputQuery = new SqlQueryDef(queryString, pp.Parameters);
            }
            else
            {
                throw new IQException("The type of object '" + query.GetType().ToString() + "' passed as a query isn't something I know how to handle.");
            }

            if (outputQuery == null)
            {
                throw new IQMissingQueryException();
            }

            if (outputQuery.QueryType == QueryType.Invalid && QueryType != 0)
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
