using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMap;

namespace IQMap.Tests
{
    [IQClass(TableName="testTable",ExcludeByDefault=true)]
    public class TestObject : IQObject
    {
        public TestObject()
        {
           
        }

        [IQField(PrimaryKey= true)]
        public int PK { get; protected set; }

        [IQField]
        public string FirstName { get; set; }

        [IQField]
        public float HowMuch { get; set; }
        
        [IQField]
        public DateTime UpdateDate { get; set; }
        
        [IQField] 
        public long SomeNumber { get; set; }

        public string NotAField { get; set; }


    }
}
