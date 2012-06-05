using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.SqlQueryBuilder
{
    public interface ISqlValue
    {
        ISqlValue Clone();
        object Value { get; }
        string ValueString();
        string ValueString(string template);
        SqlDataFormat SqlDataFormat { get; }
    }
}
