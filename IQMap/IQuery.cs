using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace IQMap
{
    public interface IQuery
    {
        QueryType QueryType { get; }

        string Select { get; set; }
        string From { get; set; }
        string Where { get; set; }
        string OrderBy { get; set; }
        string GroupBy { get; set; }
        string Having { get; set; }

        /// <summary>
        /// Returns true if this is a valid query.
        /// </summary>
        bool IsComplete { get; }

        IEnumerable<IDataParameter> Parameters { get; }

        string GetQuery();
        /// <summary>
        /// A list of key/value pairs cross referencing property or field names, to table columns
        /// </summary>
        /// <param name="fieldMap"></param>
        void AddFieldMap(IEnumerable<KeyValuePair<string, string>> fieldMap);

        ISqlQuery AddWhere(string condition);
        ISqlQuery AddParameter(IDataParameter parameter);
        IQuery Clone();
        IQuery Clone(QueryType type);
    }
}