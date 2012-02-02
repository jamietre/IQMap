using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.Implementation
{
    public class ParameterWhereCriterion : WhereCriterion,IWhereCriterion
    {
        public ParameterWhereCriterion()
        {

        }
        public ParameterWhereCriterion(string fieldName, ComparisonType comparisonType) :
            base(fieldName, "@" + fieldName, comparisonType)
        {

        }
        public ParameterWhereCriterion(string fieldName,string parameterName, ComparisonType comparisonType) :
            base(fieldName, "@" + parameterName, comparisonType)
        {

        }

        public override object Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                if (!(value is string))
                {
                    throw new Exception("Can't create a Parameter criterion with anything other than a string value");
                }
                _Value = (string)value;
                SqlValue = new SqlValueParm((string)_Value);
            }
        }
        
    }
}
