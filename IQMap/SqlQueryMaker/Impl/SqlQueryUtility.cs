using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;


namespace IQMap.SqlQueryBuilder.Impl
{
    /// <summary>
    /// A couple miscellanous tools
    /// </summary>
    public class SqlQueryUtility
    {
        
        public static string QueryAsSql(string query, IEnumerable<IDataParameter> parms)
        {
            string result = query;
            if (parms != null)
            {
                foreach (IDataParameter sp in parms)
                {
                    result = result.Replace(sp.ParameterName, ParameterValueForSQL(sp));
                }
            }
            return result;
        }
        /// <summary>
        /// Render an SQL command as plain SQL. NO SQL INJECTION PROTECTION
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        public static string CommandAsSql(IDbCommand sc)
        {
            StringBuilder sql = new StringBuilder();
            Boolean FirstParam = true;

            sql.AppendLine("use " + sc.Connection.Database + ";");
            switch (sc.CommandType)
            {
                case CommandType.StoredProcedure:
                    sql.AppendLine("declare @return_value int;");

                    foreach (IDataParameter sp in sc.Parameters)
                    {
                        if ((sp.Direction == ParameterDirection.InputOutput) || (sp.Direction == ParameterDirection.Output))
                        {
                            sql.Append("declare " + sp.ParameterName + "\t" + sp.DbType.ToString() + "\t= ");

                            sql.AppendLine(((sp.Direction == ParameterDirection.Output) ? "null" : ParameterValueForSQL(sp)) + ";");

                        }
                    }

                    sql.AppendLine("exec [" + sc.CommandText + "]");

                    foreach (IDataParameter sp in sc.Parameters)
                    {
                        if (sp.Direction != ParameterDirection.ReturnValue)
                        {
                            sql.Append((FirstParam) ? "\t" : "\t, ");

                            if (FirstParam) FirstParam = false;

                            if (sp.Direction == ParameterDirection.Input)
                                sql.AppendLine(sp.ParameterName + " = " + ParameterValueForSQL(sp));
                            else

                                sql.AppendLine(sp.ParameterName + " = " + sp.ParameterName + " output");
                        }
                    }
                    sql.AppendLine(";");

                    sql.AppendLine("select 'Return Value' = convert(varchar, @return_value);");

                    foreach (IDataParameter sp in sc.Parameters)
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
        public static String ParameterValueForSQL(IDataParameter sp, bool failOnObjectTypes=false)
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
                switch (sp.DbType)
                {
                    case DbType.AnsiString:
                    case DbType.AnsiStringFixedLength:
                    case DbType.Date:
                    case DbType.DateTime:
                    case DbType.DateTime2:
                    case DbType.Guid:
                    case DbType.StringFixedLength:
                    case DbType.String:
                    case DbType.Time:
                    case DbType.Xml:
                        quote = true;
                        if (retval == null)
                        {
                            retval = sp.Value.ToString();
                        }
                        break;
                    case DbType.Boolean:
                    case DbType.Byte:
                    case DbType.Currency:
                    case DbType.DateTimeOffset:
                    case DbType.Decimal:
                    case DbType.Double:
                    case DbType.Int16:
                    case DbType.Int32:
                    case DbType.Int64:
                    case DbType.SByte:
                    case DbType.Single:
                    case DbType.UInt16:
                    case DbType.UInt32: 
                    case DbType.UInt64:
                    case DbType.VarNumeric:
                        if (retval == null)
                        {
                            retval = CleanNumber(sp.Value.ToString());
                        }
                        break;
                    case DbType.Binary:

                        // this is probably provider specific, so just pass some value that is hopefully unique for the data
                        if (failOnObjectTypes)
                        {
                            throw new InvalidCastException("Unhandled data type.");
                        }
                        retval = String.Format("[Binary data {0}]"+sp.Value.GetHashCode());
                        break;
                    case DbType.Object:
                        if (failOnObjectTypes)
                        {
                            throw new InvalidCastException("Unhandled data type.");
                        }
                        retval = String.Format("[Object object {0}]"+sp.Value.GetHashCode());
                        break;
                    default:

                        throw new InvalidCastException("Unsupported data type for inline parameter substitution.");
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
                foreach (IDataParameter sp in parms)
                {
                    result = result.Replace(sp.ParameterName, ParameterValueForSQL(sp));
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
                    throw new InvalidCastException("Unsupported type for TryCleanNumber");
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

            HashSet<string> parameterNames = new HashSet<string>(IQ.Config.ParmNameComparer);
            if (!string.IsNullOrEmpty(sql))
            {

                int pos = 0;
                bool quoted = false;
                while (pos < sql.Length)
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
                                    parmName = (sql.Substring(curIndex));
                                }
                                else
                                {
                                    int len = nextIndex - curIndex;
                                    parmName = sql.Substring(curIndex, len);
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
                    pos++;
                }
            }
            return parameterNames;
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
            return sql.IndexOfAny(" (),=<>!.;:\r\n\t".ToCharArray(), startPos);
        }


    }
}
