using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;

namespace IQMap.Implementation
{
    public class DBFieldInfo : IDBFieldInfo
    {
        public DBFieldInfo(PropertyInfo propertyInfo, string sqlName, bool isPk=false, bool ignoreNull=false, bool isReadOnly=false)
        {
            PropertyInfo = propertyInfo;
            IsPrimaryKey = isPk;
            SqlName = sqlName;
            IgnoreNull = ignoreNull;
            IsReadOnly = isReadOnly;
        }
        private string _SqlName;
        private PropertyInfo _PropertyInfo;

        #region public properties

        public PropertyInfo PropertyInfo
        {
            get
            {
                return _PropertyInfo;
            }
            protected set
            {
                _PropertyInfo = value;
                Name = value.Name;
                Type = value.PropertyType;
                IsNullable = Utils.IsNullableType(Type);
            }
        }
        public bool IsPrimaryKey { get; internal set; }
        
        public bool IgnoreNull { get; set; }
        public bool IsNullable { get; protected set; }
        public bool IsReadOnly { get; protected set; }

        public string SqlName
        {
            get
            {
                return _SqlName ?? Name;

            }
            protected set
            {
                _SqlName = value == "" ? null : value;
            }
        }

        public Type Type
        {
            get; protected set;
        }
        public string Name
        {
            get; protected set;
        }
        #endregion

        #region public methods
        public object GetValue(object obj)
        {
            return PropertyInfo.GetValue(obj, null);
        }
        public void SetValue(object obj, object value)
        {
            PropertyInfo.SetValue(obj, value, null);
        }


        #endregion
    }
}