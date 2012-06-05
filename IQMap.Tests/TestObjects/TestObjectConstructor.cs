using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMap;

namespace IQMap.Tests
{
    public class TestObjectConstructor 
    {
        private void IQConstructor(IClassInfoConstructor data)
        {

            var query = data.Query;
            query.Select = "PK,FirstName,HowMuch";
            query.From = "testTableConstructorView";
            query.Where.Set( "defaultWhere=1");
            query.TableName = "testTableConstructor";

            query.PrimaryKey = "pk";

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
