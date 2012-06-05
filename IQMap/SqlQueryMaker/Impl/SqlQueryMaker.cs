using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMap.Impl;
using IQMap.Impl.Support;
using IQObjectMapper;
using IQMap.SqlQueryBuilder;

namespace IQMap.SqlQueryBuilder.Impl
{
    public class SqlQueryMaker: ISqlQueryMaker
    {
        #region constructors

        public SqlQueryMaker()
        {
            Initialize(QueryType.Select);
        }
        public SqlQueryMaker(QueryType queryType)
        {
            Initialize(queryType);
        }

        protected virtual void Initialize(QueryType queryType)
        {
            Clear();
            ClearFieldMap();
            QueryType = queryType;
        }

        #endregion

        #region private properties
        
       
        protected int top;
        protected string _Select;
        protected string _From;
        protected string groupBy;
        protected string having;

        protected bool _IsDirty = false;

        protected IWhereClause _whereClause = null;
        protected IOrderByClause _orderByClause = null;

        protected SqlFieldMap sqlFieldMap
        {
            get
            {
                return _sqlFieldMap.Value;
            }
        }

        protected Lazy<Dictionary<string, SqlValue>> _updateData =
            new Lazy<Dictionary<string, SqlValue>>();

        protected Dictionary<string, SqlValue> updateData
        {
            get
            {
                return _updateData.Value;
            }
        }

        protected Lazy<SqlFieldMap> _sqlFieldMap = new Lazy<SqlFieldMap>();

        private IParameterCollection _ParameterList;
        protected IParameterCollection ParameterList
        {
            get
            {
                if (_ParameterList == null)
                {
                    _ParameterList = new ParameterCollection();
                }
                return _ParameterList;
            }
            set
            {
                _ParameterList = value;
            }
        }
        protected bool HasParameters
        {
            get
            {
                return Parameters.Any();
            }
        }
        protected bool HasOwnParameters
        {
            get
            {
                return _ParameterList != null && _ParameterList.Count > 0;
            }
        }
        #endregion

        #region public properties

        public int TotalRows { get; set; }
        public int FirstRow { get; set; }
       
        public IEnumerable<IDataParameter> Parameters
        {
            get
            {
                if (HasOwnParameters)
                {
                    foreach (var item in ParameterList)
                    {
                        yield return item;
                    }
                }
                foreach (var item in Where.Parameters)
                {
                    yield return item;
                }
            }
        }
        public IWhereClause Where
        {
            get
            {
                if (_whereClause == null)
                {
                    _whereClause = new WhereClause();
                    _whereClause.Owner = this;
                }
                return _whereClause;
            }
            set
            {
                _whereClause = value;
                _whereClause.Owner = this;
            }
        }

        public bool IsComplete
        {
            get
            {
                return !String.IsNullOrEmpty(From);
            }
        }
        protected string _TableName;
        public string TableName
        {
            get
            {
                return _TableName;
            }
            set
            {
                _TableName = value;
            }
        }
        protected string _PrimaryKey;
        public string PrimaryKey
        {
            get
            {
                return _PrimaryKey;
            }
            set
            {
                _PrimaryKey = value;

            }
        }
        public bool SelectAll
        {
            get
            {
                return _Select == "*";
            }

        }
        public string Select
        {
            get
            {
                return _Select ?? "*";
            }
            set
            {
                _Select = value;
                Touch();
            }
        }
        /// <summary>
        /// Returns From, then TableName, then blank. Not required
        /// </summary>
        public string From
        {
            get
            {
                if (String.IsNullOrEmpty(_From))
                {
                    return _TableName;
                }
                else {
                    return _From;
                }
            }
            set
            {
                _From = value ?? "";
                Touch();
            }
        }


        public string GroupBy
        {
            get
            {
                return groupBy;
            }
            set
            {
                groupBy = value ?? "";
                Touch();
            }
        }
        public string Having
        {
            get
            {
                return having;
            }
            set
            {
                having = value ?? "";
                Touch();
            }
        }
      

