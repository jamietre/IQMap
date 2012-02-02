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
        string GetQuery();
        IEnumerable<IDataParameter> Parameters { get; }
        /// <summary>
        /// A list of key/value pairs cross referencing property or field names, to table columns
        /// </summary>
        /// <param name="fieldMap"></param>
        void AddFieldMap(IEnumerable<KeyValuePair<string, string>> fieldMap);
        /// <summary>
        /// Implementation-dependent identifier for the source of the data. For sql, this would be a table name.
        /// </summary>
        string From { get; set; }  
    }
}