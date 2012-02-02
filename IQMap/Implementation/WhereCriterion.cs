using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.Implementation
{

    public class WhereCriterion : IWhereCriterion
    {
        public ISqlQuery Owner { get; set; }
        public WhereCriterion()
        {

        }
        public WhereCriterion(string field, object value)
        {
            Field = field;
            ComparisonType = ComparisonType.Equal;
            Value = value;
        }
        public WhereCriterion(string field, object value, ComparisonType comparisonType)
        {
            Field = field;
            ComparisonType = comparisonType;
            Value = value;
        }
        public virtual IWhere Clone()
        {
            WhereCriterion clone = new WhereCriterion();
            clone.Field = Field;
            clone.SqlValue = SqlValue.Clone();
            clone.ComparisonType = ComparisonType;
            return clone;
        }
        public virtual object Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                SqlValue = new SqlValue(value);
            }
        }
        protected object _Value;
        public ISqlValue SqlValue { get; protected set; }
        public string Field { get; set; }
        public ComparisonType ComparisonType { get; set; }
        
        public string GetSql()
        {
            return (Owner != null ? Owner.MapField(Field) : Field)
                + ComparisonOperator() 
                + FormattedValue();
        }

        protected string FormattedValue()
        {
            return SqlValue.ValueString(FieldTemplate());
        }
        protected string FieldTemplate()
        {
            switch (ComparisonType)
            {
                case ComparisonType.LikeAny:
                    return "%{0}%";
                case ComparisonType.LikeStartsWith:
                    return "{0}%";
                case ComparisonType.LikeEndsWith:
                    return "%{0}";
                default:
                    return "{0}";
            }
        }
        protected string ComparisonOperator()
        {
            string oper;
            
            switch (ComparisonType)
            {
                case ComparisonType.Equal:
                    oper= SqlValue.SqlDataFormat == SqlDataFormat.Null ? " IS ":"=";
                    break;
                case ComparisonType.NotEqual:
                    oper = SqlValue.SqlDataFormat == SqlDataFormat.Null ? " IS NOT " : "<>";
                    break;
                case ComparisonType.LikeAny:
                    oper = " LIKE ";
                    break;
                case ComparisonType.LikeStartsWith:
                    oper=" LIKE ";
                    break;
                case ComparisonType.LikeEndsWith:
                    oper = " LIKE ";
                    break;
                case ComparisonType.LT:
                    oper = "<";
                    break;
                case ComparisonType.LE:
                    oper = "<=";
                    break;
                case ComparisonType.GT:
                    oper = ">";
                    break;
                case ComparisonType.GE:
                    oper = ">=";
                    break;
                case ComparisonType.In:
                    oper = " IN ";
                    throw new Exception("In is not implemented");
                default:
                    throw new Exception("Unhandled comparison type");
            }
            return oper;
        }
        
        public bool IsCompoundFor(JoinType joinType)
        {
            return false;
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
