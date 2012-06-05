using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using IQMap.SqlQueryBuilder;

namespace IQMap
{
    public interface ISqlQuery
    {
        QueryType QueryType { get; }

        /// <summary>
        /// Causes an exception if the query does not match type
        /// </summary>
        /// <param name="type"></param> 
        void ExpectType(QueryType type);

        /// <summary>
        /// Return the pseudo-query (an SQL-like output which is unique for the current definition,
        /// but may not actually work with an SQL server). In practice it is mostly ANSi except for
        /// how FirstRow and TotalRows are constructed which are not defined in the spec, it uses
        /// the MySql format of LIMIT. But the final query should be built by a language-specific
        /// implemenation of SqlDataStorageController.
        /// </summary>
        /// <returns></returns>
        string GetQuery();

        /// <summary>
        /// Return the DataParameters associated with this query
        /// </summary>
        IEnumerable<IDataParameter> Parameters { get; }
        
        /// <summary>
        /// The first row which should be returned
        /// </summary>
        int FirstRow
        {
            get;
            set;
        }

        /// <summary>
        /// The total number of rows that should be returned
        /// </summary>
        int TotalRows { get; set; }

        ISqlQuery Clone();
    }
  

 
}