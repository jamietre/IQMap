using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IQMapTest.Mocks;
using IQMap;
using IQMap.Implementation;
using System.Diagnostics;

namespace IQMapTest
{
    [TestClass]
    public class TestConfig
    {
        public static TestDataStorageController Controller;
        [AssemblyInitialize]
        public static void TestRunSetup(TestContext context)
        {
            Controller = new TestDataStorageController();
            IQ.Config.DataController = new SqlDataController(Controller);
            //IQ.Config.DefaultConnectionString = "Data Source=LENOVO3\\SQLEXPRESS;Initial Catalog=tempdb;Integrated Security=True";
            IQ.Config.DefaultConnectionString = "Data Source=VMSQLMD01;Initial Catalog=tempdb;Integrated Security=True";
        }
        #region static methods

        public static string GenerateSqlSelect(Type type, int totalRows)
        {
            SetRandomSeed();
            SetFirstRowData();

            IDBClassInfo info = IQ.GetClassInfo(type);

            string sql = "";

            for (int row=0;row<totalRows;row++) {
                string rowSelect = "";
                foreach (var item in info.FieldInfo)
                {
                    object value;
                    if (row == 0)
                    {
                        value = GetAsSqlSelect(SampleData[item.Type]);
                    }
                    else
                    {
                        value = GetRandomSQLData(item.Type);
                    }
                    rowSelect += (rowSelect == "" ? "" : ",") + value + " as " + item.SqlName;
                }
                rowSelect += "," + row.ToString() + " as rowOrder";
                sql += (sql==""?"":" union ") + "select " + rowSelect ;
            }
            return sql + " order by rowOrder";
        }
        private static Random _Rnd;
        private static Random Rnd
        {
            get
            {
                return _Rnd;
            }
        }
        // Ensure tests are reproducible - use same set of random data each time.
        private static void SetRandomSeed()
        {
            _Rnd = new Random(1);
        }
        private static void SetFirstRowData()
        {
            if (SampleData != null)
            {
                return;
            }

            SampleData = new Dictionary<Type, object>();
            SampleData[typeof(string)] = "The quick brown fox? What a fool! \"I'm faster,\" I would've said.";
            SampleData[typeof(int)] = 12345;
            SampleData[typeof(float)] = 3.1415;
            SampleData[typeof(decimal)] = 10.123;
            SampleData[typeof(byte[])] = new byte[] { 0xff, 0x01, 0x32, 0x30, 0x40, 0x41, 0x80, 0x81 };
            SampleData[typeof(bool)] = true;
            SampleData[typeof(DateTime)] = DateTime.Parse("12/12/1922 1:00 PM");
            SampleData[typeof(long)] = 111222333;
            SampleData[typeof(char)] = 'A';
            SampleData[typeof(DateTime?)] = null;
            SampleData[typeof(Guid)] = new Guid("12345678-8888-8888-2222-222244444444");


        }
        /// <summary>
        /// Returns "cast(nnn as ttttt)"
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetRandomSQLData(Type t)
        {

            string typeString = t.ToString().ToLower();
            int pos = typeString.IndexOf("system.");
            if (pos >= 0)
            {
                typeString = typeString.Substring(pos + 7);
            }
            // return null 1 in 10 times for nullable types
            object data;
            if (Utils.IsNullableType(t) && Rnd.Next(10) == 0)
            {
                data = null;
            }
            else
            {

                switch (typeString)
                {
                    case "int":
                    case "int32":
                    case "int16":
                    case "short":
                        data = Rnd.Next(100000);
                        break;
                    case "int64":
                    case "long":
                        data = Rnd.Next(10000000);
                        break;
                    case "double":
                        data = (Rnd.NextDouble() * (double)100000);
                        break;
                    case "float":
                    case "single":
                        data = ((float)((float)Rnd.NextDouble() * (float)100000));
                        break;
                    case "decimal":
                        data = ((decimal)((decimal)Rnd.NextDouble() * (decimal)100000));
                        break;
                    case "string":
                        {
                            string result = "";
                            int length = Rnd.Next(100);
                            string chars = "abcdeghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()\"\\/'  ";
                            for (int i = 0; i < length; i++)
                            {
                                result += chars[Rnd.Next(chars.Length - 1)];
                            }
                            data = result;
                        }
                        break;
                    case "datetime":
                        data = DateTime.Now.AddDays(Rnd.Next(365) - 180)
                            .AddYears(Rnd.Next(10) - 8)
                            .AddHours(Rnd.Next(24) - 12)
                            .AddMinutes(Rnd.Next(60) - 30);
                        break;
                    case "bool":
                        data = Rnd.Next(1) == 1 ? true : false;
                        break;
                    case "char":
                        data = ((char)Rnd.Next(65535));
                        break;
                    case "byte[]":
                        {
                            int length = Rnd.Next(100);
                            byte[] arr = new byte[length];
                            for (int i = 0; i < length; i++)
                            {
                                arr[i] = (byte)Rnd.Next(255);
                            }
                            data = arr;
                            break;
                        }
                    default:
                        throw new Exception("No idea how to handled data type '" + typeString + "'");
                }
            }
            return GetAsSqlSelect(data);
           
        }
        /// <summary>
        /// Safely map a string to sql
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SqlString(string value)
        {
            return value.Replace("'", "''").Replace("\n", "'+CHAR(13)+CHAR(10)+'").Replace("\t", "'+CHAR(9)+'");
        }
        public static string GetAsSqlSelect(object value) {
            
            string sqlVal;

            if (value == null)
            {
                sqlVal = "NULL";
            }
            else
            {
                Type t = Utils.GetUnderlyingType(value.GetType());
                if (t == typeof(byte[]))
                {
                    sqlVal = "'" + HexStr((byte[])value) + "'";
                }
                else if (t == typeof(string))
                {
                    sqlVal = "'" + SqlString((string)value) + "'";
                }
                else if (t == typeof(char))
                {
                    sqlVal = "CHAR(" + Convert.ToInt16(value).ToString() + ")";
                }
                else if (t == typeof(DateTime))
                {
                    sqlVal = "'" + value.ToString() + "'";

                }
                else
                {
                    sqlVal = value.ToString();
                }
            }
            string final = sqlVal.ToString().Trim();
            if (final=="")
            {
                throw new Exception("null value");
            }
            //Debug.Assert(value.GetType()!= typeof(int) || (int)value != 32730);

            return final=="NULL" ? final : "cast(" + final + " as " + sqlTypeName(value.GetType()) + ")";

        }
        public static string sqlTypeName(Type t)
        { 
             string typeString = t.ToString().ToLower();
            int pos = typeString.IndexOf("system.");
            if (pos >= 0)
            {
                typeString = typeString.Substring(pos + 7);
            }
            switch (typeString)
            {
                case "int":
                case "int32":
                    return "int";
                case "int16":
                case "short":
                    return "smallint";

                case "int64":
                case "long":
                    return "bigint";
                case "double":
                    return "real";
                case "float":
                case "single":
                    return "float";
                case "decimal":
                    return "decimal(10,10)";
                case "string":
                    return "varchar(8000)";
                case "datetime":
                    return "datetime";
                case "bool":
                    return "bit";
                case "char":
                    return "char(1)";
                case "byte[]":
                    return "varbinary(max)";
                case "guid":
                    return "uniqueidentifier";
                default:
                    throw new Exception("Don't know what to do with datatype '" + typeString + "'");
            }

        }
        public static object GetPredefinedData(Type t)
        {
            object data;
            if (SampleData.TryGetValue(t, out data))
            {
                return data;
            }
            else
            {
                throw new Exception("No predefined data for type '" + t.ToString() + "'");
            }

        }
        public static string HexStr(byte[] p)
        {

            char[] c = new char[p.Length * 2 + 2];

            byte b;

            c[0] = '0'; c[1] = 'x';

            for (int y = 0, x = 2; y < p.Length; ++y, ++x)
            {

                b = ((byte)(p[y] >> 4));

                c[x] = (char)(b > 9 ? b + 0x37 : b + 0x30);

                b = ((byte)(p[y] & 0xF));

                c[++x] = (char)(b > 9 ? b + 0x37 : b + 0x30);

            }

            return new string(c);

        }
        public static Dictionary<Type, object> SampleData;
        #endregion
      
    }
}
