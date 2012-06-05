using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data;
using System.Data.SqlTypes;
using System.Data.SqlClient;
using System.Dynamic;

namespace IQMap.Impl
{
    public static class ExtensionMethods
    {
        public static int IndexOf<T>(this IList<T> list, T item, IEqualityComparer<T> comparer) where T: IComparable
        {
            for (int i=0;i<list.Count;i++)
            {
                if (comparer.Equals(list[i], item))
                {
                    return i;
                }
            }
            return -1;
            
        }
        public static IEnumerable<T> EmptyEnumerable<T>()
        {
            yield break;
        }
        public static string[] SplitClean(this string stringList, char separator)
        {
            return stringList.Trim()
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
           
        }
        public static bool EqualsIgnoreCase(this string source,string other)
        {
            return source.Equals(other, StringComparison.OrdinalIgnoreCase);
        }
    }
}