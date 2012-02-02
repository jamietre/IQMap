using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.Implementation
{


    public class WhereClause: IWhereClause 
    {
        List<IWhere> CriteriaList = new List<IWhere>();
        public JoinType JoinType { get; set; }
        public ISqlQuery Owner {
            get
            {
                return _Owner;
            }
            set
            {
                foreach (IWhere item in CriteriaList)
                {
                    item.Owner = value;
                }
                _Owner = value;
            }
        }
        protected ISqlQuery _Owner;

        public WhereClause()
        {

        }
        public WhereClause(JoinType joinType)
        {
            JoinType = joinType;
        }
        public WhereClause(JoinType joinType, params IWhere[] criteria)
        {
            if (criteria == null || criteria.Length < 2)
            {
                throw new Exception("A where clause can only be created from two or more components.");
            }
            JoinType = joinType;
            Add(criteria);
        }
        public IWhere Clone()
        {
            WhereClause clone = new WhereClause();
            clone.JoinType = JoinType;
            foreach (var item in CriteriaList)
            {
                clone.Add(item.Clone());
            }
            return clone;
        }
        public int Count
        {
            get
            {
                return CriteriaList.Count;
            }
        }
        public IWhereClause Add(IWhere criterion)
        {
            if (criterion == null)
            {
                throw new Exception("Can't add a null value to a WhereClause");
            }
            
            if (!CriteriaList.Contains(criterion)) {
                criterion.Owner = Owner;
                CriteriaList.Add(criterion);
            }
            return this;
        }
        public IWhereClause Add(params IWhere[] criteria)
        {
            foreach (var item in criteria)
            {
                Add(item);
            }
            return this;
        }
        public IWhereClause Add(string field, object value)
        {
            return Add(field, value, ComparisonType.Equal);
            
        }
        public IWhereClause Add(string field, object value, ComparisonType comparisonType)
        {
            WhereCriterion crit = new WhereCriterion(field, value, comparisonType);
            return Add(crit);
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
                    throw new Exception("Unhandled join operator");
            }
        }
        /// <summary>
        /// Returns true if any components of this query are of a different join type than the test
        /// </summary>
        public bool IsCompoundFor(JoinType joinType)
        {
            return Count>1 && 
                (JoinType != joinType ||
                CriteriaList.Any(item => item is WhereClause && ((WhereClause)item).JoinType != JoinType));
        }
        public string GetSql()
        {
            string output = "";

            foreach (var member in CriteriaList)
            {
                string childSql = member.GetSql();
                if (childSql != "")
                {
                    output += (output == "" ? "" : " " + JoinOperator() + " ") +
                        (member.IsCompoundFor(JoinType) ? "(" + childSql + ")" :
                        childSql);
                }
            }
            return output;
        }
        public override int GetHashCode()
        {
            return GetSql().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is IWhere &&
                ((IWhere)obj).GetSql() == GetSql();
        } 
    }
    
}
