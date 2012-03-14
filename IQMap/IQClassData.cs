using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap
{
    public class IQClassData
    {
        protected ISqlQuery _Query;
        public ISqlQuery Query
        {
            get
            {
                if (_Query==null) {
                    _Query = IQ.CreateQuery(QueryType.Select);
                }
                return _Query;
            }
            set
            {
                _Query = value;
            }
        }
        public string TableName { get; set; }
        public string PrimaryKey { get; set; }
    }
}
