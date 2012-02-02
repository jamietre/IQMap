using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace IQMap
{
    public interface IDBFieldInfo
    {
        string Name {get;}
        string SqlName {get;}
        PropertyInfo PropertyInfo {get;}
        bool IsPrimaryKey { get; }
        /// <summary>
        /// When true, null values in the database are converted to default class value and vice versa
        /// </summary>
        bool IgnoreNull {get;}
        bool IsNullable { get; }
        bool IsReadOnly { get; }
        Type Type {get; }
        
        object GetValue(object obj);
        void SetValue(object obj, object value);
    }
}
