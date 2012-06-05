using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.QueryCache
{
    public interface ICacheItem
    {
        int Size { get; }
        DateTime DateCreated { get; }
        DateTime DateUpdated { get; }
        bool Expired { get; }
        void Touch();
        void Expire();
    }
}
