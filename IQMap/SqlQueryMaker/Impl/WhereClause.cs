using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Data;
using IQMap.Impl.Support;
using IQMap.SqlQueryBuilder;

namespace IQMap.SqlQueryBuilder.Impl
{


    public class WhereClause: IWhereClause
    {
        #region constructors
        public static implicit operator WhereClause(string where)
        {
            var clause = new WhereClause();
            clause.Add(where);
            return clause;
        }
        public WhereClause()
        {
            JoinType = JoinType.And;
        }
        public WhereClause(JoinType joinType)
        {
            JoinType = joinType;
        }
        public WhereClause(JoinType joinType,params IWhere[] criteria)
        {
            JoinType = joinType;
            AddImpl(criteria);
        }

        #endregion

        #region private properties

        protected List<IWhere> _InnerList;

        List<IWhere> InnerList {
            get{
                if (_InnerList == null)
                {
                    _InnerList = new List<IWhere>();
                }
                return _InnerList;
            }
        }

        protected string JoinOperator()
        {
            switch (JoinType)
            {
                case JoinType.And:
                    return "AND";
                case JoinType.Or:
                    return "OR";
                default:
                    throw new InvalidOperationException("Unhandled join operator");
            }
        }
        //protected bool HasParameters
        //{
        //    get
        //    {
        //        return _Parameters != null && _Parameters.Count > 0;
        //    }
        //}
        protected IEnumerable<ICollection<IDataParameter>> GetParameterCollections() {
            foreach (var item in Items) {
                yield return item.Parameters;
            }
        }
        #endregion

        #region public properties

        public IEnumerable<IWhere> Items
        {
            get
            {
                if (!IsEmpty)
                {
                    foreach (var item in InnerList)
                    {
                        yield return item;
                    }
                }
                else
                {
                    yield break;
                }
            }
        }
        public ICollection<IDataParameter> Parameters
        {
            get
            {
                return IsEmpty ?
                    QueryBuilder.EmptyParameterCollection :
                    new LazyReadOnlyCollection<IDataParameter>(GetParameterCollections()) as ICollection<IDataParameter>;
                
            }
        }

        public JoinType JoinType { get; set; }
        public ISqlQueryMaker Owner
        {
            get
            {
                return _Owner;
            }
            set
            {
                foreach (IWhere item in InnerList)
                {
                    item.Owner = value;
                }
                _Owner = value;
            }
        }
        protected ISqlQueryMaker _Owner;

      
       
        public int Count
        {
            get
            {
                return InnerList.Count;
            }
        }
       
        #endregion

        #region public methods

        public virtual IWhereClause Set(string clause)
        {
            Clear();
            return Add(clause);
        }
        public virtual IWhereClause Add(IWhere condition)
        {

            MergeWhere(condition, JoinType);
            return this;
        }
        public virtual IWhereClause Add(IWhere condition, JoinType joinType)
        {

            MergeWhere(condition, joinType);
            return this;
        }

        public IWhereClause Add(IEnumerable<IWhere> criteria)
        {
            return Add(criteria, JoinType);
        }
        public IWhereClause Add(IEnumerable<IWhere> criteria, JoinType joinType)
        {
            if (criteria != null)
            {
                foreach (var item in criteria)
                {
                    Add(item,joinType);
                }
            }
            return this;
        }
        /// <summary>
        /// Add a simple "field = value" condition
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual IWhereClause Add(string field, object value, bool parameterize=true)
        {
            return Add(field, value, ComparisonType.Equal);
        }
        public virtual IWhereClause Add(string field, object value, ComparisonType comparisonType, bool parameterize=true)
        {
            return Add(field, value, comparisonType, JoinType.And, parameterize);
        }
        public IWhereClause Add(string field, object value, ComparisonType comparisonType, JoinType joinType, bool parameterize = true)
        {
            IWhereItem crit;

            crit = new WhereItem(field, value, comparisonType);
            MergeWhere(crit, joinType);
            return this;
        }
        public virtual IWhereClause Add(string condition, params IDataParameter[] parameters)
        {
            WhereString where = new WhereString(condition, parameters);
            MergeWhere(where, JoinType.And);
            return this;
        }

