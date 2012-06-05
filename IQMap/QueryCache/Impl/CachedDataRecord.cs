using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQObjectMapper;

namespace IQMap.QueryCache.Impl
{
    public class CachedDataRecord: IDataRecord
    {
        public CachedDataRecord(IDataRecord record)
        {
            var fields = record.FieldCount;

            Fields = new string[fields];
            Values = new object[fields];
            DataTypeNames = new string[fields];
            FieldNames = new Dictionary<string, int>();
            for (var i = 0; i < fields; i++)
            {
                Fields[i] = record.GetName(i);
                FieldNames[Fields[i]] = i;
                Values[i] = record.GetValue(i);
                DataTypeNames[i] = record.GetDataTypeName(i);
            }
        }
        string[] Fields;
        object[] Values;
        string[] DataTypeNames;
        protected int _SizeEstimate=0;
        IDictionary<string, int> FieldNames;
        public int SizeEstimate()
        {
            if (_SizeEstimate == 0)
            {
                int size = 0;
                for (var i = 0; i < Fields.Length; i++)
                {
                    int elSize =Types.GetSizeEstimate(Values[i]);
                    if (elSize > 0)
                    {
                        size += elSize;
                    }
                }
                if (size!=0) {
                    _SizeEstimate = size;
                } else {
                    _SizeEstimate=-1;
                }

            }
            return _SizeEstimate;

        }
        public int FieldCount
        {
            get { return Fields.Length; }
        }

        public bool GetBoolean(int i)
        {
            return (bool)Values[i];
        }

        public byte GetByte(int i)
        {
            return (byte)Values[i];
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            byte[] bytes = (byte[])Values[i];
            long index;
            for (index = fieldOffset; index < Math.Min(length,bytes.Length); index++)
            {
                buffer[bufferoffset++] = bytes[index];
            }
            return index - fieldOffset;
        }

        public char GetChar(int i)
        {
            return (char)Values[i];
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            char[] bytes = (char[])Values[i];
            long index;
            for (index = fieldoffset; index < Math.Min(length, bytes.Length); index++)
            {
                buffer[bufferoffset++] = bytes[index];
            }
            return index - fieldoffset;
        }

        public IDataReader GetData(int i)
        {
            throw new InvalidOperationException("Nested data readers are not supported by CachedDataReader.");
        }

        public string GetDataTypeName(int i)
        {
            return DataTypeNames[i];
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)Values[i];
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)Values[i];
        }

        public double GetDouble(int i)
        {
            return (double)Values[i];
        }

        public Type GetFieldType(int i)
        {
            return (Type)Values[i];
        }

        public float GetFloat(int i)
        {
            return (float)Values[i];
        }

        public Guid GetGuid(int i)
        {
            return (Guid)Values[i];
        }

        public short GetInt16(int i)
        {
            return (short)Values[i];
        }

        public int GetInt32(int i)
        {
            return (int)Values[i];
        }

        public long GetInt64(int i)
        {
            return (long)Values[i];
        }

        public string GetName(int i)
        {
            return Fields[i];
        }

        public int GetOrdinal(string name)
        {
            int index;
            if (FieldNames.TryGetValue(name, out index))
            {
                return index;
            }
            else
            {
                return -1;
            }
        }

        public string GetString(int i)
        {
            return (string)Values[i];
        }

        public object GetValue(int i)
        {
            return Values[i];
        }

        public int GetValues(object[] values)
        {
            int maxEl= Math.Min(FieldCount,values.Length);
            for (var i=0;i<maxEl;i++) {
                values[i]=Values[i];
            }
            return maxEl;
        }

        public bool IsDBNull(int i)
        {
            return Values[i] == null || Values[i] == System.DBNull.Value;
        }

        public object this[string name]
        {
            get { return Values[FieldNames[name]]; }
        }

        public object this[int i]
        {
            get { return Values[i]; }
        }
    }
}
