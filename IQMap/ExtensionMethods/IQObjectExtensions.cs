using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Data;
using IQMap.Impl;
using IQMap.SqlQueryBuilder.Impl;
using IQMap.Impl.Support;

namespace IQMap
{
    public static class IIQObjectExtensions
    {

        #region IDatabaseBoundObject extension methods
        /// <summary>
        /// Zeroes out the primary key, disconnecting an item from the database
        /// </summary>
        /// <param name="obj"></param>
        public static void Disconnect(this IQObject obj)
        {
            IQ.Disconnect(obj);
        }
        public static void Track(this IQObject obj)
        {
            IQ.Track(obj);
        }
        public static bool IsDirty(this IQObject obj, string fieldName)
        {
            return IQ.MapperCache.GetTrackedObjectData(obj).IsDirty(fieldName);
        }
        public static void Clean(this IQObject obj)
        {
            IQ.MapperCache.GetTrackedObjectData(obj).Clean();
        }
        public static IEnumerable<string> DirtyFieldNames(this IQObject obj)
        {
            return IQ.MapperCache.GetTrackedObjectData(obj).DirtyFieldNames;
        }
        public static bool IsNew(this IQObject obj) 
        {
            return ClassInfo.IsNew(obj);
        }
        public static bool IsDirty(this IQObject obj)
        {
            return IQ.MapperCache.GetTrackedObjectData(obj).IsDirty();
        }
        public static IObjectData ObjectData(this IQObject obj)
        {
            return IQ.MapperCache.GetTrackedObjectData(obj);
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
            IQ.MapperCache.GetTrackedObjectData(source).CopyTo(destination);
        }
        public static T Clone<T>(this T source) where T: IQObject
        {
            return (T)ObjectData(source).Clone();
        }
        public static void Load<T>(this T source, string where, params object[] parms) where T : class, IQObject
        {
            IQ.From<T>(where, parms).To(source).Single();
        }
        public static void Load<T>(this T source, int primaryKeyValue) where T : class, IQObject
        {
            IQ.From<T>(primaryKeyValue).To(source).Single();
        }
        //public static object Clone(this IQObject source)
        //{
        //    return ObjectData(source).Clone();
        //}
        
        public static bool Save<T>(this T obj, params object[] options) where T: IQObject
        {
           return IQ.Save(obj,options);
        }
        public static bool Delete<T>(this T obj, params object[] options) where T: IQObject 
        {
            return IQ.Delete(obj, options);
        }

        #endregion

        #region private methods


        #endregion

    }
}
