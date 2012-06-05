using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Dynamic;
using IQObjectMapper;
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;

namespace IQMap.Impl.Support
{
    /// <summary>
    /// Handles processing of a query + parameters into something that can be run. Extracts special parameter types 
    /// (CommandBehavior, Buffered, Transaction); turns a query that's a field + a single parameter into a select x=y query;
    /// turns a query that's a single field + a numeric parameter into a primary key selector
    /// </summary>
    public class ParameterParser 
    {
        #region constructors

        
        public ParameterParser(string query, params object[] parameters)
        {
            Query = query != null ?
               query.Trim() :
               null;
            ProcessParameters(parameters);
            ParseQueryType();
        }

        #endregion

        #region private properties

        protected List<IDataParameter> _Parameters;
        protected IWhere WhereClause;

        #endregion

        #region public properties
        public QueryType QueryType { get; protected set; }
        
        
        public bool IsStoredProcedure { get; set; }

        protected string Query { get; set; }


        public IList<IDataParameter> Parameters
        {
            get
            {
                return new ReadOnlyCollection<IDataParameter>(_Parameters);
            }
        }
        public IList<IDataParameter> ParametersIgnoreNull
        {
            get
            {
                return new ReadOnlyCollection<IDataParameter>(_Parameters.Where(item => item != null).ToList());
            }
        }
        
      
        #endregion

        #region public methods
        public IWhere GetWhereClause()
        {
            // In some situations, the query has already been built.
            if (WhereClause == null)
            {
                return QueryBuilder.WhereParse(Query, Parameters);
            }
            else
            {
                return WhereClause;
            }
        }
        /// <summary>
        /// Return the query string. The "type" parameter ensures that you know what you are doing;
        /// an error will result if you request the wrong type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetQuery(QueryType type)
        {
            if (type!=QueryType)
            {
                throw new InvalidOperationException(String.Format("This query is a {0} query, you requested a {1} query.",QueryType.ToString(),type.ToString()));
            }
            return GetQuery();
        }
        public string GetQuery()
        {
            return Query;
        }
        #endregion

        #region private methods
        protected enum ParameterMode
        {
            Mapped=1,
            KVPs=2
        }

        protected void ParseQueryType()
        {
            
            string clean = Query.ToLower();
            if (clean.Length < 6)
            {
                QueryType = QueryType.Invalid;
            }
            else
            { 
                switch (clean.Substring(0, 6))
                {
                    case "select": QueryType = QueryType.Select; break;
                    case "update": QueryType = QueryType.Update; break;
                    case "delete": QueryType = QueryType.Delete; break;
                    case "insert": QueryType = QueryType.Insert; break;
                    default: QueryType = QueryType.Invalid; break;
                }
            }
        }

