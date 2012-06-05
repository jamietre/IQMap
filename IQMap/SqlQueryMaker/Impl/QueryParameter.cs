using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMap.Impl.Support;
using IQObjectMapper;

namespace IQMap.SqlQueryBuilder.Impl
{
    /// <summary>
    /// A non-SQL-implementation-specific dataparameter 
    /// </summary>
    public class QueryParameter: IDataParameter
    {
        #region constructors

        public QueryParameter(string name,  object value)
        {
            ParameterName = name.Substring(0, 1) == "@" ? name : "@" + name;
            Direction = ParameterDirection.Input;
            Value = (value == null) 
                ? DBNull.Value :
                value;

        }
        static QueryParameter()
        {
            PopulateDbTypeMap();
        }
        #endregion
        
        protected object _Value;

        #region public properties

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
                DbType = value==null || value==DBNull.Value  ? 
                   0: GetDBType(value.GetType());
            }
        }

        #endregion

        #region public methods

        public override string ToString()
        {
            return String.Format("{0}={1}", ParameterName, Value.ToString());
        }
        #endregion

        #region private methods

        protected virtual DbType GetDBType(Type type)
        {
            if (type.IsEnum)
            {
                return DbType.Int32;
            }
            Type t = Types.GetUnderlyingType(type);
            DbType dbType;
            if (DbTypeMap.TryGetValue(t, out dbType))
            {
                return dbType;
            }

             throw new InvalidCastException("Error converting system type '" + type.ToString() + "' to an SQL type." );

        }
        public override int GetHashCode()
        {
            return ParameterName.GetHashCode()+
                (Value!=null ?Value.GetHashCode():0);
        }
        public override bool Equals(object obj)
        {
            QueryParameter other = obj as QueryParameter;
            return other != null && other.ParameterName == ParameterName
                && other.Direction == Direction
                && other.Value.Equals(Value);
        }

        #endregion

        #region static config data

        public static Dictionary<Type, DbType> DbTypeMap;
        private static void PopulateDbTypeMap()
        {

            DbTypeMap = new Dictionary<Type, DbType>();
            var typeMap = DbTypeMap;
            typeMap[typeof(bool)] = DbType.Boolean;
            typeMap[typeof(string)] = DbType.String;
            typeMap[typeof(double)] = DbType.Double;
            typeMap[typeof(decimal)] = DbType.Decimal;
            typeMap[typeof(float)] = DbType.Single;
            typeMap[typeof(int)] = DbType.Int32;
            typeMap[typeof(long)] = DbType.Int64;
            typeMap[typeof(short)] = DbType.Int16;
            typeMap[typeof(ulong)] = DbType.UInt64;
            typeMap[typeof(uint)] = DbType.UInt32;
            typeMap[typeof(ushort)] = DbType.UInt16;
            typeMap[typeof(DateTime)] = DbType.DateTime;
            typeMap[typeof(Guid)] = DbType.Guid;
            typeMap[typeof(byte[])] = DbType.Binary;
        }
       
        #endregion
    }
}
