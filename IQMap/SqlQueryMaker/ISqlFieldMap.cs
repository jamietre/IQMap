using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.SqlQueryBuilder
{
    public interface ISqlFieldMap: IEnumerable<KeyValuePair<string,string>>
    {
        void AddMapping(string fieldName, string sqlFieldName);
        void SetFieldMap(IEnumerable<KeyValuePair<string, string>> map);
        void Clear();
        string SqlFieldName(string fieldName);
        ISqlFieldMap Clone();
    }
}
