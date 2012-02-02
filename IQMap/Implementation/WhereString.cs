using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.Implementation
{
    /// <summary>
    /// Legacy - to support unstructured where stuff
    /// </summary>
    public class WhereString: IWhere
    {
        public ISqlQuery Owner { get; set; }
        public string Where { get; set; }
        public WhereString()
        {

        }
        public WhereString(string clause)
        {
            Where = clause;
        }
        public IWhere Clone()
        {
            WhereString clone = new WhereString();
            clone.Where = Where;
            return clone;
        }
        public string GetSql()
        {
            return Where;
        }
        public bool IsCompoundFor(JoinType joinType)
        {
            return true;
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
