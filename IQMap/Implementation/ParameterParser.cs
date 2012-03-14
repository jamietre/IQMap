using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;

namespace IQMap.Implementation
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
            Initialize();
            Query = query.Trim();
            ProcessParameters(parameters);
            ParseQueryType();
        }

        protected void Initialize()
        {
            Buffered = true;
            CommandBehavior = 0;
        }
        #endregion

        #region public properties
        public QueryType QueryType { get; protected set; }
        protected List<IDataParameter> _Parameters;
        protected IDbConnection _Connection;
        protected IDbTransaction _Transaction;
        protected CommandBehavior _CommandBehavior;
        public bool IsStoredProcedure { get; set; }
        public CommandBehavior CommandBehavior
        {
            get
            {
                if (_CommandBehavior == 0)
                {
                    return _Transaction != null ?
                        CommandBehavior.Default :
                        CommandBehavior.CloseConnection;
                }
                else
                {
                    return _CommandBehavior;
                }
            }
            protected set
            {
                _CommandBehavior = value;
            }
        }

        public string Query { get; protected set; }
        public IDbConnection Connection
        {
            get
            {
                if (_Connection == null)
                {
                    _Connection =
                    (_Transaction != null ?
                        _Transaction.Connection :
                        IQ.Connection);
                }
                return _Connection;
            }
            protected set
            {
                _Connection = value;
            }
        }

        public IDbTransaction Transaction { 
            get 
            {
                return _Transaction;
            }
            protected set 
            {
                _Transaction = value;
            } 
        }
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
        public bool Buffered { get; protected set; }
        
        #endregion

        #region public methods
        public IQuery GetIQuery()
        {
            SqlQueryRaw query = new SqlQueryRaw(Query, Parameters);
            return query;
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

        /// <summary>
        /// Process the parameters array, extracting a transaction from the first element if present. Otherwise,
        /// ensure the result is non-null IEnumerable
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected void ProcessParameters(IEnumerable<object> parameters)
        {
            List<string> parmNames = new List<string>(Utils.GetParameterNames(Query));
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
                if (cur is IDbTransaction)
                {
                    Transaction = (IDbTransaction)cur;
                }
                else if (cur is IQBuffered)
                {
                    Buffered = (IQBuffered)cur == IQBuffered.Buffered ? true : false;
                }
                else if (cur is IQSqlDirectiveType)
                {
                    IsStoredProcedure = (IQSqlDirectiveType)cur == IQSqlDirectiveType.StoredProcedure ? true : false;
                }
                else if (cur is CommandBehavior)
                {
                    CommandBehavior = (CommandBehavior)cur;
                }
                else if (cur is IDbConnection)
                {
                    Connection = (IDbConnection)cur;
                }
                else if (cur is IDataParameter) {
                    if (mode != 0 && mode != ParameterMode.KVPs)
                    {
                        throw new Exception("Can't mix parameter types.");
                    }
                    _Parameters.Add((IDataParameter)cur);
                }
                else if (!IsParameterType(cur))
                {
                    if (mode!=0 && mode!=ParameterMode.KVPs) {
                        throw new Exception("Can't mix parameter types.");
                    }
                    mode = ParameterMode.KVPs;
                    foreach (var kvp in GetProperties(cur))
                    {
                        int parmIndex = parmNames.IndexOf("@" + kvp.Key);
                        if (parmIndex>=0)
                        {
                            //Use name from the list of parms to ensure proper case
                            _Parameters.Add(new QueryParameter(parmNames[parmIndex], kvp.Value));
                        }
                    }
                }
                else
                {
                    // Skip null values when they are before any other parameters

                    if (cur == null && mode != ParameterMode.Mapped)
                    {
                        continue;
                    }
                    
                    if (mode != 0 && mode != ParameterMode.Mapped)
                    {
                        throw new Exception("Can't mix parameter types.");
                    }

                    mode = ParameterMode.Mapped;
                    interim.Add(cur);
                }
            }

            // now address interim parameters, if any

            if (mode == ParameterMode.KVPs || interim.Count==0)
            {
                return;
            }
            
            bool isNamed = false;
            if ((interim.Count % 2) == 0)
            {
                isNamed = true;
                for (int i = 0; i < interim.Count; i += 2)
                {
                    if (interim[i].GetType() == typeof(string))
                    {
                        string parmName = (string)interim[i];
                        
                        // Don't check parm name list for stored procedures
                        if (!IsStoredProcedure && !parmNames.Contains(parmName))
                        {
                            isNamed = false;
                        }
                    }
                    else
                    {
                        isNamed = false;
                    }
                }
            }

            if (isNamed)
            {

                for (int i = 0; i < interim.Count; i += 2)
                {
                    string name = (string)interim[i];
                    object value = interim[i + 1];

                    if (!IsStoredProcedure)
                    {
                        int parmIndex = parmNames.IndexOf(name);
                        if (parmIndex >= 0)
                        {
                            _Parameters.Add(new QueryParameter(parmNames[parmIndex], value));
                        }
                    }
                    else
                    {
                        _Parameters.Add(new QueryParameter(name, value));
                    }
                }
                return;
            }

            // now see if there's just one parameter & none named in the query: it's a field query

            else if (!IsStoredProcedure && interim.Count == 1 && parmNames.Count == 0)
            {
                // One parameter, none in query - make it an equals                
                Query=Query+ "=@val";
                _Parameters.Add(new QueryParameter("@val", interim[0]));
                return;
            }
           
                       
            // eliminate any null values if we've fallen through this far, and the # of parms does not match
            // This could break certain queries, but if you are both passing null non-query parameters, and expect to be able
            // to pass null values to the query, then we should force it to break rather than guess if it's valid code.
            if (!IsStoredProcedure)
            {
                if (interim.Count > parmNames.Count)
                {
                    int i = 0;
                    while (i < interim.Count && interim[i] == null)
                    {
                        interim.RemoveAt(i++);
                    }
                }

                // Final validation: if #s match, make a query
                if (interim.Count == parmNames.Count)
                {
                    for (int i = 0; i < interim.Count; i++)
                    {
                        _Parameters.Add(new QueryParameter(parmNames[i], interim[i]));
                    }
                    return;
                }
                else
                {
                    string names = String.Join(",", parmNames);
                    string values = String.Join(",", interim);
                    throw new Exception(String.Format("Parameters must match the query parameter names or count. Names: [{0}] Values: [{1}]", names, values));
                }
            }
            else
            {
                throw new Exception("Invalid parameter types were passed to a stored procedure. They must be named.");
            }
        }
        /// <summary>
        /// Enumerate props/values for an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected IEnumerable<KeyValuePair<string, object>> GetProperties(object obj)
        {
            Type type = obj.GetType();
            IEnumerable<MemberInfo> members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            foreach (var member in members)
            {
                if (member is PropertyInfo)
                {
                    PropertyInfo info = (PropertyInfo)member;
                    yield return new KeyValuePair<string, object>(info.Name, info.GetValue(obj, null));
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
                Type t = Utils.GetUnderlyingType(obj.GetType());
                return t.IsPrimitive || t == typeof(string) || t == typeof(byte[]) || t == typeof(DateTime) || t.IsEnum;
            }
        }
       
        #endregion
    }
}
