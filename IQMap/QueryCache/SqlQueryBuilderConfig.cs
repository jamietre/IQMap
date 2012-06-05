using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Data;
using IQMap.Impl.Support;
using IQObjectMapper;
using IQMap.QueryCache.Impl;

namespace IQMap.QueryCache
{
    
    public static class SqlQueryBuilderConfig
    {
        /// <summary>
        /// The total number of rows to be cached. Unfortunately it's hard to figure out how much data,
        /// exactly, is being cached, so this is a short-term kludge to limit the cache.
        /// </summary>
        public static int CacheSizeLimit;
        /// <summary>
        /// Objects bigger than this will not be cached. (Since they are probably ad-hoc queries).
        /// </summary>
        public static int CacheObjectSizeLimit;
        public static int CacheSize;
        public static TimeSpan CacheLifetime;

        static SqlQueryBuilderConfig()
        {
            QueryResultCache = new ConcurrentDictionary<ICacheKey, ICacheItem>();
            QueryOrder = new Queue<ICacheKey>();
            CacheSizeLimit = 1000000;
            CacheObjectSizeLimit = 100000;
            CacheSize = 0;
            CacheLifetime = TimeSpan.FromSeconds(10);
        }
         private static ConcurrentDictionary<ICacheKey, ICacheItem> QueryResultCache;
         private static Queue<ICacheKey> QueryOrder;

         public static bool TryGetCachedData(ISqlQuery query, out IDataReader data)
         {
             var key = new CacheKey(query);
             ICacheItem cached;
             if (QueryResultCache.TryGetValue(key, out cached)) {
                 if (cached.Expired)
                 {
                     RemoveFromCache(key);
                     data = null;
                     return false;
                 }
                 else
                 {
                     CachedDataReader reader = ((CacheItem)cached).Data;
                     reader.Reset();
                     data = new CachedDataReaderView(reader);
                     return true;
                 }
             } else {
                 data=null;
                 return false;
             }
         }
         private static object ActiveLock = new object();
         public static void RemoveAllForTable(string tableName)
         {
             lock(ActiveLock) {
                foreach (var key in QueryResultCache.Keys.Where(item => item.TableName == tableName))
                {
                     QueryResultCache[key].Expire();
                }
            }

         }
         private static bool RemoveFromCache(ICacheKey key)
         {
             lock (ActiveLock)
             {
                 ICacheItem item;
                 if (QueryResultCache.TryRemove(key, out item))
                 {
                     QueryOrder.Dequeue();
                     CacheSize -= item.Size;
                     return true;
                 }
                 else
                 {
                     return false;
                 }
            }
         }
         public static bool RemoveFromCache(ISqlQuery query)
         {
            ICacheKey key = new CacheKey(query);
            return RemoveFromCache(key);
         }
        /// <summary>
        ///  Clear the entire cache
        /// </summary>
        /// <returns></returns>
         public static void Clear()
         {
             lock (ActiveLock)
             {
                 QueryResultCache.Clear();
                 QueryOrder.Clear();
             }

         }
         public static IDataReader AddToCache(ISqlQuery query, IDataReader data)
         {
             ICacheKey key = new CacheKey(query);

             CachedDataReader cached = new CachedDataReader(data);

             int newSize = cached.Size() + key.Query.Length;
             ICacheItem item;
             if (newSize < CacheObjectSizeLimit)
             {
                 while (CacheSize + newSize > CacheSizeLimit && QueryOrder.Count > 0)
                 {
                     ICacheKey oldestKey = QueryOrder.Peek();
                     if (!RemoveFromCache(oldestKey))
                     {
                         break;
                     }
                 }
                 if (QueryOrder.Count == 0 && CacheSize != 0)
                 {
                     CacheSize = 0;
                     QueryResultCache.Clear();
                     // this should be logged but thrwing an error seems too much... test the hellout of this?
                 }
                 // It is possible that we failed to remove it from the cache, so just bail out
                 // if still won't fit
                 if (CacheSize + newSize <= CacheSizeLimit)
                 {

                     item = new CacheItem(cached, newSize);

                     if (QueryResultCache.TryAdd(key, item))
                     {
                         QueryOrder.Enqueue(key);
                     }
                 }
                 else
                 {

                 }
             }
             // whether or not we end up caching the datareader, we must return the in-memory copy because you can't go back 
             // after reading a datareader. We only try to cache when buffering is enabled, so nothing's really lost.
             return cached;
         }
    }
}
