using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Data;
using System.Reflection;
using IQMap.Impl.Support;

namespace IQMap.SqlQueryBuilder.Impl
{
    /// <summary>
    ///  A pass-through implementation of IQuery
    /// </summary>
    public class SqlQueryDef : ISqlQuery
    {
        #region constructor
        public SqlQueryDef()
        {
        }
        public SqlQueryDef(string sql)
        {
            QuerySql = sql.Trim();
        }
        public SqlQueryDef(ISqlQuery query )
        {
            QuerySql = query.GetQuery();
            QueryParameters.AddRange(query.Parameters);
        }
        public SqlQueryDef(string sql, IEnumerable<object> parameters)
        {
            IEnumerable<IDataParameter> parms;
            if (parameters is IEnumerable<IDataParameter>)
            {
                parms = (IEnumerable<IDataParameter>)parameters;
                QuerySql = sql;
            }
            else
            {
                ParameterParser pp = new ParameterParser(sql, parameters);
                QuerySql = pp.GetQuery();
                parms = pp.Parameters;
                
            }


            QueryParameters.AddRange(parms);

        }

        #endregion

        protected string QuerySql;


        private List<IDataParameter> _QueryParameters;
        protected List<IDataParameter> QueryParameters
        {
            get
            {
                if (_QueryParameters == null)
                {
                    _QueryParameters = new List<IDataParameter>();
                }
                return _QueryParameters;
            }

        }

        protected IEnumerable<IDataParameter> _Parameters;
        public IEnumerable<IDataParameter> Parameters
        {
            get
            {
                if (_Parameters == null)
                {
                    _Parameters = new ReadOnlyCollection<IDataParameter>(QueryParameters);
                }
                return _Parameters;
            }
            set
            {
                _Parameters = value;
            }
        }
        public string GetQuery()
        {
            return GetQuery((SqlClause)511);
        }
        public string GetQuery(SqlClause clause)
        {
            return QuerySql;
        }


        public bool IsComplete
        {
            get
            {
                return true;
            }

        }
        public QueryType QueryType
        {
            get
            {
                return ParseQueryType();
            }
        }
        
        public ISqlQuery Clone()
        {
            SqlQueryDef query = new SqlQueryDef(QuerySql, Parameters);
            return query;

        }

        public void ExpectType(QueryType type)
        {
            if (type != ParseQueryType())
            {
                throw new InvalidOperationException("The query must be of type " + type.ToString());
            }
        }


        protected QueryType ParseQueryType()
        {
            string clean = QuerySql.ToLower();
            if (clean.Length < 6)
            {
                throw new InvalidOperationException("Invalid query.");
            }


            switch (clean.Substring(0, 6))
            {
                case "select": return QueryType.Select;
                case "update": return QueryType.Update;
                case "delete": return QueryType.Delete;
                case "insert": return QueryType.Insert;
                default: return QueryType.Invalid;
            }

        }

        public string Select { get { throw Fail(); } set { throw Fail(); } }
        public string Having { get { throw Fail(); } set { throw Fail(); } }
        public string From { get { throw Fail(); } set { throw Fail(); } }
        public string Where { get { throw Fail(); } set { throw Fail(); } }
        public string OrderBy { get { throw Fail(); } set { throw Fail(); } }
        public string GroupBy { get { throw Fail(); } set { throw Fail(); } }
        public string TableName { get { throw Fail(); } set { throw Fail(); } }
        public string PrimaryKey { get { throw Fail(); } set { throw Fail(); } }
        public int TotalRows { get { return 0; } set { throw Fail(); } }
        public int FirstRow { get { return 0; } set { throw Fail(); } }
        public string InsertFields { get { throw Fail(); } set { throw Fail(); } }
        public string InsertValues { get { throw Fail(); } set { throw Fail(); } }
        public string UpdateSet { get { throw Fail(); } set { throw Fail(); } }

        public override string ToString()
        {
            return GetQuery() ;
        }
        protected Exception Fail()
        {
            return new NotImplementedException("This type of query cannot be modified inline. It "
                +"is only valid for one-offs. Someday we would like to implement a query parser so "
                +"arbitrary queries can be used with the LINQ expressions.");

        }

    }
}