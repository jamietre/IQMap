using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    /// RequiredOrder means the field is required in this position and the order cannot be changed. Required can be ordered asc or desc but not removed.
    /// </summary>
    public enum SortPriority
    {
        RequiredOrder = 1,
        Required = 2,
        Normal = 3,
        Default = 4
    }
    
}
