using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap.Implementation;

namespace IQMap
{
    public interface IDBClassInfo
    {
        /// <summary>
        /// Table or other db-specific identifier for the data
        /// </summary>
        string TableName { get;  }
        /// <summary>
        /// When true, queryies will not attempt to select individual fields but use *.
        /// </summary>
        bool SelectAll { get; }
        /// <summary>
        /// The base query
        /// </summary>
        ISqlQuery Query(QueryType type);

        object PrimaryKeyDefaultValue { get; }

        IList<IDBFieldInfo> FieldInfo { get; }
        IList<string> FieldNames { get; }
        IList<string> SqlNames { get; }

        /// <summary>
        /// Map of field (property) names to SQL names
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> FieldNameMap {get;}

        
        IDBFieldInfo this[string fieldName] {get;}
        IDBFieldInfo this[int fieldIndex] {get;}
        IDBFieldInfo PrimaryKey { get;  }

        int FieldIndex(string fieldName);
        string SqlName(string fieldName);
        bool HasField(string fieldName);
        IDBFieldInfo GetFieldInfo(string fieldName);
        bool TryGetFieldInfo(string fieldName, out IDBFieldInfo info);
        

    }
}
