using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap.SqlQueryBuilder;
using IQMap.Impl;

namespace IQMap
{
    public static class IQueryBuilderExtensionMethods
    {

        // TODO: Recode these to use enumerator instead of try/catch
        public static bool TrySingle<T>(this IQuery<T> source, out T item)
        {
            try
            {
                item= source.Single<T>();
                return true;

            }
            catch
            {
                item = default(T);
                return false;
            }

        }
        public static bool TryFirst<T>(this IQuery<T> source, out T item)
        {
            try
            {
                item = source.First<T>();
                return true;

            }
            catch
            {
                item = default(T);
                return false;
            }
        }
        public static bool TryLast<T>(this IQuery<T> source, out T item)
        {
            try
            {
                item = source.Last<T>();
                return true;

            }
            catch
            {
                item = default(T);
                return false;
            }
        }


    }
}
