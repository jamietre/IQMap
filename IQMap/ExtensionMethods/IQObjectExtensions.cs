using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using IQMap.Implementation;

namespace IQMap
{
    public static class IIQObjectExtensions
    {

        #region IDatabaseBoundObject extension methods

        public static void Clean(this IQObject obj)
        {
            IQ.DBData(obj).Clean();
        }
        public static IEnumerable<string> DirtyFieldNames(this IQObject obj)
        {
            return IQ.DBData(obj).DirtyFieldNames;
        }
        public static bool IsNew(this IQObject obj)
        {
            return IQ.DBData(obj).IsNew();
        }
        public static bool IsDirty(this IQObject obj)
        {
            return IQ.DBData(obj).IsDirty();
        }

        public static bool IsDirty(this IQObject obj, string fieldName)
        {
            return IQ.DBData(obj).IsDirty(fieldName);
        }

        public static IDBObjectData DBData(this IQObject obj)
        {
            return IQ.DBData(obj);
        }

        public static bool Save(this IQObject obj)
        {
            return IQ.Save(obj);
        }

        /// <summary>
        /// Copy the database bound properties of one object to another. If the target object is dirty, will throw
        /// an error. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void CopyTo(this IQObject source, object destination)
        {
            IQ.DBData(source).CopyTo(destination);
        }
        public static T Clone<T>(this IQObject source)
        {
            return (T)Clone(source);
        }
        public static object Clone(this IQObject source)
        {
            return DBData(source).Clone();
        }
        #endregion

        #region private methods


        #endregion

    }
}
