using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Data;
using System.Dynamic;
using IQObjectMapper.Adapters;

namespace IQMap
{
    /// <summary>
    /// Methods for mapping directly from an IDataReader to a poco object. These should mostly mirror the methods
    /// in IEnumerableVKPExtensions just by wrapping each datareater 
    /// </summary>
    public static class IDataReaderExtensionMethods
    {
        

        public static T MapFirst<T>(this IDataReader reader)
        {
            return MapNext<T>(reader, true);
        }
        public static bool MapFirst(this IDataReader reader, object obj)
        {
            IDataReader result = MapNext(reader, obj);
            if (result != null)
            {
                reader.Close();
            }
            return result != null;
        }
        /// <summary>
        ///  Read the next record from a datareader and map to a new object of type T. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="closeDataReader">When true (default), the datareader is closed after mapping is finished. Set to false
        /// to read further data.</param>
        /// <returns></returns>
        public static T MapNext<T>(this IDataReader reader, bool closeDataReader = true)
        {
            T result;
            if (reader.Read())
            {
                result = ((IDataRecord)reader).Map<T>();
            }
            else
            {
                result = default(T);
            }
            if (closeDataReader)
            {
                reader.Dispose();
            }
            return result;
        }
        /// <summary>
        ///  Read the next record from a datareader and map to an existing object. Returns the datareader for chaining.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="closeDataReader">When true (default), the datareader is closed after mapping is finished. Set to false
        /// to read further data.</param>
        /// <returns>true if successful, false if the reader did not contain another record.</returns>
        public static IDataReader MapNext(this IDataReader reader, object obj)
        {
            if (reader.IsClosed)
            {
                throw new InvalidOperationException("The datareader is closed.");
            }

            bool result = reader.Read();
            if (result)
            {
                Map(reader, obj);
            }

            return result ? reader : null;
        }
        /// <summary>
        /// Map a datareader to a new object of type T. If T implements IDictionary&lt;string,object&gt; then 
        /// the dictionary will be populated.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T Map<T>(this IDataRecord record)
        {
            var wrapper = new DataRecordAdapter(record);
            return wrapper.Map<T>();

        }
        /// <summary>
        /// Map to a data record.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="obj"></param>
        public static void Map(this IDataRecord record, object obj)
        {
            var wrapper = new DataRecordAdapter(record);
            wrapper.Map(obj);

        }
        /// <summary>
        /// Map all data in the reader to new objects of type T. When buffered, the reader is disposed of after finishing.
        /// The reader is not finalized when using unbuffered mode.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="buffered"></param>
        /// <returns></returns>
        public static IEnumerable<T> MapAll<T>(this IDataReader reader, bool buffered = true)
        {
            var enumerated = EnumerateReader<T>(reader);
            if (!buffered)
            {
                return enumerated;
            }
            else
            {
                List<T> list = new List<T>();
                foreach (var item in enumerated)
                {
                    list.Add(item);
                }
                reader.Dispose();
                return list;
            }
        }

        #region private methods

        private static IEnumerable<T> EnumerateReader<T>(IDataReader reader)
        {
            var wrapper = new DataReaderAdapter(reader);
            foreach (var item in wrapper)
            {
                yield return item.Map<T>();
            }
            reader.Dispose();
        }

        #endregion

    }
}
