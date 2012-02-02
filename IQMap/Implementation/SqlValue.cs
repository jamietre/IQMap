using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.Implementation
{
    public interface ISqlValue
    {
        ISqlValue Clone();
        object Value {get;}
        string ValueString();
        string ValueString(string template);
        SqlDataFormat SqlDataFormat  { get; }
    }
    public class SqlValueParm : SqlValue
    {
        public SqlValueParm()
        {

        }
        public SqlValueParm(string parmName)
        {
            Value = parmName;
        }

        public override object Value 
        {
            get {
                return _Value;
            }
            protected set {
                SqlDataFormat = SqlDataFormat.Parameter;
                _Value=(string)value;
            }
        }

    }

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
                    type = Utils.GetUnderlyingType(value.GetType());

                    if (type.IsEnum)
                    {
                        SqlDataFormat = SqlDataFormat.Numeric;
                        Value = Convert.ToInt32(value);
                    }
                    else if (Utils.IsNumericType(type))
                    {
                        SqlDataFormat = SqlDataFormat.Numeric;
                    }
                    else if (typeof(string).IsAssignableFrom(type))
                    {
                        SqlDataFormat = SqlDataFormat.String;
                    }
                    else if (typeof(DateTime).IsAssignableFrom(Utils.GetUnderlyingType(type)))
                    {
                        SqlDataFormat = SqlDataFormat.DateTime;
                    }
                    else if (type == typeof(bool))
                    {
                        SqlDataFormat = SqlDataFormat.Numeric;
                        _Value = (bool)value ? 1 : 0;
                    }
                    else
                    {
                        SqlDataFormat = SqlDataFormat.Unsupported;
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
                    dataValue = Utils.CleanSql((string)Value);
                    quoted = true;
                    break;
                case SqlDataFormat.Parameter:
                    dataValue = (string)Value;
                    quoted = false;
                    if (!String.IsNullOrEmpty(template) && template != "{0}")
                    {
                        throw new Exception("This is a parameter-type value, the template is invalid");
                    }
                    break;
                default:
                    throw new Exception("Unhandled data type");
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
    }
}
