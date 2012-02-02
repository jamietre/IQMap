using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using IQMap;

namespace IQMapTest.Mocks
{
    public class TestDataReader: IDataReader
    {

        public TestDataReader()
        {
            IsClosed = false;
            Fields = new List<string>();
            Types = new List<Type>();
        }
        #region static methods

        public static IDataReader GenerateFakeDataReader(Type type, int rows)
        {
            SetRandomSeed();
            SetFirstRowData();

            IDBClassInfo info = IQ.GetClassInfo(type);
            
            TestDataReader reader = new TestDataReader();
            foreach (var item in info.FieldInfo)
            {
                reader.AddFieldToMock(item.SqlName, item.Type);
            }
            reader.Generate(rows);
            return reader;
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
            SampleData[typeof(byte[])]=new byte[] {0xff, 0x01, 0x32, 0x30, 0x40, 0x41, 0x80, 0x81};
            SampleData[typeof(bool)] = true;
            SampleData[typeof(DateTime)] = DateTime.Parse("12/12/1922 1:00 PM");
            SampleData[typeof(long)] = 111222333;
            SampleData[typeof(char)] = 'A';
            SampleData[typeof(DateTime?)] = null;
            SampleData[typeof(Guid)] = new Guid("12345678-8888-8888-2222-222244444444");
                                                

        }
        public static object GetRandomData(Type t)
        {

            string typeString = t.ToString().ToLower();
            int pos = typeString.IndexOf("system.");
            if (pos >= 0)
            {
                typeString = typeString.Substring(pos + 7);
            }
            // return null 1 in 10 times for nullable types
            if (Utils.IsNullableType(t) && Rnd.Next(10) == 0)
            {
                return null;
            }
            switch (typeString)
            {
                case "int":
                case "int32":
                    return Rnd.Next(100000);
                case "int16":
                case "short":
                    return (short)Rnd.Next(100000);
                case "int64":
                case "long":
                    return (long)Rnd.Next(100000);
                case "double":
                    return Rnd.NextDouble() * (double)100000;
                case "float":
                case "single":
                    return (float)((float)Rnd.NextDouble() * (float)100000);
                case "decimal":
                    return (decimal)((decimal)Rnd.NextDouble() * (decimal)100000);
                case "string":
                    {
                        string result = "";
                        int length = Rnd.Next(100);
                        string chars = "abcdeghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()\"\\/'  ";
                        for (int i = 0; i < length; i++)
                        {
                            result += chars[Rnd.Next(chars.Length - 1)];
                        }
                        return result;
                    }
                case "datetime":
                    return DateTime.Now.AddDays(Rnd.Next(365) - 180)
                        .AddYears(Rnd.Next(10) - 8)
                        .AddHours(Rnd.Next(24) - 12)
                        .AddMinutes(Rnd.Next(60) - 30);
                case "bool":
                    return Rnd.Next(1) == 1 ? true : false;
                case "char":
                    return (char)Rnd.Next(65535);
                case "byte[]":
                    {
                        int length = Rnd.Next(100);
                        byte[] arr = new byte[length];
                        for (int i = 0; i < length; i++)
                        {
                            arr[i]= (byte)Rnd.Next(255);
                        }
                        return arr;
                    }
                default:
                    return Activator.CreateInstance(t);
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
        private static Dictionary<Type, object> SampleData;

        #endregion

        /// <summary>
        /// Configure this fake datareader using a list of fields from the query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="rows"></param>
        public void AddFieldToMock(string fieldName, Type type)
        {
            Fields.Add(fieldName);
            Types.Add(type);
        }
        /// <summary>
        /// Generate rows of random data. Make sure you've created fields with AddFieldsToMock first.
        /// </summary>
        /// <param name="rows"></param>
        public void Generate(int rows)
        {
            for (int i = 0; i < rows; i++)
            {
                List<object> rowData = new List<object>();
                for (int j = 0; j < Fields.Count; j++)
                {
                    if (i == 0)
                    {
                        rowData.Add(GetPredefinedData(Types[j]));
                    }
                    else
                    {
                        rowData.Add(GetRandomData(Types[j]));
                    }
                }
                Data.Add(rowData);
            }
        }
        protected List<string> Fields;
        protected List<Type> Types;
        protected List<List<object>> Data = new List<List<object>>();

        public int CurrentRow = -1;
        protected int RowCount
        {
            get
            {
                return Data.Count;
            }
        }
        /// <summary>
        /// Set the data for a specified row & field in the fake datareader
        /// </summary>
        /// <param name="row"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        public void SetFakeData(int row, string fieldName, object value)
        {
            Data[row][GetFieldIndex(fieldName)] = value;

        }

        private int GetFieldIndex(string fieldName)
        {
            for (int i = 0; i < Fields.Count; i++)
            {
                if (Fields[i].ToLower() == fieldName)
                {
                    return i;
                }
            }
            return -1;
        }
        public List<object> CurrentRowData
        {
            get
            {
                return Data[CurrentRow];

            }
        }


        public void Close()
        {
            IsClosed = true;
        }

        public int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool IsClosed
        {
            get;
            protected set;
        }

        public bool NextResult()
        {
            if (CurrentRow < RowCount - 1)
            {
                CurrentRow++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Read()
        {
            return NextResult();
        }

        public int RecordsAffected
        {
            get { return 0; }
        }

        public void Dispose()
        {
            Close();
        }

        public int FieldCount
        {
            get { return Fields.Count; }
        }

        public bool GetBoolean(int i)
        {
            return (bool)Convert.ChangeType(CurrentRowData[i], typeof(bool));
        }

        public byte GetByte(int i)
        {
            return (byte)Convert.ChangeType(CurrentRowData[i], typeof(byte));
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)Convert.ChangeType(CurrentRowData[i], typeof(char));
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            return Types[i].ToString();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)Convert.ChangeType(CurrentRowData[i], typeof(DateTime));
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)Convert.ChangeType(CurrentRowData[i], typeof(decimal));
        }

        public double GetDouble(int i)
        {
            return (double)Convert.ChangeType(CurrentRowData[i], typeof(double));
        }

        public Type GetFieldType(int i)
        {
            return Types[i];
        }

        public float GetFloat(int i)
        {
            return (float)Convert.ChangeType(CurrentRowData[i], typeof(float));
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            return (short)Convert.ChangeType(CurrentRowData[i], typeof(int));
        }

        public int GetInt32(int i)
        {
            return (int)Convert.ChangeType(CurrentRowData[i], typeof(int));
        }

        public long GetInt64(int i)
        {
            return (long)Convert.ChangeType(CurrentRowData[i], typeof(int));
        }

        public string GetName(int i)
        {
            return Fields[i];
        }

        public int GetOrdinal(string name)
        {
            return Fields.IndexOf(name.ToLower());
        }

        public string GetString(int i)
        {
            return (string)Convert.ChangeType(CurrentRowData[i], typeof(string));
        }

        public object GetValue(int i)
        {
            return CurrentRowData[i];
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return CurrentRowData[i]==null;
        }

        public object this[string name]
        {
            get { return CurrentRowData[GetOrdinal(name)]; }
        }

        public object this[int i]
        {
            get { return CurrentRowData[i]; }
        }

    }
}
