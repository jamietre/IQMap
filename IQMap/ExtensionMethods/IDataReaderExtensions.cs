using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Data;
using System.Dynamic;
using IQMap.Implementation;


namespace IQMap
{
    public static class IDataReaderExtensionMethods
    {
        public static T MapFirst<T>(this IDataReader reader)
        {
            return MapNext<T>(reader,true);
        }
        public static bool MapFirst(this IDataReader reader, object obj)
        {
            IDataReader result =  MapNext(reader,obj);
            if (result!=null)
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
        public static T MapNext<T>(this IDataReader reader, bool closeDataReader=true) 
        {
            T result;
            if (reader.Read())
            {
                result= Map<T>(reader);
            }
            else
            {
                result= default(T);
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
                throw new Exception("The datareader is closed.");
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
        public static T Map<T>(this IDataRecord reader)
        {
            T obj = Utils.GetInstanceOf<T>();

            if (Utils.IsMappableType<T>())
            {
                obj = (T)ChangeType(reader[0], typeof(T));
            }
            else
            {
                Map(reader, obj);
            }
            
            return obj;
        }
        
        /// <summary>
        /// Map to a data record.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="obj"></param>
        public static void Map(this IDataRecord reader,object obj)
        {
            Type t = Utils.GetUnderlyingType(obj.GetType());
            if (t.IsValueType || t == typeof(string))
            {
                throw new Exception("You can't map to a value type. You can, however, return new value types using Map<T>. Try that instead.");
            }
            bool isDict = obj is IDictionary<string, object>;
            IDBObjectData dbData=null;
            IDBClassInfo info=null;
           
            IDictionary<string,object> dict=null;

            if (isDict)
            {
                dict = (IDictionary<string, object>)obj;
            }
            else
            {
                dbData = IQ.DBData(obj);
                info = dbData.ClassInfo;
            }
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string name = reader.GetName(i);
                if (isDict)
                {
                    dict[name] = reader[i];
                }
                else
                {
                    if (info.FieldIndex(name) >= 0)
                    {
                        SetFieldFromSqlObject(dbData, name, reader[i]);
                    }
                }
            }
            if (!isDict)
            {
                dbData.Clean();
            }
        }
        /// <summary>
        /// Map all data in the reader to new objects of type T. When buffered, the reader is disposed of after finishing.
        /// The reader is not finalized when using unbuffered mode.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="buffered"></param>
        /// <returns></returns>
        public static IEnumerable<T> MapAll<T>(this IDataReader reader, bool buffered=true) 
        {
            if (!buffered)
            {
                return EnumerateReader<T>(reader);
            }
            else
            {
                List<T> list = new List<T>();

                while (reader.Read())
                {
                    list.Add(Map<T>(reader));
                }
                reader.Dispose();
                return list;
            }
        }
        private static IEnumerable<T> EnumerateReader<T>(IDataReader reader)
        {
            while (reader.Read())
            {
                yield return Map<T>(reader);
            }
            reader.Dispose();
        }
        #region private methods

        private static void SetFieldFromSqlObject(IDBObjectData dbData, string field, object dataObject)
        {

            IDBFieldInfo info;
            object obj = dbData.Owner;
            if (dbData.ClassInfo.TryGetFieldInfo(field, out info))
            {
                if (dataObject == System.DBNull.Value || dataObject==null)
                {
                    if (info.IsNullable)
                    {
                        info.SetValue(obj, null);
                    }
                    else if (info.IgnoreNull)
                    {
                        info.SetValue(obj, Utils.DefaultValue(info.Type));
                    }
                    else
                    {
                        throw new Exception("Can't assign null value in database for field '" + info.Name + "'.");
                    }
                }
                else
                {
                    info.SetValue(obj, ChangeType(dataObject, info.Type));
                }
            }
        }
        /// <summary>
        /// Change an object to another type accounting for nullable types and enums
        /// </summary>
        /// <param name="value"></param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        private static object ChangeType(object value, Type conversionType)
        {
            if (conversionType.IsGenericType &&
                conversionType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {

                if (value == null)
                    return null;

                System.ComponentModel.NullableConverter nullableConverter
                    = new System.ComponentModel.NullableConverter(conversionType);

                conversionType = nullableConverter.UnderlyingType;
            }

            if (conversionType.IsEnum)
            {
                return ChangeToEnumType(value, conversionType);
            }
            else
            {
                return Convert.ChangeType(value, conversionType);
            }
        }

        private static object ChangeToEnumType(object value, Type type)
        {
            if (value is string)
            {
                return Enum.Parse(type, value as string);
            }
            else
            {
                //long enumValue = (int)value;
                return Enum.ToObject(type, value);

            }
        }
        #endregion

    }
}
