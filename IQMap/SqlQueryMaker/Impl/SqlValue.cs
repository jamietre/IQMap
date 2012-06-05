using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap.Impl.Support;
using IQObjectMapper;
using IQMap.SqlQueryBuilder.Impl;

namespace IQMap.SqlQueryBuilder
{

    public class SqlValue: ISqlValue
    {
        public SqlValue()
        {

        }
        public SqlValue(object value)
        {
            Value = value;
        }
        public virtual ISqlValue Clone()
        {
            SqlValue clone = new SqlValue();
            clone._Value = _Value;
            clone.SqlDataFormat = SqlDataFormat;
            return clone;
        }
        public virtual SqlDataFormat SqlDataFormat
        { get; protected set; }
        public virtual object Value
        {
            get
            {
                return _Value;
            }
            protected set
            {
                _Value = value;
                // gets the non-nullable base type
                Type type;
                if (value == null)
                {
                    SqlDataFormat = SqlDataFormat.Null;
                }
                else
                {
                    type = Types.GetUnderlyingType(value.GetType());

                    if (type.IsEnum)
                    {
                        SqlDataFormat = SqlDataFormat.Numeric;
                        Value = Convert.ToInt32(value);
                    }
                    else if (typeof(DateTime).IsAssignableFrom(Types.GetUnderlyingType(type)))
                    {
                        SqlDataFormat = SqlDataFormat.DateTime;
                    }
                    else if (Types.IsNumericType(type))
                    {
                        // Note that IsNumeric
                        SqlDataFormat = SqlDataFormat.Numeric;
                    }
                    else if (typeof(string).IsAssignableFrom(type))
                    {
                        SqlDataFormat = SqlDataFormat.String;
                    }
                    else if (type == typeof(bool))
                    {
                        SqlDataFormat = SqlDataFormat.Numeric;
                        _Value = (bool)value ? 1 : 0;
                    }
                    else if (type==typeof(byte[])) {
                        SqlDataFormat = SqlDataFormat.Binary;
                        _Value = value;
                    }
                    else
                    {
                        throw new InvalidCastException("Unhandled data type: " + type.ToString());
                        //SqlDataFormat = SqlDataFormat.Unsupported;
                    }
                }
            }
        }
        protected object _Value;
        public string ValueString()
        {
            return ValueString(null);
        }
        public string ValueString(string template)
        {
            string dataValue;
            bool quoted = false;
            switch (SqlDataFormat)
            {
                case SqlDataFormat.Null:
                    dataValue = "NULL";
                    break;
                case SqlDataFormat.DateTime:
                    dataValue = String.Format("{0:d/M/yyyy HH:mm:ss}", Value);
                    quoted = true;
                    break;
                case SqlDataFormat.Numeric:
                    dataValue = Value.ToString();
                    break;
                case SqlDataFormat.String:
                    dataValue = SqlQueryUtility.CleanSql((string)Value);
                    quoted = true;
                    break;
                case SqlDataFormat.Parameter:
                    dataValue = (string)Value;
                    quoted = false;
                    if (!String.IsNullOrEmpty(template) && template != "{0}")
                    {
                        throw new ArgumentException("This is a parameter-type value, the template is invalid");
                    }
                    break;
                default:
                    throw new InvalidCastException("Unhandled data type");
            }
            string quoteChar = quoted ? "'" : "";
            if (String.IsNullOrEmpty(template))
            {
                return quoteChar + dataValue + quoteChar;
            }
            else
            {
                return quoteChar + String.Format(template, dataValue) + quoteChar;
            }
        }
        public override string ToString()
        {
            return ValueString();
        }
    }
}
