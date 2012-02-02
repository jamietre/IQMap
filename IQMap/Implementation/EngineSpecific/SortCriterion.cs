using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap;

namespace IQMap.Implementation
{
    public class SortCriterion
    {
        public SortCriterion(string field, SortOrder order, SortPriority priority)
        {
            Field = field;
            Order = order;
            Priority = priority;

        }
        public SortCriterion Clone()
        {
            SortCriterion clone = new SortCriterion(Field,Order,Priority);
            return clone;
        }
        public ISqlQuery Owner {get;set;}

        public string Field { get; set; }
        public SortOrder Order { get; set; }
        public SortPriority Priority { get; set; }

        protected IDictionary<string, string> _FieldMap;

        public string GetSql()
        {
            return (Owner != null ? Owner.MapField(Field) : Field)
                + (Order == SortOrder.Descending ? " desc" : "");
        }
    }
}