        public QueryType QueryType
        {
            get;
            set;
        }
        /// <summary>
        /// This object has changed since it was created or cloned
        /// </summary>
        public virtual bool IsDirty
        {
            get
            {
                return this._IsDirty;
            }
        }

    
        public IEnumerable<KeyValuePair<string, SqlValue>> UpdateData 
        {
            get
            {
                foreach (var kvp in updateData)
                {
                    yield return kvp;
                }
            }
        }
        public string UpdateSet
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (var kvp in updateData)
                {
                    sb.Append((sb.Length == 0 ? "" : ",") + kvp.Key + "=" + kvp.Value.ValueString());
                }
                return sb.ToString();
            }
        }

        public string InsertFields 
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (string key in updateData.Keys)
                {
                    sb.Append((sb.Length == 0 ? "" : ",") + key);
                }
                return "(" + sb.ToString() + ")";
            }
        }
        public string InsertValues 
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (string key in updateData.Keys)
                {
                    sb.Append((sb.Length == 0 ? "" : ",") + updateData[key].ValueString());
                }
                return "(" + sb.ToString() + ")";
            }
        }

        public event EventHandler Dirty;
        
        #endregion

        #region public methods

        public string GetNewParameterName(string basedOn = "")
        {
            return ParameterList.GetNewParameterName(basedOn);
        }
        public void ExpectType(QueryType type)
        {
            if (QueryType != type)
            {
                throw new InvalidOperationException("The query must be of type " + type);
            }
        }
        public void AddParameter(IDataParameter parameter)
        {
            ParameterList.Add(parameter);
        }

        public void AddParameter(IEnumerable<IDataParameter> parameter)
        {
            ParameterList.Add(parameter);
        }

        /// <summary>
        /// Mark the object as dirty
        /// </summary>
        public virtual void Touch()
        {
            this._IsDirty = true;
            if (Dirty != null)
            {
                Dirty(this, new EventArgs());

            }
        }

        /// <summary>
        /// Marks this object as unchanged (IsDirty=false)
        /// </summary>
        public virtual void Clean()
        {
            this._IsDirty = false;
        }

        public IOrderByClause OrderBy
        {
            get
            {
                if (_orderByClause == null)
                {
                    _orderByClause = new OrderByClause();
                    _orderByClause.Owner = this;
                }
                return _orderByClause;

            }
            set
            {
                _orderByClause = value;
                _orderByClause.Owner = this;
            }
        }


        public ISqlQuery AddUpdateData(string fieldName, object value)
        {
            string parmName = GetNewParameterName(fieldName);

            ParameterList.Add(parmName, value);
            updateData[GetFieldMap(fieldName)] = new SqlValueParm(parmName);
            return this;
        }

        /// <summary>
        /// Returns the mapped value for a field alias
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetFieldMap(string name)
        {
            if (_sqlFieldMap.IsValueCreated)
            {
                Touch();
                return sqlFieldMap.SqlFieldName(name.ToLower());    
            }
            else
            {
                return name;
            }
        }

        /// <summary>
        /// Add a range of key/value pairs to map field names to SQL server column names
        /// </summary>
        /// <param name="map"></param>
        public void AddFieldMap(IEnumerable<KeyValuePair<string, string>> map)
        {
            sqlFieldMap.SetFieldMap(map);
            Touch();
        }

        /// <summary>
        /// Map a field name to an alternate representation to be passed to the SQL Server
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public void AddFieldMap(string fieldName, string value)
        {
            sqlFieldMap.AddMapping(fieldName, value);
            Touch();
        }
        /// <summary>
        /// Clear all current aliases in the field map
        /// </summary>
        public void ClearFieldMap()
        {
            if (_sqlFieldMap.IsValueCreated)
            {
                sqlFieldMap.Clear();
            }
        }
       
        public ISqlQueryMaker Clone()
        {
            return CloneImpl();
        }
        public ISqlQueryMaker Clone(QueryType type)
        {
            var clone = CloneImpl();
            clone.QueryType = type;
            return clone;
        }

        /// <summary>
        /// Clears all criteria. This does not clear the field map.
        /// </summary>
        public virtual void Clear()
        {
            QueryType = QueryType.Select;
            Select = null;
            From = "";
            GroupBy = "";
            if (ParameterList != null)
            {
                ParameterList.Clear();
            }

            if (_orderByClause != null)
            {
                OrderBy.Clear();
            }
            if (_whereClause != null)
            {
                Where.Clear();
            }

            Clean();
        }

        public string GetQuery()
        {
            return GetQuery((SqlClause)1023);
        }
        /// <summary>
        /// Return the SQL for this query
        /// </summary>
        /// <returns></returns>
        public string GetQuery(SqlClause clause)
        {
                //if (String.IsNullOrEmpty(PrimaryKey) || String.IsNullOrEmpty(TableName)) {
            //    throw new InvalidOperationException("Missing primary key or table name.");
                //}

                SqlClause required;
                switch (QueryType)
                {
                    case QueryType.Select:
                        // special case for select: nothing is required, so we can extract parts
                         required = SqlClause.Select ;
                         RequireClauses(required);

                         /// <summary>
                         /// This may not actually create a query that is valid SQL in every (or even any) language. The SQL output from the IQuery/
                         /// ISqlQueryMaker object is not intended to be used directly by a SQL engine, rather, it is supposed to generate a unique 
                         /// query for each distict conceptual query, and provide component parts that a DataStorageController can use to map to a 
                         /// particular engine's implementation.
                         /// </summary>
                         /// 
                        return String.Format("{0}{1}{2}{3}{4}{5}{6}",
                            (clause.HasFlag(SqlClause.Select)) ?
                                "SELECT " + Select :
                                "",
                             (clause.HasFlag(SqlClause.From) && !String.IsNullOrEmpty(From)) ?
                                " FROM " + From : "",
                            (clause.HasFlag(SqlClause.Where) && !Where.IsEmpty) ?
                                " WHERE " + Where : "",
                            (clause.HasFlag(SqlClause.GroupBy) && !String.IsNullOrEmpty(GroupBy)) ?
                                " GROUP BY " + GroupBy : "",
                           (clause.HasFlag(SqlClause.Having) && !String.IsNullOrEmpty(Having)) ?
                                " HAVING " + Having : "",
                            (clause.HasFlag(SqlClause.OrderBy) && !OrderBy.IsEmpty) ?
                                " ORDER BY " + OrderBy : "",
                            (clause.HasFlag(SqlClause.Limit) && (TotalRows>0 || FirstRow>0)) ?
                                (" LIMIT " + FirstRow+"," + TotalRows) : ""
                                ).Trim();

                    case QueryType.Delete:
                        required = SqlClause.Table | SqlClause.Where;
                        RequireClauses(required);
                        return String.Format("DELETE FROM {0} WHERE {1}",
                            TableName,
                            Where);
                    case QueryType.Update:
                        required = SqlClause.Table | SqlClause.Where | SqlClause.UpdateSet;
                        RequireClauses(required);
                        // TODO: From queries
                        return String.Format("UPDATE {0} SET {1} WHERE {2}",
                            TableName,
                            UpdateSet,
                            Where);
                    case QueryType.Insert:
                        RequireClauses(SqlClause.Table | SqlClause.InsertFields);
                        return String.Format("INSERT INTO {0} {1} VALUES {2}",
                            TableName,
                            InsertFields,
                            InsertValues);
                    default:
                        throw new InvalidOperationException("Unsupported query type.");
                }
            
            
        }
        protected void RequireClauses(SqlClause clause)
        {
            var err = new Func<string>(() =>
            {
                return " is required for an " + QueryType.ToString() + " query.";
            });

            if (clause.HasFlag(SqlClause.Table) && String.IsNullOrEmpty(TableName))
            {
                throw new IQException("TableName" + err());
            }
            if (clause.HasFlag(SqlClause.Select) && String.IsNullOrEmpty(Select))
            {
                throw new IQException("A SELECT clause" + err());
            }
            if (clause.HasFlag(SqlClause.From) && String.IsNullOrEmpty(From))
            {
                throw new IQException("A FROM clause" + err());
            }
            if (clause.HasFlag(SqlClause.Where) && Where.IsEmpty)
            {
                throw new IQException("A WHERE clause" + err());
            }
            if (clause.HasFlag(SqlClause.UpdateSet) && String.IsNullOrEmpty(UpdateSet))
            {
                throw new IQException("TableName" + err());
            }
            if (clause.HasFlag(SqlClause.InsertFields) && String.IsNullOrEmpty(InsertFields))
            {
                throw new IQException("TableName" + err());
            }
        }



        public string SqlAuditString()
        {
            return SqlQueryUtility.QueryAsSql(GetQuery(), Parameters);
        }

        public override string ToString()
        {
            return GetQuery();
        }

        public override int GetHashCode()
        {
            return GetQuery().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            SqlQueryMaker other = obj as SqlQueryMaker;

            return other != null &&
                other.GetQuery() == GetQuery() &&
                    (HasParameters == other.HasParameters) &&
                    (
                        HasParameters ?
                        _ParameterList == other._ParameterList :
                        true);
        }
        #endregion

        #region private methods

        /// <summary>
        /// Returns a deep copy of this object
        /// </summary>
        /// <returns></returns>
        protected SqlQueryMaker CloneImpl()
        {
            SqlQueryMaker newQuery = new SqlQueryMaker();

            newQuery.QueryType = QueryType;
            newQuery._TableName = _TableName;
            newQuery._PrimaryKey = _PrimaryKey;
            newQuery._Select = _Select;
            newQuery._From = _From;
            newQuery.GroupBy = GroupBy;
            newQuery.Having = Having;
            newQuery.FirstRow = FirstRow;
            newQuery.TotalRows = TotalRows;

            if (_orderByClause != null)
            {
                newQuery.OrderBy = OrderBy.Clone();
            }
            if (_whereClause != null)
            {
                newQuery.Where = Where.Clone();
            }
            if (_sqlFieldMap.IsValueCreated)
            {
                newQuery.AddFieldMap(sqlFieldMap);
            }
            if (HasParameters)
            {
                newQuery.AddParameter(ParameterList.Clone());
            }
            newQuery.Clean();
            return newQuery;

        }

        IEnumerable<IDataParameter> ISqlQuery.Parameters
        {
            get
            {
                return Parameters;
            }
        }

        ISqlQuery ISqlQuery.Clone()
        {
            return Clone();
        }

        #endregion


    }

    
   
}
