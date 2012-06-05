using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.SqlQueryBuilder
{
    public interface IParameterCollection: ICollection<IDataParameter>
    {
        ISqlQueryMaker Owner { get; set; }
        IParameterCollection Clone();
        
        void Add(IEnumerable<IDataParameter> parameterList);
        void Add(string name, object value);
        bool ContainsKey(string name);
        /// <summary>
        /// Return a new, unique parameter name (including @) that can be used with this query. if "basedOn" is 
        /// passed, it will attempt to use that as the name, or a variant if it has already been used.
        /// </summary>
        /// <param name="basedOn"></param>
        /// <returns></returns>
        string GetNewParameterName(string basedOn = "");
    }
}
