using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.QueryCache.Impl
{
    class CacheItem: ICacheItem
    {
        public CacheItem(CachedDataReader data, int size)
        {
            Data = data;
            Size = size;
            DateCreated = DateTime.Now;
            DateUpdated = DateTime.Now;
        }
        private bool _Expired = false;
        private CachedDataReader _Data;

        public CachedDataReader Data
        {
            get
            {
                return _Data;
            }
            set
            {
                _Data = value;
            }
        }
        public int Size { get; protected set; }
        public DateTime DateCreated { get; protected set; }
        public DateTime DateUpdated { get; protected set; }
        public bool Expired
        {
            get
            {
                if (_Expired)
                {
                    return true;
                }
                else
                {
                    return DateTime.Now - DateUpdated > SqlQueryBuilderConfig.CacheLifetime;
                }
            }

        }
        public void Touch()
        {
            DateUpdated = DateTime.Now;
        }
        public void Expire()
        {
            _Expired = true;
        }

    }
}
