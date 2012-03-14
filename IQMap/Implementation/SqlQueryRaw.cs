using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Data;
using System.Reflection;

namespace IQMap.Implementation
{
    /// <summary>
    ///  A pass-through implementation of IQuery
    /// </summary>
    public class SqlQueryRaw: IQuery
    {
        #region constructor

        public SqlQueryRaw(string sql)
        {
            QuerySql = sql.Trim();
            QueryType = ParseQueryType();
        }
        public SqlQueryRaw(string sql, IEnumerable<object> parameters)
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
                parms = pp.Parameters;
                QuerySql = pp.Query;
            }


            QueryParameters.AddRange(parms);

            QueryType = ParseQueryType();

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
        public IEnumerable<IDataParameter> Parameters {
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
            return QuerySql;
        }

        public void AddFieldMap(IEnumerable<KeyValuePair<string, string>> fieldMap)
        {
            throw new NotImplementedException();
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
            get;
            protected set;
        }
        public IQuery Clone()
        {
            SqlQueryRaw query = new SqlQueryRaw(QuerySql, Parameters);
            return query;

        }

         
        public IQuery Clone(QueryType type)
        {
            throw new Exception("You can't change the type of an SqlQueryRaw object by cloning it..");
        }
        

           
        

        protected QueryType ParseQueryType()
        {
            string clean = QuerySql.ToLower();
            if (clean.Length < 6) {
                throw new Exception("Invalid query.");
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



        public string Select
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Having
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public string From
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Where
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string OrderBy
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string GroupBy
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public ISqlQuery AddWhere(string condition)
        {
            throw new NotImplementedException();
        }

        public ISqlQuery AddParameter(IDataParameter parameter)
        {
            throw new NotImplementedException();
        }
    }
}