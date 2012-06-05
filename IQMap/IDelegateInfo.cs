using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace IQMap
{
    public interface IDelegateInfo: IQObjectMapper.IDelegateInfo
    {
        /// <summary>
        /// The name of the SQL column (if different from the field name)
        /// </summary>
        string SqlName { get; set; }
        /// <summary>
        /// This field represents the data table primary key
        /// </summary>
        bool IsPrimaryKey { get; set; }
        /// <summary>
        /// The estimated size (in byes) of the data structure
        /// </summary>
        int Size { get; set; }
        /// <summary>
        /// When true, null values will be mapped to the default value of a target
        /// that doesn't support null
        /// </summary>
        bool IgnoreNull { get; set; }
        /// <summary>
        /// When true, the column cannot be updated in the data table.
        /// </summary>
        bool IsSqlReadOnly { get; set; }
    }
}
