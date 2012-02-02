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
        #region private properties


        protected Lazy<Dictionary<string, SqlValue>> _updateData =
            new Lazy<Dictionary<string, SqlValue>>();

        protected Dictionary<string, SqlValue> updateData
        {
            get
            {
                return _updateData.Value;
            }

        }
        #endregion

        #region public properties

        public QueryType QueryType
        {
            get;
            protected set;
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
        #endregion

        public event EventHandler Dirty;
        
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
        protected SqlFieldMap sqlFieldMap
        {
            get
            {
                return _sqlFieldMap.Value;
            }
        }
        protected Lazy<SqlFieldMap> _sqlFieldMap = new Lazy<SqlFieldMap>();
        protected List<SortCriterion> sortCriterionList {
            get
            {
                return _sortCriterionList.Value;
            }
        }
        protected Lazy<List<SortCriterion>> _sortCriterionList = new Lazy<List<SortCriterion>>();

        protected List<IDataParameter> parameterList
        {
            get
            {
                return _parameterList.Value;
            }
        }
        private Lazy<List<IDataParameter>> _parameterList = new Lazy<List<IDataParameter>>();

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

        #region public methods

        public ISqlQuery AddUpdateData(string fieldName, object value)
        {
            SqlValue sqlValue = new SqlValue(value);
            updateData[MapField(fieldName)] = sqlValue;
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
        public ISqlQuery Clone()
        {
            SqlQuery newQuery = new SqlQuery();

            newQuery.QueryType = QueryType;
            newQuery.Select = Select;
            newQuery.From = From;
            newQuery.GroupBy = GroupBy;

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
        /// <summary>
        /// Return the SQL for this query
        /// </summary>
        /// <returns></returns>
        public string GetQuery()
        {
            if (From=="") {
                throw new Exception("Incomplete query (must include at least From");
            }
            switch (QueryType)
            {
                case QueryType.Select:
                    return String.Format("SELECT {0} FROM {1}{2}{3}{4}",
                        Select,
                        From,
                        Where != "" ? " WHERE " + Where : "",
                        GroupBy != "" ? " GROUP BY " + GroupBy : "",
                        OrderBy != "" ? " ORDER BY " + OrderBy : "");
                case QueryType.Delete:
                    if (Where == "")
                    {
                        throw new Exception("You must project a WHERE clause for a DELETE query.");
                    }
                    return String.Format("DELETE FROM {0} WHERE {1}",
                        From,
                        Where);
                case QueryType.Update:
                    if (Where == "")
                    {
                        throw new Exception("You must project a WHERE clause for an UPDATE query.");
                    }
                    return String.Format("UPDATE {0} SET {1} WHERE {2}",
                        From,
                        UpdateSet,
                        Where);
                case QueryType.Insert:
                    return String.Format("INSERT INTO {0} {1} VALUES {2}",
                        From,
                        InsertFields,
                        InsertValues);
                default:
                    throw new Exception("Unsupported query type.");
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
                        parameterList.Add(parm);
                    }
                }
                else
                {
                    if (_parameterList.IsValueCreated)
                    {
                        parameterList.Clear();
                    }
                }
            }
        }
        /// <summary>
        /// Adds an SQL parameter to the query
        /// </summary>
        /// <param name="parm"></param>
        public ISqlQuery AddParameter(IDataParameter parm)
        {
            IDataParameter existing = parameterList.FirstOrDefault(item => item.ParameterName== parm.ParameterName);
            if (existing!=null )
            {
                if (existing.Value != parm.Value)
                {
                    throw new Exception("A parameter named \"" + parm.ParameterName + "\" with a different value has already been added to this query.");
                }
            }
            else
            {
                parameterList.Add(parm);
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
            if (_parameterList.IsValueCreated)
            {
                parameterList.Clear();
            }
            if (_sortCriterionList.IsValueCreated)
            {
                sortCriterionList.Clear();
            }
            _whereClause = null;
            Clean();
        }

        #endregion

        protected virtual void Touch()
        {
            this._IsDirty = true;
            if (Dirty != null)
            {
                Dirty(this, new EventArgs());

            }
        }
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
        protected string select;
        public string From
        {
            get
            {
                return from;
            }
            set
            {
                from = value ?? "";
                Touch();
            }
        }
        protected string from;
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
        protected string groupBy;
        public string OrderBy
        {
            get
            {
                List<SortCriterion> finalList = new List<SortCriterion>(sortCriterionList.Where(item => item.Priority==SortPriority.Required 
                    || item.Priority == SortPriority.RequiredOrder));
                finalList.AddRange(sortCriterionList.Where(item => item.Priority==SortPriority.Normal));
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
                    .Where(item => item.Priority == SortPriority.Required || item.Priority == SortPriority.RequiredOrder )
                    .Select(item=>item.GetSql()));
            }
            set
            {
                AddSort(value, SortPriority.Required);
            }
        }
        protected string orderBy;

        
        public virtual ISqlQuery AddWhere(string condition)
        {
            WhereString where = new WhereString(condition);
            MergeWhere(where,JoinType.And);
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
                crit = new ParameterWhereCriterion(field,((string)value).Substring(1), comparisonType);
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
            return AddWhereParam(field, paramName,value, ComparisonType.Equal);
        }
        public virtual ISqlQuery AddWhereParam(string field, object value, ComparisonType comparisonType)
        {
            return AddWhereParam(field, field, value, comparisonType);
        }
        public virtual ISqlQuery AddWhereParam(string field, string paramName, object value, ComparisonType comparisonType)
        {
            WhereCriterion crit = new ParameterWhereCriterion(field,paramName, comparisonType);
            MergeWhere(crit, JoinType.And);
            AddParameter("@" + paramName, value);
            return this;
        }

        public virtual ISqlQuery AddWhereOr(string condition)
        {
            var cond = new WhereString(condition);
            MergeWhere(cond,JoinType.Or);
            return this;
        }
        public virtual ISqlQuery AddWhere(IWhere condition)
        {
            MergeWhere(condition,JoinType.And);
            return this;
        }
        public virtual ISqlQuery AddWhereOr(IWhere condition)
        {
            MergeWhere(condition, JoinType.Or);
            return this;
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
            return AddSort(orderClause, SortPriority.Normal );
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
                AddSort(clause, order,priority);
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
        public string SqlAuditString()
        {
            return Utils.QueryAsSql(GetQuery(), Parameters);
        }

        /// <summary>
        /// Marks this object as unchanged (IsDirty=false)
        /// </summary>
        public virtual void Clean()
        {
            this._IsDirty = false;
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
        protected bool _IsDirty = false;
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


    }
}
