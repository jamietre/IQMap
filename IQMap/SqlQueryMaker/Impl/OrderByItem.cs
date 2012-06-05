using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap;

namespace IQMap.SqlQueryBuilder.Impl
{
    public class OrderByItem: IOrderByItem
    {
        public OrderByItem()
        {

        }
        public OrderByItem(string field, SortOrder order)
        {
            Field = field;
            Order = order;

        }
        public OrderByItem Clone()
        {
            OrderByItem clone = new OrderByItem(Field,Order);
            return clone;
        }
        public ISqlQueryMaker Owner { get; set; }

        public string Field { get; set; }
        public SortOrder Order { get; set; }

        protected IDictionary<string, string> _FieldMap;

        public string GetSql()
        {
            return IsEmpty ? "" :
                ((Owner != null ? 
                    Owner.GetFieldMap(Field) : 
                    Field)
                + (Order == SortOrder.Descending ? 
                    " desc" : 
                    ""));
        }


        public void Reverse()
        {
            Order = Order == SortOrder.Ascending ?
                SortOrder.Descending :
                SortOrder.Ascending;
        }

        IOrderBy IOrderBy.Clone()
        {
            return Clone();
        }

        public void Clear()
        {
            Field = "";
            Order = SortOrder.Ascending;
        }

        public bool IsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(Field);
            }
        }
        public override string ToString()
        {
            return GetSql();
        }
        public override int GetHashCode()
        {
            return GetSql().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return obj is IOrderBy ?
                ((IOrderBy)obj).GetSql()==GetSql() :
                false;

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
    }
}
