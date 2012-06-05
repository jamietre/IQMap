using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap.SqlQueryBuilder.Impl;

namespace IQMap.SqlQueryBuilder
{
    public interface IOrderByItem: IOrderBy
    {
        string Field { get; set; }
        new OrderByItem Clone();
    }
}
