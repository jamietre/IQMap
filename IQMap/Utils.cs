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
        public static IEnumerable<T> EmptyEnumerable<T>()
        {
            yield break;
        }
        /// <summary>
        // return an instance of an object or value type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetInstanceOf<T>() {
                    
            T obj;
            if (IsMappableType<T>())
            {
                obj = default(T);
            }
            else
            {
                obj = Activator.CreateInstance<T>();
                // We have special handling for Expando-like objects -- don't create metadata for them
                if (!(obj is IDictionary<string,object>))
                {
                    IQ.CreateDBData(obj);
                }
            }
            
            return obj;
        }
        /// <summary>
        // return an instance of an object or value type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object GetInstanceOf(Type type)
        {

            object obj;
            if (IsMappableType(type))
            {
                obj = Utils.DefaultValue(type);
            }
            else
            {
                obj = Activator.CreateInstance(type);
                // We have special handling for Expando-like objects -- don't create metadata for them
                if (!(obj is IDictionary<string, object>))
                {
                    IQ.CreateDBData(obj);
                }
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
            return type.IsValueType && !(type == typeof(string) || type == typeof(char) || type == typeof(bool) || type == typeof(DateTime));
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
        /// <summary>
        /// The type is a class that can have metadata associated with it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsMappableClass<T>()
        {
            Type t = GetUnderlyingType(typeof(T));
            return t.IsClass && t!=typeof(string);
        }
        /// <summary>
        /// If the type can be the direct target of the map.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsMappableType<T>()
        {
            return IsMappableType(typeof(T));
        }
        public static bool IsMappableType(object obj)
        {
            return IsMappableType(obj.GetType());
        }
        public static bool IsMappableType(Type type)
        {
            Type t = GetUnderlyingType(type);
            return t.IsEnum || t.IsValueType || t.IsPrimitive || t == typeof(string) || t == typeof(byte[]);
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
        public static HashSet<string> GetParameterNames(string sql)
        {

            HashSet<string> parameterNames = new HashSet<string>( new ParmNameComparer());
            if (!string.IsNullOrEmpty(sql))
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
                                string parmName;
                                if (nextIndex < 0)
                                {
                                    // yield return new Tuple<string, int,int>(sql.Substring(curIndex), curIndex, sql.Length - pos);
                                    parmName=(sql.Substring(curIndex));
                                }
                                else
                                {
                                    int len = nextIndex - curIndex;
                                    parmName=sql.Substring(curIndex, len);
                                }
                                parameterNames.Add(parmName);
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
            return parameterNames;
        }
        //public static IEnumerable<Tuple<string,int,int>> GetParametexrNames(string sql)
        //{
        //    HashSet<Tuple<string,int,int>>
        //    int pos = 0;
        //    bool quoted = false;
        //    while (pos >= 0)
        //    {
        //        char cur = sql[pos];
        //        if (!quoted)
        //        {
        //            if (cur == '\'')
        //            {
        //                quoted = true;
        //            }
        //            else
        //            {

        //                if (cur == '@')
        //                {
        //                    int curIndex = pos;
        //                    int nextIndex = GetParameterNameStop(sql, pos + 1);
        //                    if (nextIndex < 0)
        //                    {
        //                        yield return new Tuple<string, int,int>(sql.Substring(curIndex), curIndex, sql.Length - pos);
        //                    }
        //                    else
        //                    {
        //                        int len =  nextIndex - curIndex;
        //                        yield return new Tuple<string, int, int>(sql.Substring(curIndex, len), curIndex, len);
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (cur == '\'')
        //            {
        //                quoted = false;
        //            }

        //        }
        //        if (pos + 1 < sql.Length)
        //        {
        //            pos = sql.IndexOf("@", pos + 1);
        //        }
        //        else
        //        {
        //            pos = -1;
        //        }
        //    }
        //}
        /// <summary>
        /// Assuming startPos is inside a parameter name in an sql query, returns the first character position falling
        /// outside the parameter name by looking for characters that are not valid in a name.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="startPos"></param>
        /// <returns></returns>
        private static int GetParameterNameStop(string sql, int startPos = 0)
        {
            return sql.IndexOfAny(" (),=<>!.;:\r\n\t".ToCharArray(), startPos);
        }
        private class ParmNameComparer : IEqualityComparer<string>
        {
            public ParmNameComparer()
            {
                caseSensitive = IQ.Config.ParametersCaseSensitive;
            }
            protected bool caseSensitive;
            public bool Equals(string x, string y)
            {
                return caseSensitive ? x.Equals(y, StringComparison.CurrentCultureIgnoreCase) :
                    x.Equals(y, StringComparison.CurrentCulture);
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}