        protected void ProcessParameters(IEnumerable<object> parameters)
        {
            List<string> parmNames = new List<string>(SqlQueryUtility.GetParameterNames(Query));
            var parms = new List<object>(UnwrapParameters(parameters));

            _Parameters = new List<IDataParameter>();
            var interim = new List<object>();

            if (parms.Count == 0)
            {
                return;
            }

            ParameterMode mode = 0;
            int index;

            for (index = 0; index < parms.Count; index++)
            {
                object cur = parms[index];
             
                if (cur is IQSqlDirectiveType)
                {
                    IsStoredProcedure = (IQSqlDirectiveType)cur == IQSqlDirectiveType.StoredProcedure ? true : false;
                }
                
                else if (cur is IDataParameter)
                {
                    if (mode != 0 && mode != ParameterMode.KVPs)
                    {
                        throw new ArgumentException("An IDataParameter was found after a value-typed parameter. You can't mix parameter types.");
                    }
                    _Parameters.Add((IDataParameter)cur);
                }
                else if (!IsParameterType(cur))
                {
                    // An object; treat it's properties as key/value pairs
                    if (mode != 0 && mode != ParameterMode.KVPs)
                    {
                        throw new ArgumentException("An object parameter was found after a value-typed parameter. To map object properties " +
                            "as parameters, only objects can be passed.");
                    }
                    mode = ParameterMode.KVPs;
                    foreach (var kvp in GetProperties(cur))
                    {
                        int parmIndex = parmNames.IndexOf(CleanParmName(kvp.Key), IQ.Config.ParmNameComparer);
                        if (parmIndex >= 0)
                        {
                            //Use name from the list of parms to ensure proper case
                            _Parameters.Add(CreateParameter(parmNames[parmIndex], kvp.Value));
                        }
                    }
                }
                else
                {

                    if (mode != 0 && mode != ParameterMode.Mapped)
                    {
                        throw new ArgumentException("Can't mix parameter types.");
                    }

                    mode = ParameterMode.Mapped;
                    interim.Add(cur);
                }
            }

            // now address interim parameters, if any

            if (interim.Count > 0)
            {

                bool isNamed = false;
                if ((interim.Count % 2) == 0)
                {
                    isNamed = true;
                    List<IDataParameter> tempList = new List<IDataParameter>();
                    for (int i = 0; i < interim.Count; i += 2)
                    {
                        var val = interim[i];
                        if (val!=null && interim[i].GetType() == typeof(string))
                        {
                            string parmName = (string)interim[i];

                            // Don't check parm name list for stored procedures
                            if (!IsStoredProcedure)
                            {
                                int parmIndex = parmNames.IndexOf(CleanParmName(parmName), IQ.Config.ParmNameComparer);
                                if (parmIndex >= 0)
                                {
                                    tempList.Add(CreateParameter(parmNames[parmIndex], interim[i + 1]));
                                }
                                else
                                {
                                    isNamed = false;
                                    break;
                                }
                            }
                            else
                            {
                                tempList.Add(CreateParameter((string)interim[i], interim[i + 1]));
                            }
                        }
                        else
                        {
                            isNamed = false;
                            break;
                        }
                    }
                    if (isNamed)
                    {
                        _Parameters.AddRange(tempList);
                        return;

                    }
                }

                // Final type of query to parse: a field name passed with a single parameter. If there are no spaces,
                // no parameters in the query, and one parm passed, make it an "equals"

                else if (!IsStoredProcedure && interim.Count == 1 
                    && parmNames.Count == 0
                    && Query.IndexOfAny(new char[] {' ','='})<=0)
                {

                    WhereClause = QueryBuilder.WhereEquals(Query, interim[0]);
                    //Query = Query + "=@val";
                    //_Parameters.Add(CreateParameter("@val", interim[0]));
                    return;
                }

                if (!IsStoredProcedure)
                {
                    // Final validation: if #s match, make a query
                    if (interim.Count == parmNames.Count)
                    {
                        for (int i = 0; i < interim.Count; i++)
                        {
                            _Parameters.Add(CreateParameter(parmNames[i], interim[i]));
                        }
                        return;
                    }
                    else
                    {
                        string names = String.Join(",", parmNames);
                        string values = String.Join(",", interim);
                        throw new ArgumentException(String.Format("Parameters must match the query parameter names or count. Names: [{0}] Values: [{1}]", names, values));
                    }
                }
            } else {
                if (_Parameters.Count != parmNames.Count) {
                    string names = String.Join(",", parmNames);
                    string values = String.Join(",", _Parameters.Select(item=>item.ParameterName));
                    throw new ArgumentException(String.Format("Parameters must match the query parameter names or count. Names: [{0}] Values: [{1}]", names, values));

                }
            }

        }
        private IDataParameter CreateParameter(string name, object value)
        {
            return new QueryParameter(name, value);

        }
        /// <summary>
        /// Enumerate props/values for an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected IEnumerable<KeyValuePair<string, object>> GetProperties(object obj)
        {

            Type type = obj.GetType();
            bool isAnon = Types.IsAnonymousType(obj.GetType());
            if (obj is IDictionary<string, object>)
            {
                foreach (var item in (IDictionary<string, object>)obj)
                {
                    yield return item;
                }
            } else {
                IEnumerable<MemberInfo> members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var member in members)
                {
                    if (member is MethodInfo && isAnon)
                    {
                        MethodInfo info = (MethodInfo)member;
                        if (info.Name.Length >= 4 && info.Name.StartsWith("get_"))
                        {
                            yield return new KeyValuePair<string, object>(info.Name, info.Invoke(obj, null));
                        }
                    }

                    else
                    {

                        if (member is PropertyInfo)
                        {
                            PropertyInfo info = (PropertyInfo)member;
                            yield return new KeyValuePair<string, object>(info.Name, info.GetValue(obj, null));
                        }
                    }
                }
                
            }
        }
        /// <summary>
        /// Expands any arrays in a list
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected IEnumerable<object> UnwrapParameters(IEnumerable<object> parameters)
        {
            if (parameters == null)
            {
                yield break;
            }
            // Unwrap any inner lists because we can combine new parameters with the param array list. 
            foreach (var parm in parameters)
            {
             
                if (parm is IEnumerable<object>)
                {
                    foreach (var obj in UnwrapParameters((IEnumerable<object>)parm)) {
                        yield return obj;
                    }
                }
                else
                {
                    yield return parm;
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
                Type t = Types.GetUnderlyingType(obj.GetType());
                return t.IsPrimitive || t == typeof(string) || t == typeof(byte[])
                    || t == typeof(DateTime) || t.IsEnum || t == typeof(DBNull);
            }
        }
        protected string CleanParmName(string text) {
            if (!string.IsNullOrEmpty(text) && text.Length > 0 && text[0]!='@')
            {
                return "@" + text;
            }
            else
            {
                return text;
            }
        }
        #endregion



    }
}
