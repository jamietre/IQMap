using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.Implementation
{
    public class SqlQuery: ISqlQuery
    {
        #region constructors

        public SqlQuery()
        {
            Initialize(QueryType.Select);
        }
        public SqlQuery(QueryType queryType)
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
        
        protected int parameterCount = 0;
        protected string select;
        protected string from;
        protected string groupBy;
        protected string having;
        protected string orderBy;
        protected bool _IsDirty = false;

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
        protected List<SortCriterion> sortCriterionList
        {
            get
            {
                return _sortCriterionList.Value;
            }
        }
        protected Lazy<List<SortCriterion>> _sortCriterionList = new Lazy<List<SortCriterion>>();


        protected IList<IDataParameter> parameterList
        {
            get
            {
                if (_parameterList == null)
                {
                    _parameterList = new List<IDataParameter>();
                    _parameterNameList = new HashSet<string>();
                }
                // Only allow adding from methods
                return new ReadOnlyCollection<IDataParameter>(_parameterList);
            }
            set
            {
                if (value != null)
                {
                    throw new Exception("Can only set parameterList to null");
                }
                _parameterList = null;
                _parameterNameList = null;
            }
        }
        protected bool ParameterExists(string parameterName)
        {
            if (_parameterNameList == null)
            {
                return false;
            }
            else
            {
                string check = (parameterName.Substring(0, 1) == "@" ? "" : "@") + parameterName;
                return _parameterNameList.Contains(check);
            }

        }

        private HashSet<string> _parameterNameList;
        private List<IDataParameter> _parameterList;

        protected IWhere whereClause
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
        protected IWhere _whereClause = null;

        #endregion

        #region public properties
        public bool IsComplete
        {
            get
            {
                return !String.IsNullOrEmpty(From);
            }
        }
        public string TableName { get; set; }

        public string Select
        {
            get
            {
                return select;
            }
            set
            {
                select = value ?? "";
                Touch();
            }
        }
        public string From
        {
            get
            {
                if (String.IsNullOrEmpty(from))
                {
                    return TableName;
                }
                else {
                    return from;
                }
            }
            set
            {
                from = value ?? "";
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
        public string Where
        {
            get
            {
                return whereClause.GetSql();
            }
            set
            {
                whereClause = new WhereString(value);
                Touch();
            }
        }

        public string OrderBy
        {
            get
            {
                List<SortCriterion> finalList = new List<SortCriterion>(sortCriterionList.Where(item => item.Priority == SortPriority.Required
                    || item.Priority == SortPriority.RequiredOrder));
                finalList.AddRange(sortCriterionList.Where(item => item.Priority == SortPriority.Normal));
                finalList.AddRange(sortCriterionList.Where(item => item.Priority == SortPriority.Default));

                return String.Join(",", finalList.Select(item => item.GetSql()));
            }
            set
            {
                sortCriterionList.Clear();
                if (value != "")
                {
                    AddSort(value, SortPriority.Normal);
                }
                Touch();
            }
        }
        /// <summary>
        /// Sort clauses that appear before any other and cannot be removed
        /// </summary>
        public string OrderByRequired
        {
            get
            {
                return String.Join(",", sortCriterionList
                    .Where(item => item.Priority == SortPriority.Required || item.Priority == SortPriority.RequiredOrder)
                    .Select(item => item.GetSql()));
            }
            set
            {
                AddSort(value, SortPriority.Required);
            }
        }
        public QueryType QueryType
        {
            get;
            protected set;
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



        /// <summary>
        /// A list of parameters associated with this object
        /// </summary>
        public IEnumerable<IDataParameter> Parameters
        {
            get
            {
                if (parameterList != null)
                {
                    foreach (var parm in parameterList)
                    {
                        yield return parm;
                    }
                }
                else
                {
                    yield break;
                }
            }
            set
            {
                if (value != null)
                {
                    foreach (var parm in value)
                    {
                        AddParameter(parm);
                    }
                }
                else
                {
                    parameterList = null;
                }
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

        public bool OptimizeParameterNames { get; set; }
        public event EventHandler Dirty;
        
        #endregion

        #region public methods
        /// <summary>
        /// Marks this object as unchanged (IsDirty=false)
        /// </summary>
        public virtual void Clean()
        {
            this._IsDirty = false;
        }
        public string SqlAuditString()
        {
            return Utils.QueryAsSql(GetQuery(), Parameters);
        }

        public ISqlQuery AddUpdateData(string fieldName, object value)
        {
            //SqlValue sqlValue = new SqlValue(value);
            //updateData[MapField(fieldName)] = sqlValue;
            //return this;
            string parmName = GetParameterName(OptimizeParameterNames ? "" : fieldName);

            AddParameter("@" + parmName, value);
            updateData[MapField(fieldName)] = new SqlValueParm("@" + parmName);
            return this;
        }

        /// <summary>
        /// Returns the mapped value for a field alias
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string MapField(string name)
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
        /// Add a range of key/value pairs to the field map
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
        /// <summary>
        /// Returns a deep copy of this object
        /// </summary>
        /// <returns></returns>
        public ISqlQuery Clone(QueryType type)
        {
            SqlQuery newQuery = new SqlQuery();

            newQuery.QueryType = type;
            newQuery.TableName = TableName;
            newQuery.Select = Select;
            newQuery.From = From;
            newQuery.GroupBy = GroupBy;
            newQuery.Having = Having;
            newQuery.OptimizeParameterNames = OptimizeParameterNames;

            if (_sortCriterionList.IsValueCreated)
            {
                foreach (SortCriterion item in sortCriterionList)
                {
                    var clone = item.Clone();
                    clone.Owner = newQuery;
                    newQuery.sortCriterionList.Add(clone);
                }
            }
            if (_whereClause != null)
            {
                newQuery.whereClause = whereClause.Clone();
            }
            if (_sqlFieldMap.IsValueCreated)
            {
                newQuery.AddFieldMap(sqlFieldMap);
            }
            if (parameterList != null)
            {
                foreach (IDataParameter parm in Parameters)
                {
                    newQuery.AddParameter(parm);
                }
            }
            newQuery.Clean();
            return newQuery;

        }

        public ISqlQuery Clone()
        {
            return Clone(QueryType);
        }
        /// <summary>
        /// Return the SQL for this query
        /// </summary>
        /// <returns></returns>
        public string GetQuery()
        {
            switch (QueryType)
            {
                case QueryType.Select:
                    RequireClauses(select: true, from: true);
                    return String.Format("SELECT {0} FROM {1}{2}{3}{4}",
                        Select,
                        From,
                        Where != "" ? " WHERE " + Where : "",
                        GroupBy != "" ? " GROUP BY " + GroupBy : "",
                        OrderBy != "" ? " ORDER BY " + OrderBy : "");
                case QueryType.Delete:
                    RequireClauses(tableName: true, where: true);
                    return String.Format("DELETE FROM {0} WHERE {1}",
                        TableName,
                        Where);
                case QueryType.Update:
                    RequireClauses(tableName: true, where: true, set:true);
                    // TODO: From queries
                    return String.Format("UPDATE {0} SET {1} WHERE {2}",
                        TableName,
                        UpdateSet,
                        Where);
                case QueryType.Insert:
                    RequireClauses(tableName: true,insertFields: true);
                    return String.Format("INSERT INTO {0} {1} VALUES {2}",
                        TableName,
                        InsertFields,
                        InsertValues);
                default:
                    throw new Exception("Unsupported query type.");
            }

            
        }
        protected void RequireClauses(bool tableName = false, bool select = false, bool from = false, bool where = false, 
            bool set=false, bool insertFields= false)
        {
            var err = new Func<string>(() =>
            {
                return " is required for an " + QueryType.ToString() + " query.";
            });

            if (tableName && String.IsNullOrEmpty(TableName))
            {
                throw new IQException("TableName" + err());
            }
            if (select && String.IsNullOrEmpty(Select))
            {
                throw new IQException("A SELECT clause" + err());
            }
            if (from && String.IsNullOrEmpty(From))
            {
                throw new IQException("A FROM clause" + err());
            }
            if (where && String.IsNullOrEmpty(Where))
            {
                throw new IQException("A WHERE clause" + err());
            }
            if (set && String.IsNullOrEmpty(UpdateSet))
            {
                throw new IQException("TableName" + err());
            }
            if (insertFields && String.IsNullOrEmpty(InsertFields))
            {
                throw new IQException("TableName" + err());
            }
        }
        /// <summary>
        /// Add an ordering condition to this query. If it's already present, it will be replaced (superceded) by the 
        /// new one. This means exsting orders will appear at the end of the list instead of their original position.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public ISqlQuery AddSort(string field, SortOrder order)
        {
            return AddSort(field, order, SortPriority.Normal);
        }
        /// <summary>
        /// Add sort clauses in regular SQL syntax
        /// </summary>
        /// <param name="orderClause"></param>
        /// <returns></returns>
        public ISqlQuery AddSort(string orderClause)
        {
            return AddSort(orderClause, SortPriority.Normal);
        }
        public ISqlQuery AddSort(string orderClause, SortPriority priority)
        {
            string[] items = orderClause.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length == 0)
            {
                throw new Exception("Cannot add a missing field for sorting.");
            }
            foreach (var item in items)
            {
                string clause = item.Trim();
                SortOrder order = SortOrder.Ascending;
                if (clause.IndexOf(' ') > 0)
                {
                    string[] parts = clause.Split(' ');
                    clause = parts[0];
                    if (parts[1].ToLower().Trim() == "desc")
                    {
                        order = SortOrder.Descending;
                    }
                }
                AddSort(clause, order, priority);
            }
            return this;
        }
        /// <summary>
        /// Adds an item to the sorting list. If it exists already, it will be superceded (deprioritized), unless the existing
        /// item is marked as "Required" or "RequiredOrder." In the first case, only the order (asc or desc) will be changed to
        /// the current value. In the 2nd case, no changes will be made.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="order"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public ISqlQuery AddSort(string field, SortOrder order, SortPriority priority)
        {
            if (String.IsNullOrEmpty(field))
            {
                throw new Exception("The field name cannot be missing to add a sort");
            }
            string sortOrder = order == SortOrder.Ascending ? "" : " DESC";
            int index = sortCriterionList.Count - 1;
            string fieldNameClean = unAliased(field.Trim().ToLower());

            bool found = false;
            while (index >= 0)
            {
                var item = sortCriterionList[index];
                if (item.Field == fieldNameClean)
                {
                    switch (item.Priority)
                    {
                        case SortPriority.RequiredOrder:
                            found = true;
                            index = -1;
                            break;
                        case SortPriority.Required:
                            sortCriterionList[index] = new SortCriterion(fieldNameClean, order, item.Priority);
                            sortCriterionList[index].Owner = this;
                            index = -1;
                            found = true;
                            break;
                        default:
                            sortCriterionList.RemoveAt(index);
                            break;
                    }
                }

                index--;
            }
            if (!found)
            {
                var crit = new SortCriterion(fieldNameClean, order, priority);
                crit.Owner = this;
                sortCriterionList.Add(crit);
                Touch();
            }

            return this;
        }
        /// <summary>
        /// Adds an SQL parameter to the query
        /// </summary>
        /// <param name="parm"></param>
        public ISqlQuery AddParameter(IDataParameter parm)
        {
            
            if ( ParameterExists(parm.ParameterName) )
            {
                IDataParameter existing = parameterList.FirstOrDefault(item => item.ParameterName == parm.ParameterName);
                if (existing.Value != parm.Value)
                {
                    throw new Exception("A parameter named \"" + parm.ParameterName + "\" with a different value has already been added to this query.");
                }
            }
            else
            {
                // touch it - this simply forces list to be created. We want creation code in the property so that methods can freely
                // check its members, but we also want it to be a read-only list so you must add using the innner property. this
                // makse sure this code block is the only place that can add to it (since it takes special effort to do so) as we
                // must keep the name list synchronized
                parameterList.FirstOrDefault();
                _parameterList.Add(parm);
                _parameterNameList.Add(parm.ParameterName);
                Touch();
            }
            return this;
        }
        /// <summary>
        /// Adds a list of parameters to the query
        /// </summary>
        /// <param name="parameterList"></param>
        public ISqlQuery AddParameter(IEnumerable<IDataParameter> parameterList)
        {
            foreach (var parm in parameterList)
            {
                AddParameter(parm);
            }
            return this;
        }
        /// <summary>
        /// Add a paremeter to the list, @ will be added to name automatically if not present
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public ISqlQuery AddParameter(string name, object value)
        {
            string fullName = name.Substring(0, 1) == "@" ? name : "@" + name;
            SqlValue val = new SqlValue(value);
            return AddParameter(new QueryParameter(name, val.Value));
        }
  
        /// <summary>
        /// Clears all criteria. This does not clear the field map.
        /// </summary>
        public virtual void Clear()
        {
            QueryType = QueryType.Select;
            Select = "*";
            From = "";
            GroupBy = "";
            parameterList = null;

            if (_sortCriterionList.IsValueCreated)
            {
                sortCriterionList.Clear();
            }
            parameterCount = 0;
            _whereClause = null;
            Clean();
        }

        public virtual ISqlQuery AddWhere(string condition)
        {
            WhereString where = new WhereString(condition);
            MergeWhere(where, JoinType.And);
            return this;
        }
        /// <summary>
        /// Add a simple "field = value" condition
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ISqlQuery AddWhere(string field, object value)
        {
            return AddWhere(field, value, ComparisonType.Equal);
        }
        public virtual ISqlQuery AddWhere(string field, object value, ComparisonType comparisonType)
        {
            IWhereCriterion crit;
            if (value is string
                && ((string)value).Substring(0, 1) == "@")
            {
                crit = new ParameterWhereCriterion(field, ((string)value).Substring(1), comparisonType);
            }
            else
            {
                crit = new WhereCriterion(field, value, comparisonType);
            }
            MergeWhere(crit, JoinType.And);
            return this;
        }
        /// <summary>
        /// Adds a condition and parameterizes the value. This is the equivalent of adding a field+same-named parameter, and then 
        /// adding a parameter with the value "value"
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ISqlQuery AddWhereParam(string field, object value)
        {
            return AddWhereParam(field, value, ComparisonType.Equal);
        }
        public virtual ISqlQuery AddWhereParam(string field, string paramName, object value)
        {
            return AddWhereParam(field, paramName, value, ComparisonType.Equal);
        }
        public virtual ISqlQuery AddWhereParam(string field, object value, ComparisonType comparisonType)
        {
            string paramName = GetParameterName(OptimizeParameterNames ? "" : field);
            return AddWhereParam(field, paramName, value, comparisonType);
        }
        public virtual ISqlQuery AddWhereParam(string field, string paramName, object value, ComparisonType comparisonType)
        {
            WhereCriterion crit = new ParameterWhereCriterion(field, paramName, comparisonType);
            MergeWhere(crit, JoinType.And);
            AddParameter("@" + paramName, value);
            return this;
        }

        public virtual ISqlQuery AddWhereOr(string condition)
        {
            var cond = new WhereString(condition);
            MergeWhere(cond, JoinType.Or);
            return this;
        }
        public virtual ISqlQuery AddWhere(IWhere condition)
        {
            MergeWhere(condition, JoinType.And);
            return this;
        }
        public virtual ISqlQuery AddWhereOr(IWhere condition)
        {
            MergeWhere(condition, JoinType.Or);
            return this;
        }
        #endregion

        #region private methods

        IQuery IQuery.Clone()
        {
            return Clone();
        }

        IQuery IQuery.Clone(QueryType type)
        {
            return Clone(type);
        }
        /// <summary>
        /// Return an unused parameter name WITHOUT the @
        /// </summary>
        /// <returns></returns>
        protected string GetParameterName(string basedOn="")
        {
            
            string baseName = string.IsNullOrEmpty(basedOn) ? "p"  : basedOn;

            string test = baseName=="p" ? baseName+parameterCount.ToString() : baseName ;
            
            while (ParameterExists("@" + test))
            {
                parameterCount++;
                test = baseName + parameterCount.ToString();
            }
            return test;
        }

        protected virtual void Touch()
        {
            this._IsDirty = true;
            if (Dirty != null)
            {
                Dirty(this, new EventArgs());

            }
        }

        /// <summary>
        /// Adds a condition to the "Where" property, and returns self for chainability
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="conditionType"></param>
        /// <returns></returns>
        protected void MergeWhere(IWhere component, JoinType joinType)
        {
            IWhereClause compound = whereClause as IWhereClause;
            if (joinType == JoinType.Or && compound != null && compound.Count == 0)
            {
                throw new Exception("There is no existing condition to OR");
            }
            if (whereClause is IWhereClause && compound.JoinType == joinType)
            {
                compound.Add(component);
            }
            else
            {
                // currently just a single criterion
                whereClause = new WhereClause(joinType, whereClause, component);
            }
            Touch();
        }


        /// <summary>
        /// Return an SQL field clause without an alias  (e.g. tbl.Field becomes just Field)
        /// </summary>
        /// <param name="sqlField"></param>
        /// <returns></returns>
        protected string unAliased(string sqlField)
        {
            int aliasIndex = sqlField.IndexOf(".");
            if (aliasIndex >= 0)
            {
                return sqlField.Substring(aliasIndex + 1);
            }
            else
            {
                return sqlField;
            }
        }

        #endregion
    }
}
