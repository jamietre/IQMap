using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.QueryCache.Impl
{
    /// <summary>
    /// A wrapper for a CachedDataReader that doesn't use that classes internal position counter. This lets us have threadsafe operations against
    /// more than one instance of the same reader, so we don't have to clone it for access.
    /// </summary>
    public class CachedDataReaderView: CachedDataReader
    {
        public CachedDataReaderView(IList<CachedDataRecord> list)
            : base()
        {
            CurrentRow = -1;
            InnerList = list;
        }
       

    }

}
