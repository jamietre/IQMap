using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.Impl
{
    public interface IOptionParser : IDbContextData, IQueryOptions
    {
        void MapTo(IDbContextData optionUser);
        void MapTo(IQueryOptions optionUser);
    }
}
