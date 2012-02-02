using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IQMap
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class IQField: Attribute 
    {
        public string SqlName;
        public bool IsPrimaryKey;
        /// <summary>
        /// When true, data will be loaded from the selection set, but never updated.
        /// </summary>
        public bool ReadOnly;
        /// <summary>
        /// When true, null values will be mapped to the data type default in 
        /// </summary>
        public bool IgnoreNull;
    }
}