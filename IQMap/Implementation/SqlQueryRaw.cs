using System;
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
        public SqlQueryRaw(string sql, params object[] parameters)
        {


            QuerySql = sql.Trim();
            object[] parms = parameters == null ? 
                new object[] { null } :
                parameters.Length == 0 ? null :
                parameters;
            if (parms != null)
            {
                QueryParameters.AddRange(ParseParameters(sql, parms));
            } 

            QueryType = ParseQueryType();

        }


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
        //public string SetQuery(string sql)
        //{
        //    QuerySql = sql;
        //}
        public void AddFieldMap(IEnumerable<KeyValuePair<string, string>> fieldMap)
        {
            
        }

        public string From
        {
            get;
            set;
        }
        public QueryType QueryType
        {
            get;
            protected set;
        }

        protected IEnumerable<IDataParameter> ParseParameters(string sql, params object[] parameters)
        {
            if (parameters.Length==0) {
                yield break;
            }

            bool isNamed=false;
            if ((parameters.Length % 2) == 0)
            {
                isNamed = true;
                for (int i = 0; i < parameters.Length; i+=2)
                {
                    if (parameters[i].GetType() == typeof(string))
                    {
                        string parmName = (string)parameters[i];
                        if (sql.IndexOf(parmName) <= 0)
                        {
                            isNamed = false;
                        }
                    }
                }
            }

            if (isNamed)
            {

                for (int i = 0; i < parameters.Length; i += 2)
                {
                    IDataParameter parm = new QueryParameter((string)parameters[i], parameters[i+1]);
                    yield return parm;
                }
            }
            else
            {
                // now see if the parameter could be an object
                
                List<string> parmNames = new List<string>(Utils.GetParameterNames(sql).Select(item=>item.Item1));
                
                if (parameters.Length == 1 && !IsParameterType(parameters[0])) {
                    foreach (var kvp in GetProperties(parameters[0]))
                    {
                        IDataParameter parm = new QueryParameter("@" + kvp.Key, kvp.Value);
                        yield return parm;
                    }
                } else if (parameters.Length==1 && parmNames.Count==0) {
                    // One parameter, none in query - make it an equals                
                        QuerySql = QuerySql + "=@1";
                        IDataParameter parm = new QueryParameter("@1", parameters[0]);
                        yield return parm;
                    
                }
                else if (parameters.Length == parmNames.Count)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        IDataParameter parm = new QueryParameter(parmNames[i], parameters[i]);
                        yield return parm;
                    }
                }
                else
                {
                    throw new Exception("Parameters must match the query parameter names or count.");
                }
            }
            yield break;
        }
        /// <summary>
        /// Enumerate props/values for an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private IEnumerable<KeyValuePair<string,object>> GetProperties(object obj)
        {
            Type type = obj.GetType();
            IEnumerable<MemberInfo> members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            foreach (var member in members)
            {
                if (member is PropertyInfo)
                {
                    PropertyInfo info = (PropertyInfo)member;
                    yield return new KeyValuePair<string, object>(info.Name, info.GetValue(obj,null));
                }
            }
        }
        /// <summary>
        /// Check the parameter to see if it's legitimate paramter values
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected bool IsParameterType(Object obj)
        {
            if (obj == null)
            {
                return true;
            }
            else
            {
                Type t = Utils.GetUnderlyingType(obj.GetType());
                return t.IsPrimitive || t == typeof(string) || t == typeof(byte[]) || t == typeof(DateTime) || t.IsEnum;
            }
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

    }
}