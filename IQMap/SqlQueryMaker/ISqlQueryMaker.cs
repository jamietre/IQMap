using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMap.SqlQueryBuilder.Impl;

namespace IQMap.SqlQueryBuilder
{
    public interface ISqlQueryMaker : ISqlQuery
    {
        new QueryType QueryType { get;  }
       
        string TableName { get; set; }
        string PrimaryKey { get; set; }

        IWhereClause Where { get; set; }
        IOrderByClause OrderBy { get; set; }

        /// <summary>
        ///  Indicates whether this query has been explicitly set as "select all" using an asterisk, or if its a default 
        /// </summary>
        bool SelectAll { get; }
        string Select { get; set; }
        string From { get; set; }
        string GroupBy { get; set; }
        string Having { get; set; }

        string UpdateSet { get; }
        string InsertFields { get; }
        string InsertValues { get; }

        new IEnumerable<IDataParameter> Parameters { get; }

        /// <summary>
        /// Returns true if this is a valid query.
        /// </summary>
        bool IsComplete { get; }
        bool IsDirty { get; }
        void Touch();
        void Clean();

        /// <summary>
        /// Add a parameter to the query definition
        /// </summary>
        /// <param name="parameter"></param>
        void AddParameter(IDataParameter parameter);

        /// <summary>
        /// Add one or more parameters to the query definition
        /// </summary>
        /// <param name="parameter"></param>
        void AddParameter(IEnumerable<IDataParameter> parameters);

        /// <summary>
        /// Map a list of key/value pairs of field names to SQL server column names. 
        /// </summary>
        /// <param name="map"></param>
        void AddFieldMap(IEnumerable<KeyValuePair<string, string>> map);

        /// <summary>
        /// Add a translation from field name to SQL server column names
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        void AddFieldMap(string fieldName, string value);
        
        /// <summary>
        /// Add a name/value pair for an UPDATE clause
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        ISqlQuery AddUpdateData(string fieldName, object value);

        string GetFieldMap(string name);

        /// <summary>
        /// Return a new, unique parameter name that can be used with this query. if "basedOn" is 
        /// passed, it will attempt to use that as the name, or a variant if it has already been used.
        /// </summary>
        /// <param name="basedOn"></param>
        /// <returns></returns>
        string GetNewParameterName(string basedOn = "");
        string GetQuery(SqlClause clause);

        /// <summary>
        /// Return a deep clone of the query defintion
        /// </summary>
        /// <returns></returns>
        new ISqlQueryMaker Clone();

        /// <summary>
        /// Clone the query, casting it as a new type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        ISqlQueryMaker Clone(QueryType type);
    }
}
