using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data;
using System.Data.SqlTypes;
using System.Data.SqlClient;

namespace IQMap
{
    public static class Utils
    {
        // return an instance of an object or value type
        public static T GetInstanceOf<T>() {
                    
            T obj;
            if (typeof(T)==typeof(string) || typeof(T).IsValueType)
            {
                obj = default(T);
            }
            else
            {
                obj = Activator.CreateInstance<T>();
            }
            
            return obj;
        }
        
        public static object DefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
        /// <summary>
        /// Render a paramaterized SQL query as plain SQL. NO SQL INJECTION PROTECTION
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>

        public static string QueryAsSql(string query, IEnumerable<SqlParameter> parms)
        {
            string result = query;
            if (parms != null)
            {
                foreach (SqlParameter sp in parms)
                {
                    result = result.Replace(sp.ParameterName, sp.ParameterValueForSQL());
                }
            }
            return result;
        }
        /// <summary>
        /// Render an SQL command as plain SQL. NO SQL INJECTION PROTECTION
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        public static string CommandAsSql(SqlCommand sc)
        {
            StringBuilder sql = new StringBuilder();
            Boolean FirstParam = true;

            sql.AppendLine("use " + sc.Connection.Database + ";");
            switch (sc.CommandType)
            {
                case CommandType.StoredProcedure:
                    sql.AppendLine("declare @return_value int;");

                    foreach (SqlParameter sp in sc.Parameters)
                    {
                        if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
                        {
                            sql.Append("declare " + sp.ParameterName + "\t" + sp.SqlDbType.ToString() + "\t= ");

                            sql.AppendLine(((sp.Direction == ParameterDirection.Output) ? "null" : sp.ParameterValueForSQL()) + ";");

                        }
                    }

                    sql.AppendLine("exec [" + sc.CommandText + "]");

                    foreach (SqlParameter sp in sc.Parameters)
                    {
                        if (sp.Direction != ParameterDirection.ReturnValue)
                        {
                            sql.Append((FirstParam) ? "\t" : "\t, ");

                            if (FirstParam) FirstParam = false;

                            if (sp.Direction == ParameterDirection.Input)
                                sql.AppendLine(sp.ParameterName + " = " + sp.ParameterValueForSQL());
                            else

                                sql.AppendLine(sp.ParameterName + " = " + sp.ParameterName + " output");
                        }
                    }
                    sql.AppendLine(";");

                    sql.AppendLine("select 'Return Value' = convert(varchar, @return_value);");

                    foreach (SqlParameter sp in sc.Parameters)
                    {
                        if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
                        {
                            sql.AppendLine("select '" + sp.ParameterName + "' = convert(varchar, " + sp.ParameterName + ");");
                        }
                    }
                    break;
                case CommandType.Text:
                    sql.Append(QueryAsSql(sc.CommandText, sc.Parameters));
                    break;
            }

            return sql.ToString();
        }
        public static String ParameterValueForSQL(this SqlParameter sp)
        {
            string retval = null;
            bool quote = false;

            if (sp.Value == null)
            {
                retval = "NULL";
            }
            else
            {
                if (sp.Value.GetType() == typeof(bool))
                {
                    retval = (bool)sp.Value ? "1" : "0";
                }
                switch (sp.SqlDbType)
                {
                    case SqlDbType.Char:
                    case SqlDbType.NChar:
                    case SqlDbType.NText:
                    case SqlDbType.NVarChar:
                    case SqlDbType.Text:
                    case SqlDbType.Time:
                    case SqlDbType.VarChar:
                    case SqlDbType.Xml:
                    case SqlDbType.Date:
                    case SqlDbType.DateTime:
                    case SqlDbType.DateTime2:
                    case SqlDbType.DateTimeOffset:
                    case SqlDbType.SmallDateTime:
                        quote = true;
                        if (retval == null)
                        {
                            retval = sp.Value.ToString();
                        }
                        break;
                    case SqlDbType.Bit:
                    case SqlDbType.BigInt:
                    case SqlDbType.Decimal:
                    case SqlDbType.Float:
                    case SqlDbType.Int:
                    case SqlDbType.Money:
                    case SqlDbType.Real:
                    case SqlDbType.SmallInt:
                    case SqlDbType.SmallMoney:
                    case SqlDbType.Timestamp:
                    case SqlDbType.TinyInt:
                        if (retval == null)
                        {
                            retval = CleanNumber(sp.Value.ToString());
                        }
                        break;
                    default:
                        throw new Exception("Unsupported data type for inline parameter substituion");
                }

            }
            if (quote)
            {
                retval = "'" + retval.Replace("'", "''") + "'";
            }
            return retval;
        }
        /// <summary>
        /// Render a paramaterized SQL query as plain SQL. NO SQL INJECTION PROTECTION
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>

