using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap
{

    public enum SqlDataFormat
    {
        String = 1,
        Numeric = 2,
        DateTime = 3,
        Null = 4,
        Parameter=5,
        Unsupported = 6
    }
    public enum ComparisonType
    {
        Equal = 1,
        NotEqual = 2,
        LT = 3,
        LE = 4,
        GT = 5,
        GE = 6,
        LikeAny = 7,
        LikeStartsWith = 8,
        LikeEndsWith = 9,
        In = 10
    }
    interface IWhereCriterion : IWhere
    {
        string Field { get; set; }
        ComparisonType ComparisonType { get; set; }
        object Value {get;set;}
    }
}
