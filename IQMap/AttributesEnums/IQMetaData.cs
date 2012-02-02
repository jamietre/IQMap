using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IQMap
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class IQMetaData: Attribute 
    {
        /// <summary>
        /// The FROM clause used to create SQL select statemeents.
        /// </summary>
        ////public string SelectFrom;

        public string TableName;
        public string FieldMap;
        /// <summary>
        /// When true, class properties are excluded (rather than included) by default. Only properties identified with
        /// DBField will be included. 
        /// </summary>
        public bool ExcludeProperties;
        /// <summary>
        /// When true, use "*" instead of generating fields for select statements.
        /// </summary>
        public bool SelectAll;
        public IQMetaData()
        {

        }
        public IQMetaData(string tableName)
        {
            TableName = tableName;
        }
    }
}