        public static string QueryAsSql(string query, IEnumerable parms)
        {
            string result = query;
            if (parms != null)
            {
                foreach (SqlParameter sp in parms)
                {
                    result = result.Replace(sp.ParameterName, sp.ParameterValueForSQL());
                }
            }
            return result;
        }
        public static string CleanNumber(string numberString)
        {
            double result;
            string output = "";
            if (TryCleanNumber<double>(numberString, out result))
            {
                if ((double)(int)result == result)
                {
                    output = ((int)result).ToString();
                }
                else
                {
                    output = result.ToString();
                }
            }
            return output;
        }
        public static bool TryCleanNumber(string numberString, out long Number)
        {
            return TryCleanNumber<long>(numberString, out Number);
        }

        public static bool TryCleanNumber(string numberString, out double Number)
        {
            return TryCleanNumber<double>(numberString, out Number);
        }
        private static bool TryCleanNumber<T>(string numberString, out T Number) where T : IConvertible
        {
            object numericResult;
            bool couldParse;
            string theCleanNumber;

            if (String.IsNullOrEmpty(numberString))
            {
                numericResult = default(T);
                couldParse = false;
            }
            else
            {
                bool isPct = numberString.IndexOf("%") >= 0;
                theCleanNumber = numberString.Replace("$", "").Replace(",", "").Replace("%", "");
                if (typeof(long).IsAssignableFrom(typeof(T)))
                {
                    long result;
                    couldParse = long.TryParse(theCleanNumber, out result);
                    numericResult = isPct ? result / 100 : result;
                }
                else if (typeof(double).IsAssignableFrom(typeof(T)))
                {
                    double result;
                    couldParse = double.TryParse(theCleanNumber, out result);
                    numericResult = isPct ? result / 100 : result;
                }
                else
                {
                    throw new Exception("Unsupported type for TryCleanNumber");
                }
            }
            if (couldParse)
            {
                Number = (T)(object)numericResult;
            }
            else
            {
                Number = default(T);
            }
            return couldParse;
        }

        /// <summary>
        /// Returns true if the object is a primitive numeric type, e.g. exluding string & char
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNumericType(object obj)
        {
            Type t = GetUnderlyingType(obj.GetType());
            return IsNumericBaseType(t);
        }
        public static bool IsNumericType(Type type)
        {
            Type t = GetUnderlyingType(type);
            return IsNumericBaseType(t);
        }
        private static bool IsNumericBaseType(Type type)
        {
            return type.IsPrimitive && !(type == typeof(string) || type == typeof(char) || type == typeof(bool));
        }
        /// <summary>
        /// Return the proper type for an object (ignoring nullability)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetUnderlyingType(Type type)
        {

            if (type != typeof(string) && IsNullableType(type))
            {
                return Nullable.GetUnderlyingType(type);
            }
            else
            {
                return type;
            }

        }
        public static bool IsNullableType(Type type)
        {
            return type == typeof(string) ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
        public static string CleanSql(string sql)
        {
            string cleanSql = sql.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ").Replace("\\", " ").Replace("'", "''");
            while (cleanSql.IndexOf("  ") > 0)
            {
                cleanSql = cleanSql.Replace("  ", " ");
            }
            return cleanSql;
        }

        /// <summary>
        /// Locate all named parameters in a query
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static IEnumerable<Tuple<string,int,int>> GetParameterNames(string sql)
        {
            int pos = 0;
            bool quoted = false;
            while (pos >= 0)
            {
                char cur = sql[pos];
                if (!quoted)
                {
                    if (cur == '\'')
                    {
                        quoted = true;
                    }
                    else
                    {

                        if (cur == '@')
                        {
                            int curIndex = pos;
                            int nextIndex = GetParameterNameStop(sql, pos + 1);
                            if (nextIndex < 0)
                            {
                                yield return new Tuple<string, int,int>(sql.Substring(curIndex), curIndex, sql.Length - pos);
                            }
                            else
                            {
                                int len =  nextIndex - curIndex;
                                yield return new Tuple<string, int, int>(sql.Substring(curIndex, len), curIndex, len);
                            }
                        }
                    }
                }
                else
                {
                    if (cur == '\'')
                    {
                        quoted = false;
                    }

                }
                if (pos + 1 < sql.Length)
                {
                    pos = sql.IndexOf("@", pos + 1);
                }
                else
                {
                    pos = -1;
                }
            }
        }
        /// <summary>
        /// Assuming startPos is inside a parameter name in an sql query, returns the first character position falling
        /// outside the parameter name by looking for characters that are not valid in a name.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="startPos"></param>
        /// <returns></returns>
        private static int GetParameterNameStop(string sql, int startPos = 0)
        {
            return sql.IndexOfAny(" =<>!.;:\r\n\t".ToCharArray(), startPos);
        }
    }
}