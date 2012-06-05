using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMap;

namespace IQMap.Tests
{
    public class TestObjectDefault
    {
        public TestObjectDefault()
        {

        }

        [IQPrimaryKey]
        public int DefaultPK { get; protected set; }
        public string FirstName { get; set; }
        public float HowMuch { get; set; }
        public DateTime UpdateDate { get; set; }
        public long SomeNumber { get; set; }
        public string NotAField { get; set; }
    }

}
