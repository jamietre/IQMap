using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMap.SqlQueryBuilder;

namespace IQMap.SqlQueryBuilder.Impl
{
    public class OrderByClause: IOrderByClause
    {

        #region constructors

        public static implicit operator OrderByClause(string orderBy)
        {
            return new OrderByClause(orderBy);
        }
        public OrderByClause()
        {
          
        }
        public OrderByClause(string clause) {
            Parse(clause);
        }
        #endregion

        #region private properties

        // "MustBeMissing" means throw an error if the clause is already in the list
        // "WhenMissing" means ignore if it's arleady there
        // "Always" will remove an existing one and replace with this at the end of the clause

        protected enum AddWhen
        {
            MustBeMissing=1,
            WhenMissing=2,
            Always=3
        }
        protected List<IOrderByItem> _InnerList;
        protected List<IOrderByItem> InnerList
        {
            get
            {
                if (_InnerList == null)
                {
                    _InnerList = new List<IOrderByItem>();
                }
                return _InnerList;
            }
        }
        protected ISqlQueryMaker _Owner;

        #endregion

        #region public properties

        public ISqlQueryMaker Owner
        {
            get
            {
                return _Owner;
            }
            set
            {
                _Owner = value;
                if (_InnerList != null)
                {
                    foreach (var item in InnerList)
                    {
                        item.Owner = value;
                    }
                }
            }
        }

        #endregion

        #region public methods

        public IOrderByClause Set(string orderClause)
        {
            Clear();
            return Add(orderClause);

        }

        public void Reverse()
        {
            if (IsEmpty) {
                throw new InvalidOperationException("The list is not ordered, you can't reverse the order.");
            }
            foreach (var item in InnerList)
            {
                item.Reverse();
            }
        }

        /// <summary>
        /// Add sort clauses in regular SQL syntax
        /// </summary>
        /// <param name="orderClause"></param>
        /// <returns></returns>
        public IOrderByClause Add(string orderClause)
        {
            return Add(orderClause,AddWhen.MustBeMissing);
        }

        public IOrderByClause AddAlways(string orderClause)
        {
            return Add(orderClause,AddWhen.Always);
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
        public IOrderByClause Add(string field, SortOrder order)
        {
            return Add(field, order, AddWhen.MustBeMissing);
        }
        public IOrderByClause AddAlways(string field, SortOrder order)
        {
            return Add(field, order, AddWhen.Always);
        }
        public IOrderByClause Add(IOrderBy item)
        {
            return Add(item, AddWhen.MustBeMissing);
        }
        public IOrderByClause AddAlways(IOrderBy item)
        {
            return Add(item, AddWhen.Always);
        }

        public IOrderByClause AddIfNotPresent(IOrderBy item)
        {
            return Add(item, AddWhen.WhenMissing);
        }

        public IOrderByClause AddIfNotPresent(string orderClause)
        {
            return Add(orderClause, AddWhen.WhenMissing);
        }

        public IOrderByClause AddIfNotPresent(string field, SortOrder order)
        {
            return Add(field, order, AddWhen.WhenMissing);
        }
        public void Clear()
        {
            _InnerList = null;

        }
        public string GetSql()
        {
            if (!IsEmpty)
            {
                return String.Join(",", InnerList);
            }
            else
            {
                return "";
            }

        }
        public OrderByClause Clone()
        {
            var clone = new OrderByClause();
            foreach (var item in InnerList)
            {
                clone.Add(item.Clone());
            }
            return clone;
        }
        public override string ToString()
        {
            return GetSql();
        }
        IOrderByClause IOrderByClause.Clone()
        {
            return Clone();
        }
        IOrderBy IOrderBy.Clone()
        {
            return Clone();
        }

        public void Touch()
        {
            if (Owner != null)
            {
                Owner.Touch();
            }
        }


        public bool IsEmpty
        {
            get
            {
                return _InnerList == null || InnerList.Count == 0;
            }
        }
        public override int GetHashCode()
        {
            return GetSql().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is IOrderBy)
            {
                return ((IOrderBy)obj).GetSql() == GetSql();
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region private methods

        protected void Parse(string clause)
        {
 	        InnerList.Clear();
            if (!String.IsNullOrEmpty(clause))
            {
                Add(clause);
            }

        }

        protected IOrderByClause Add(string orderClause,  AddWhen when)
        {
            string[] items = orderClause.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (items.Length == 0)
            {
                throw new ArgumentOutOfRangeException("Cannot add a missing field for sorting.");
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
                Add(clause, order,when);
 
            }
            return this;
        }
               
        protected IOrderByClause Add(string field, SortOrder order,AddWhen when)
        {
            if (String.IsNullOrEmpty(field))
            {
                throw new ArgumentOutOfRangeException("The field name cannot be missing to add a sort");
            }
            string sortOrder = order == SortOrder.Ascending ? "" : " DESC";
            int index = InnerList.Count - 1;
            string fieldNameClean = unAliased(field.Trim().ToLower());
            var crit = new OrderByItem(fieldNameClean, order);
           
            return Add(crit,when);
        }
        protected IOrderByClause Add(IOrderBy orderBy, AddWhen when)
        {
            if (orderBy is IOrderByItem)
            {
                return Add((IOrderByItem)orderBy, when);
            }
            else
            {
                foreach (var item in (IOrderByClause)orderBy)
                {
                    Add(item);
                }
                return this;
            }
        }
        protected IOrderByClause Add(IOrderByItem item, AddWhen when)
        {

            int existingIndex = InnerList.FindIndex(existing => 
                existing.Field == item.Field);

            if (existingIndex >= 0)
            {
                switch(when) {
                    case AddWhen.MustBeMissing:
                        throw new ArgumentException(String.Format("The clause already contains a sort criteria for {0}",
                            item.Field));
                    case AddWhen.WhenMissing:
                        return this;
                    case AddWhen.Always:
                        InnerList.RemoveAt(existingIndex);
                        break;
                }
            }
            
            
            item.Owner = this.Owner;
           
            InnerList.Add(item);
            Touch();
            return this;

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

        ISqlQueryMaker ISqlClause.Owner
        {
            get
            {
                return Owner;
            }
            set
            {
                Owner = (ISqlQueryMaker)value;
            }
        }

        ISqlClause ISqlClause.Clone()
        {
            return Clone();
        }


        public IEnumerator<IOrderByItem> GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


    }
}
