using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IQMap
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IQClass: Attribute 
    {
        public IQClass()
        {

        }

        public IQClass(string tableName, string primaryKey)
        {
            TableName = tableName;
            PrimaryKey = primaryKey;
        }
        /// <summary>
        /// Never cache queries based on this class
        /// </summary>
        public bool NeverCache;
        
        /// <summary>
        /// The name of the SQL table to which this class is bound
        /// </summary>
        public string TableName;
        
        /// <summary>
        /// The primary key column for this table.
        /// </summary>
        public string PrimaryKey;
        
        /// <summary>
        /// When true, class properties are excluded (rather than included) by default. Only properties identified with
        /// DBField will be included. 
        /// </summary>
        public bool ExcludeByDefault;
        
        /// <summary>
        /// When true, use "*" instead of generating fields for select statements.
        /// </summary>
        public bool SelectAll;

    }
}