using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = NUnit.Framework.Assert;
using IQMapTest.Mocks;
using IQMap;
using IQMap.Implementation;

namespace IQMapTest
{
    [TestClass]
    public class SqlQueryRaw_
    {
        [TestMethod]
        public void Basic()
        {
            var testQ = new SqlQueryRaw("someField", 123);
            Assert.AreEqual("someField=@1", testQ.GetQuery());
            Assert.AreEqual("@1", testQ.Parameters.First().ParameterName, "Parm name was correct when created for a simple field compariosn");
            Assert.AreEqual(123, testQ.Parameters.First().Value, "Parm value correct");

            testQ = new SqlQueryRaw("someField=@valParm", "abc");
            Assert.AreEqual("someField=@valParm",testQ.GetQuery() );
            Assert.AreEqual("@valParm", testQ.Parameters.First().ParameterName, "Parm name was correct when created for a simple field compariosn");
            Assert.AreEqual( "abc",testQ.Parameters.First().Value, "Parm value correct");

            string query = "someField=@val1 and someOtherField=@val2";
            testQ = new SqlQueryRaw(query, "@val2","abc","@val1",678);
            Assert.AreEqual(testQ.GetQuery(), query);
            Assert.AreEqual(testQ.Parameters.ElementAt(1).ParameterName, "@val1", "Parm name was correct when created for a simple field compariosn");
            Assert.AreEqual(testQ.Parameters.ElementAt(1).Value, 678, "Parm value correct");

        }
        [TestMethod]
        public void NullHandling()
        {

            var testQ = new SqlQueryRaw("someField",null);
            Assert.AreEqual("someField=@1", testQ.GetQuery());
        }
    }
}
