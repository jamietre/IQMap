using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace IQMap
{
    
    public enum ConditionType
    {
        And = 1,
        Or = 2
    }
    public enum SortOrder
    {
        Ascending = 1,
        Descending = 2
    }

    /// <summary>
    /// These flags identify the conceptual parts of a SQL query. They may not actually translate to a clause in every language. The SQL output from the IQuery/
    /// ISqlQueryMaker object is not intended to be used directly by a SQL engine, rather, it is supposed to generate a unique query for each distict conceptual
    /// query, and provide component parts that a DataStorageController can use to map to a particular engine's implementation.
    /// </summary>
    [Flags]
    public enum SqlClause
    {
        Select = 1,
        From = 2,
        Where = 4,
        OrderBy = 8,
        GroupBy=16,
        Having=32,
        Limit = 64,
        Table=128,
        UpdateSet = 256,
        InsertFields=512
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
}
