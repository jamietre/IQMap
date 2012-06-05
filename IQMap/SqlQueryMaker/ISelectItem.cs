using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.SqlQueryBuilder
{
    public interface ISelectItem: ISelect
    {
        string Field { get; set; }
        string Alias { get; set; }
        new ISelectItem Clone();
    }
}
