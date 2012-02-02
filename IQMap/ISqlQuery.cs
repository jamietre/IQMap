using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMap.Implementation;

namespace IQMap
{
  
    public interface ISqlQuery: IQuery
    {

        //SqlFieldMap sqlFieldMap { get; }
        void AddFieldMap(string fieldName, string value);
        void ClearFieldMap();
        string Select {get;set;}
        
        /// <summary>
        /// A list of fields in an Insert or Update query
        /// </summary>
        IEnumerable<KeyValuePair<string, SqlValue>> UpdateData { get; }

        string UpdateSet { get; }
        string InsertFields { get; }
        string InsertValues { get; }
        string Where {get;set;}
        string OrderBy { get; set; }
        string GroupBy { get; set; }

        /// <summary>
        /// Add a field/value pair to be included in an update query.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        ISqlQuery AddUpdateData(string fieldName, object value);

        ISqlQuery AddParameter(IDataParameter parameter);
        ISqlQuery AddParameter(string name, object value);
        ISqlQuery AddParameter(IEnumerable<IDataParameter> parameterList);
        ISqlQuery AddWhere(string condition);
        ISqlQuery AddWhere(string fieldName,object value);
        ISqlQuery AddWhere(string fieldName, object value, ComparisonType comparisonType);
        ISqlQuery AddWhereParam(string fieldName, object value);
        ISqlQuery AddWhereParam(string fieldName, string paramName, object value);
        ISqlQuery AddWhereParam(string fieldName, object value, ComparisonType comparisonType);
        ISqlQuery AddWhereParam(string fieldName, string paramName, object value, ComparisonType comparisonType);
        ISqlQuery AddWhereOr(string condition);
        ISqlQuery AddWhere(IWhere condition);
        ISqlQuery AddWhereOr(IWhere condition);
        ISqlQuery AddSort(string field, SortOrder order);
        ISqlQuery AddSort(string field, SortOrder order, SortPriority priority);
        ISqlQuery AddSort(string orderClause);
        ISqlQuery AddSort(string orderClause, SortPriority priority);
        ISqlQuery Clone();
        string MapField(string fieldName);
        //string GetQuery();
        string SqlAuditString();
        void Clear();
        void Clean();
        bool IsDirty { get; }
        event EventHandler Dirty;
    }

}
