using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap
{
    public enum JoinType
    {
        And = 1,
        Or = 2
    }
    public interface IWhere
    {
        string GetSql();
        bool IsCompoundFor(JoinType joinType);
        ISqlQuery Owner { get; set; }
        IWhere Clone();
    }
    
}