        public virtual IWhereClause Add(string condition, JoinType joinType, params IDataParameter[] parameters)
        {
            WhereString where = new WhereString(condition, parameters);
            MergeWhere(where, joinType);
            return this;
        }
     
      
        /// <summary>
        /// Returns true if any components of this query are of a different join type than the test
        /// </summary>
        public bool MustParenthesizeFor(JoinType joinType)
        {
            return Count > 1 &&
                (JoinType != joinType ||
                InnerList.Any(item => item is WhereClause && ((WhereClause)item).JoinType != JoinType));
        }
        public string GetSql()
        {
            
            string output = "";
            if (!IsEmpty)
            {
                if (InnerList.Count == 1)
                {
                    return InnerList[0].GetSql();
                }
                else
                {
                    foreach (var member in InnerList)
                    {
                        string childSql = member.GetSql();
                        if (childSql != "")
                        {
                            output += (output == "" ? "" : " " + JoinOperator() + " ") +
                                (member.MustParenthesizeFor(JoinType) ? "(" + childSql + ")" :
                                childSql);
                        }
                    }
                }
            }
            return output;
        }
       
       
        public void Clear()
        {
            _InnerList = null;
        }
        public WhereClause Clone()
        {
            WhereClause clone = new WhereClause();
            clone.JoinType = JoinType;
            foreach (var item in InnerList)
            {
                clone.Add(item.Clone());
            }
            return clone;
        }


        public override int GetHashCode()
        {
            return GetSql().GetHashCode() + Parameters.Sum(item => item.Value.GetHashCode());
        }
        public override bool Equals(object obj)
        {
            IWhere other = obj as IWhere;
            if (other == null || GetSql() != other.GetSql())
            {
                return false;
            }
            SetComparer<IDataParameter> comparer = new SetComparer<IDataParameter>(Parameters, other.Parameters);
            return comparer.AreEqual;
        }

        public override string ToString()
        {
            return GetSql();
        }

        public void Touch()
        {
            if (Owner != null)
            {
                Owner.Touch();
            }
        }

        #endregion

        #region private methods

        public bool IsEmpty
        {
            get {
                return _InnerList == null || InnerList.Count == 0;
            }
        }

        /// <summary>
        /// Adds a condition to the "Where" property, and returns self for chainability
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="conditionType"></param>
        /// <returns></returns>
        protected void MergeWhere(IWhere item, JoinType joinType)
        {
            if (ReferenceEquals(item, this))
            {
                throw new ArgumentException("You're trying to add a where clause to itself.");
            }

            if (JoinType == joinType || IsEmpty)
            {
                AddImpl(item);
            }
            else
            {
                    // currently just a single criterion
                var clause = new WhereClause(joinType, this.Clone(), item);
                _InnerList = clause.InnerList;
                JoinType = joinType;
            }
            Touch();
        }
        protected void AddImpl( params IWhere[] criteria )
        {
            foreach (var item in criteria)
            {
                InnerList.Add(item);
                item.Owner = this.Owner;
            }
        }
        //protected void RecurseList(IWhere list, Action<IWhereItem> callback)
        //{
        //    IWhereClause clause = list as IWhereClause;
        //    if (clause!=null) {
        //        foreach (var item in clause.Items)
        //        {
        //            IWhereItem innerItem = item as IWhereItem;
        //            if (innerItem != null)
        //            {
        //                callback(innerItem);
        //            }
        //            else
        //            {
        //                RecurseList(innerItem,callback);
        //            }
        //        }
        //    }

        //}

        #endregion

        #region private methods

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
        IWhereClause IWhereClause.Clone()
        {
            return Clone();
        }
        IWhere IWhere.Clone()
        {
            return Clone();
        }

        #endregion

    }
    
}
