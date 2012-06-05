using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap;

namespace IQMap.Tests
{
    [IQClass("testTable","")]
    public class TestObjectBugs : IQObject
    {
        public TestObjectBugs()
        {
          
        }

        // no primary key
        public int PK { get; protected set; }

        [IQField]
        public string FirstName { get; set; }

        [IQField]
        public object RandomObject { get; set; }
        
        [IQField]
        public DateTime UpdateDate { get; set; }
        
        [IQField] 
        public long SomeNumber { get; set; }

        public string NotAField { get; set; }

    }
}
