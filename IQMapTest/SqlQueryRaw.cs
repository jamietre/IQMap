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
            var testQ = new SqlQueryRaw("someField", new object[] {123});
            Assert.AreEqual("someField=@val", testQ.GetQuery());
            Assert.AreEqual("@val", testQ.Parameters.First().ParameterName, "Parm name was correct when created for a simple field compariosn");
            Assert.AreEqual(123, testQ.Parameters.First().Value, "Parm value correct");

            testQ = new SqlQueryRaw("someField=@valParm", new object[] { "abc"});
            Assert.AreEqual("someField=@valParm",testQ.GetQuery() );
            Assert.AreEqual("@valParm", testQ.Parameters.First().ParameterName, "Parm name was correct when created for a simple field compariosn");
            Assert.AreEqual( "abc",testQ.Parameters.First().Value, "Parm value correct");

            string query = "someField=@val1 and someOtherField=@val2";
            testQ = new SqlQueryRaw(query, new object[] {"@val2","abc","@val1",678});
            Assert.AreEqual(testQ.GetQuery(), query);
            Assert.AreEqual(testQ.Parameters.ElementAt(1).ParameterName, "@val1", "Parm name was correct when created for a simple field compariosn");
            Assert.AreEqual(testQ.Parameters.ElementAt(1).Value, 678, "Parm value correct");

        }
        [TestMethod]
        public void NullHandling()
        {

            /// Nulls should be stripped
            var testQ = new SqlQueryRaw("someField", new object[] { null });
            Assert.AreEqual("someField", testQ.GetQuery());
            Assert.AreEqual(testQ.Parameters.Count(), 0);


             testQ = new SqlQueryRaw("someField=@p1 and someField2=@p2",new object[] { 123,null});
            Assert.AreEqual("someField=@p1 and someField2=@p2", testQ.GetQuery());
            Assert.AreEqual(testQ.Parameters.Count(), 2);

            Assert.AreEqual(123, testQ.Parameters.ElementAt(0).Value);
            Assert.AreEqual(null,testQ.Parameters.ElementAt(1).Value);
        }
    }
}
