using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.QueryCache.Impl
{
    /// <summary>
    /// A class for a memory-resident IDataReader
    /// </summary>
    public class CachedDataReader : IDataReader, IList<CachedDataRecord>
    {
        public CachedDataReader()
        {

        }
        /// <summary>
        /// Create a memory-resident datareader building from "source"
        /// </summary>
        /// <param name="source"></param>
        public CachedDataReader(IDataReader source)
        {
            InnerList = new List<CachedDataRecord>();
            while (source.Read())
            {
                InnerList.Add(new CachedDataRecord(source));
            }
            source.Dispose();
            RecordsAffected = source.RecordsAffected;
            CurrentRow = -1;
        }

        private int _Size;
        private int _CurrentRow;
        private IList<CachedDataRecord> _InnerList;
        
        protected virtual IList<CachedDataRecord> InnerList
        {
            get
            {
                return _InnerList;
            }
            set
            {
                _InnerList = value;
            }
        }

        public virtual int CurrentRow
        {
            get
            {
                return _CurrentRow;
            }
            protected set
            {
                _CurrentRow = value;
            }
        }
        protected virtual IDataRecord Current
        {
            get
            {
                return InnerList[CurrentRow];
            }
        }

        public int Size()
        {
            if (_Size == 0)
            {
                for (int i = 0; i < InnerList.Count; i++)
                {
                    int itemSize = InnerList[i].SizeEstimate();
                    _Size += (itemSize< 0 ? 0 : itemSize);
                }
                if (_Size == 0)
                {
                    _Size = -1;
                }
                else
                {
                    _Size = (int)Math.Floor((double)_Size * ((double)InnerList.Count / (double)10)) + 128;
                }
            }
            return _Size;
        }
        public void Reset()
        {
            CurrentRow = -1;
        }
        public void Close()
        {
            CurrentRow = InnerList.Count;
        }

        public int Depth
        {
            get { return 0; }
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public  bool IsClosed
        {
            get
            {
                return CurrentRow >= InnerList.Count;
            }
        }

        public bool NextResult()
        {
            if (IsClosed)
            {
                throw new InvalidOperationException("The datareader is closed.");
            }
            return ++CurrentRow < InnerList.Count;
        }

        public bool Read()
        {
            return NextResult();
        }

        public int RecordsAffected
        {
            get;
            protected set;
        }

        public void Dispose()
        {
            Close();
        }

        public int FieldCount
        {
            get { return InnerList[0].FieldCount; }
        }

        public bool GetBoolean(int i)
        {
            return Current.GetBoolean(i);
        }

        public byte GetByte(int i)
        {
            return Current.GetByte(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return Current.GetBytes(i,fieldOffset,buffer,bufferoffset,length);
        }

        public char GetChar(int i)
        {
            return Current.GetChar(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return Current.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public IDataReader GetData(int i)
        {
            return Current.GetData(i);
        }

        public string GetDataTypeName(int i)
        {
            return Current.GetDataTypeName(i);
        }

        public DateTime GetDateTime(int i)
        {
            return Current.GetDateTime(i);
        }

        public decimal GetDecimal(int i)
        {
            return Current.GetDecimal(i);
        }

        public double GetDouble(int i)
        {
            return Current.GetDouble(i);
        }

        public Type GetFieldType(int i)
        {
            return Current.GetFieldType(i);
        }

        public float GetFloat(int i)
        {
            return Current.GetFloat(i);
        }

        public Guid GetGuid(int i)
        {
            return Current.GetGuid(i);
        }

        public short GetInt16(int i)
        {
            return Current.GetInt16(i);
        }

        public int GetInt32(int i)
        {
            return Current.GetInt32(i);
        }

        public long GetInt64(int i)
        {
            return Current.GetInt64(i);
        }

        public string GetName(int i)
        {
            return Current.GetName(i);
        }

        public int GetOrdinal(string name)
        {
            return Current.GetOrdinal(name);
        }

        public string GetString(int i)
        {
            return Current.GetString(i);
        }

        public object GetValue(int i)
        {
            return Current.GetValue(i);
        }

        public int GetValues(object[] values)
        {
            return Current.GetValues(values);
        }

        public bool IsDBNull(int i)
        {
            return Current.IsDBNull(i);
        }

        public object this[string name]
        {
            get { return Current[name]; }
        }

        public object this[int i]
        {
            get { return Current[i]; }
        }

        #region IList members

        /// <summary>
        /// The IList interface is mostly implemented for conveneince in testing/debugging - not all members may be implemented completely.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>

        public int IndexOf(CachedDataRecord item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, CachedDataRecord item)
        {
            InnerList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            InnerList.RemoveAt(index);
        }

        CachedDataRecord IList<CachedDataRecord>.this[int index]
        {
            get
            {
                return InnerList[index];
            }
            set
            {
                InnerList[index] = value;
            }
        }

        public void Add(CachedDataRecord item)
        {
            InnerList.Add(item);
        }

        public void Clear()
        {
            InnerList.Clear();
        }

        public bool Contains(CachedDataRecord item)
        {
            return InnerList.Contains(item);
        }

        public void CopyTo(CachedDataRecord[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return InnerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(CachedDataRecord item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<CachedDataRecord> GetEnumerator()
        {
            return InnerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }

}
