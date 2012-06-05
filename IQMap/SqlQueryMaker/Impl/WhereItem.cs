using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMap.Impl.Support;
using IQMap.SqlQueryBuilder.Impl;

namespace IQMap.SqlQueryBuilder.Impl
{

    public class WhereItem : IWhereItem
    {
       
        #region constructor
        public WhereItem()
        {
            ComparisonType = ComparisonType.Equal;
            Parameterize = true;
        }
        public WhereItem(string field, object value)
        {
            Field = field;
            ComparisonType = ComparisonType.Equal;
            Value = value;
            Parameterize = true;
        }
        public WhereItem(string field, object value, ComparisonType comparisonType=ComparisonType.Equal,bool parameterize=true)
        {
            Field = field;
            ComparisonType = comparisonType;
            Value = value;
            Parameterize = parameterize;
        }
        #endregion

        #region private properties

        protected string _ParameterName;
        protected ISqlQueryMaker _Owner;
        
        protected string ParameterName
        {
            get
            {
                // auto-generate a parameter name if none was assigned when this was created.

                if (String.IsNullOrEmpty(_ParameterName))
                {
                    _ParameterName = Owner!= null ?
                        Owner.GetNewParameterName(Field) :
                        "@" + Field;
                }
                return _ParameterName;
            }
            set
            {
                _ParameterName = value;
            }
        }
        
        #endregion

        #region public properties

        public ISqlQueryMaker Owner
        {
            get {
                return _Owner;
            }
            set {
                _Owner = value;
                ParameterName = null;
            }
        }
        public ICollection<IDataParameter> Parameters
        {
            get
            {
                return Parameterize ?
                    new LazyReadOnlyCollection<IDataParameter>(new QueryParameter(ParameterName, Value)) as ICollection<IDataParameter> :
                    QueryBuilder.EmptyParameterCollection;
                
            }
        }
        public bool Parameterize
        { 
            get;set;
        }
        public virtual object Value
        {
            get;
            set;
        }
        public string Field { get; set; }
        protected ISqlValue SqlValue
        {
            get
            {
                if (!Parameterize)
                {
                    return new SqlValue(Value);
                }
                else
                {
                    throw new InvalidOperationException("This item is parameterized. You should not be accessing SqlValue.");
                }

            }
        }
        public ComparisonType ComparisonType { get; set; }

        #endregion

        #region public methods
        
        public virtual IWhereItem Clone()
        {
            WhereItem clone = new WhereItem();
            clone.Field = Field;
            clone.Value = Value;
            clone.Parameterize = Parameterize;
            clone.ComparisonType = ComparisonType;

            return clone;
        }
        
        public string GetSql()
        {
            return (Owner != null ? Owner.GetFieldMap(Field) : Field)
                + ComparisonOperator()
                + FormattedValue();
        }


        public virtual void Clear()
        {
            Field = "";
            ComparisonType = ComparisonType.Equal;
            Value = null;
        }

        public virtual bool IsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(Field);
            }
        }

        /// <summary>
        /// Single items are always associative, never need parenthesizing.
        /// </summary>
        /// <param name="joinType"></param>
        /// <returns></returns>
        public bool MustParenthesizeFor(JoinType joinType)
        {
            return false;
        }
        public override int GetHashCode()
        {
            return Field.GetHashCode() + Value.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            IWhereItem other = obj as IWhereItem;
            return other != null &&
                other.Field.Equals(Field) &&
                other.Value.Equals(Value);
                
        }
        public override string ToString()
        {
            return GetSql();
        }


        #endregion

        #region private methods

        protected string FormattedValue()
        {
            return Parameterize ?
                ParameterName :
                SqlValue.ValueString(FieldTemplate());
               
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
                    oper = Value==null  ? " IS " : "=";
                    break;
                case ComparisonType.NotEqual:
                    oper = Value==null  ? " IS NOT " : "<>";
                    break;
                case ComparisonType.LikeAny:
                    oper = " LIKE ";
                    break;
                case ComparisonType.LikeStartsWith:
                    oper = " LIKE ";
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
                    throw new NotImplementedException("In is not implemented");
                default:
                    throw new InvalidOperationException("Unhandled comparison type");
            }
            return oper;
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

        IWhere IWhere.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
