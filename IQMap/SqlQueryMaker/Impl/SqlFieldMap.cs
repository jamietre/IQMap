using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap.SqlQueryBuilder;

namespace IQMap.SqlQueryBuilder.Impl
{
    public class SqlFieldMap : ISqlFieldMap
    {

        /// <summary>
        /// A map of field names to fully-qualified SQL names
        /// </summary>
        public IDictionary<string, string> FieldMap
        {
            get
            {
                if (_FieldMap == null)
                {
                    _FieldMap = new Dictionary<string, string>();
                }
                return _FieldMap;
            }
        }
        protected IDictionary<string, string> _FieldMap = null;
        public string SqlFieldName(string fieldName)
        {
            string sqlField = fieldName;
            if (_FieldMap != null)
            {
                if (!_FieldMap.TryGetValue(fieldName.ToLower(), out sqlField))
                {
                    sqlField = fieldName;
                };
            }
            return sqlField;
        }
        public void Clear()
        {
            _FieldMap = null;
        }
        public void SetFieldMap(IEnumerable<KeyValuePair<string, string>> map)
        {
            foreach (var kvp in map)
            {
                FieldMap[kvp.Key.ToLower()] = kvp.Value;
            }
        }
        public void AddMapping(string fieldName, string sqlFieldName)
        {
            FieldMap[fieldName] = sqlFieldName;
        }
        public ISqlFieldMap Clone()
        {
            SqlFieldMap clone = new SqlFieldMap();
            if (_FieldMap != null)
            {
                clone.SetFieldMap(FieldMap);
            }
            return clone;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            if (_FieldMap != null)
            {
                foreach (var kvp in _FieldMap)
                {
                    yield return kvp;
                }
            }
            yield break;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
