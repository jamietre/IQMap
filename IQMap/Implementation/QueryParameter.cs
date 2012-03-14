using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.Implementation
{
    class QueryParameter: IDataParameter
    {

        public QueryParameter(string name, object value)
        {
            ParameterName = name.Substring(0, 1) == "@" ? name : "@" + name;
            Value = value;

        }
        protected void Initialize()
        {
            Direction = ParameterDirection.Input;

        }
        protected object _Value;

        public DbType DbType
        {
            get;
            set;
        }

        public ParameterDirection Direction
        {
            get;
            set;
        }

        public bool IsNullable
        {
            get;
            protected set;
        }

        public string ParameterName
        {
            get;
            set;
        }

        public string SourceColumn
        {
            get;
            set;
        }

        public DataRowVersion SourceVersion
        {
            get;
            set;
        }

        public object Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                DbType = value==null ? 0: GetDBType(value.GetType());
            }
        }

        protected virtual DbType GetDBType(Type type)
        {
            if (type.IsEnum)
            {
                return DbType.Int32;
            }
            Type t = Utils.GetUnderlyingType(type);
            DbType dbType;
            if (Config.DbTypeMap.TryGetValue(t, out dbType))
            {
                return dbType;
            }

             throw new Exception("Error converting system type '" + type.ToString() + "' to an SQL type." );
            
        }


    }
}
