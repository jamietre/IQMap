using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap.Impl;
using IQMap.Impl.Support;
using IQObjectMapper;
using IQObjectMapper.Adapters;

namespace IQMap
{
    public static class IEnumerableExtensions
    {
        #region IEnumerable-T- extension methods
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return true;
            }
            else
            {
                return !source.GetEnumerator().MoveNext();
            }
        }
        /// <summary>
        /// Return the sequence, or a sequence containing a single new instance of T if the sequence is null or empty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<T> OrNew<T>(this IEnumerable<T> source)
        {

            bool found = false;
            if (source!=null){
                
                foreach (var item in source)
                {
                    found = true;
                    yield return item;

                }
            }
            if (!found)
            {
                yield return Types.GetInstanceOf<T>();
            }
        }
        /// <summary>
        /// Try to get a single result from the sequence. If the sequnce is empty or there is more than one element, return false.
        /// When more than one element is present, the first element will still be returned as the output.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool TrySingle<T>(this IEnumerable<T> list, out T obj ) {
            bool found = false;
            obj = default(T);
            foreach (var el in list)
            {
                obj = el;
                found = true;
                break;
            }
            
            return found;
        }
        public static bool TryFirst<T>(this IEnumerable<T> list, out T obj)
        {
            bool first = false;
            obj = default(T);
            foreach (var item in list)
            {
                obj = item;
                first = true;
                break;
            }
            return first;

        }


        #endregion

        #region IEnumerable-Ienumerable-KVP-string,object extenstion methods
        /// <summary>
        /// Map the contents of a list of dictionary-type objects to a list of typed objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<T> Map<T>(this IEnumerable<IEnumerable<KeyValuePair<string,object>>> list)
        {
            if (typeof(T) == typeof(IQTestOnly))
            {
                yield break;
            }
            foreach (IEnumerable<KeyValuePair<string,object>> item in list) {
                yield return Map<T>(item);
            }
        }

        /// <summary>
        /// Map the contents of a list of KeyValue paris to a new object. If the new type is a value type,
        /// the first value found in the dictionary will be its target. If that value doesn't happen to be a value
        /// type an error will result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Map<T>(this IEnumerable<KeyValuePair<string, object>> source)
        {
            if (typeof(T) == typeof(IQTestOnly))
            {
                return (T)(IQTestOnly)null;
            }
            T target;
            if (Types.IsSimpleType(typeof(T)))
            {
                target = ObjectMapper.ParseValue<T>((IConvertible)source.First().Value);
            }
            else
            {
                target = ObjectMapper.ToNew<T>(source);
            }
            return target;
        }

        /// <summary>
        /// Map the contents of a list of KeyValue paris to another existing object.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="obj"></param>
        public static void Map(this IEnumerable<KeyValuePair<string, object>> source, object target)
        {
            ObjectMapper.ToExisting(source, target);
        }
        /// <summary>
        /// Map an enumeration of objects to an enumeration of dictionarys of their fields/values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="?"></param>
        /// <returns></returns>
        public static IEnumerable<IDictionary<string, object>> ToDictionaries<T>(this IEnumerable<T> source, StringComparer comparer=null)
        {
            foreach (var obj in source) {
                yield return new PropertyDictionaryAdapter(obj);
            }
        }

        /// <summary>
        /// Map an enumeration of obejcts to an enumeration of KeyValuePair sequences. This is similar to ToDictionary but
        /// with a bit less overhead, if lookups are not needd.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="?"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<KeyValuePair<string, object>>> ToKeyValuePairSequences<T>(this IEnumerable<T> source, StringComparer comparer = null)
        {
            foreach (var obj in source)
            {
                yield return new PropertyKVPAdapter(obj);
            }
        }
        /// <summary>
        /// Map the first two columns of the output to a single KeyValuePair per row
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, object>> ToKeyValuePairs<T>(this IEnumerable<T> source) where T: class
        {
            var classInfo = IQ.MapperCache.GetClassInfo<T>();
            var keyField = classInfo.Fields.First();
            var valueField = classInfo.Fields.ElementAt(2);

            foreach (var obj in source)
            {
                string key = keyField.GetValue(obj).ToString();
                object value = valueField.GetValue(obj);
                yield return new KeyValuePair<string, object>(key, value);
            }
        }
        #endregion
    }
}
