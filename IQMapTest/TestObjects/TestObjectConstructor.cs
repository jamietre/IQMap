using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMapTest.Mocks;
using IQMap;

namespace IQMapTest
{
    public class TestObjectConstructor 
    {
        [IQConstructor]
        private static void IQConstructor(IQClassData data)
        {

            var query = data.Query;
            query.Select = "PK,FirstName,HowMuch";
            query.From = "testTableConstructorView";
            query.Where = "defaultWhere=1";

            data.PrimaryKey = "PK";
            data.TableName = "testTableConstructor";

        }

        public int PK { get; protected set; }
        public string FirstName { get; set; }
        public float HowMuch { get; set; }
        public DateTime UpdateDate { get; set; }
        public long SomeNumber { get; set; }
        
        [IQIgnore]
        public string NotAField { get; set; }
    }
}
