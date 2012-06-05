using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.SqlQueryBuilder.Impl
{

    public class SqlValueParm : SqlValue
    {
        public SqlValueParm()
        {

        }
        public SqlValueParm(string parmName)
        {
            Value = parmName;
        }

        public override object Value
        {
            get
            {
                return _Value;
            }
            protected set
            {
                SqlDataFormat = SqlDataFormat.Parameter;
                _Value = (string)value;
            }
        }
    }

}
