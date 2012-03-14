using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IQMapTest.Mocks;
using IQMap;
using IQMap.Implementation;
using Assert = NUnit.Framework.Assert;
using TC = IQMapTest.TestConfig;

namespace IQMapTest
{
    [TestClass]
    public class Types
    {
        string selectSql = @"SELECT 'The quick brown fox? What a fool! ""I''m faster,"" I would''ve said.' as stringfield,
                cast(12345 as int) as intfield,
                cast(3.1415 as float) as floatfield,
                cast(10.123 as decimal(10,5)) as decimalfield,
                convert(varbinary(max), 0xff01323040418081) as bytearrayfield,
                cast(1 as bit) as booleanfield,
                cast('12/12/1922 1:00 PM' as datetime) as datetimefield,
                cast(null as datetime) as datetimenullable,
                cast(2 as int) as enumfield,
                'some read only data' as readonlyfield";
        [TestMethod]
        public void BasicTypes()
        {


            var test = IQ.Query(selectSql).MapNext<TestObjectTypes>();

            Assert.AreEqual("The quick brown fox? What a fool! \"I'm faster,\" I would've said.", test.StringField);
            Assert.AreEqual(12345, test.IntField);
            Assert.AreEqual( 3.1415f,test.FloatField);
            Assert.AreEqual(10.123m,test.DecimalField);
            Assert.AreEqual(new byte[] { 0xff, 0x01, 0x32, 0x30, 0x40, 0x41, 0x80, 0x81 }, test.ByteArrayField);
            Assert.AreEqual(true,test.BooleanField );
            Assert.AreEqual(DateTime.Parse("12/12/1922 1:00 PM"),test.DateTimeField);
            Assert.AreEqual(null, test.DateTimeNullableField);

            //Assert.AreEqual('A',te
            
        }
        [TestMethod]
        public void TestStrongType()
        {
            var guid = Guid.NewGuid();
            IEnumerable<Dog> dog = IQ.Connection.Select<Dog>("select Age = @Age, Id = @Id", new { Age = (int?)null, Id = guid });

            Assert.AreEqual(1,dog.Count());
            
            var firstDog = dog.First();
            Assert.AreEqual(null, firstDog.Age);
            Assert.AreEqual(guid, firstDog.Id);
        }
        [TestMethod]
        public void SaveTypes()
        {
            var test = IQ.Query(selectSql).MapNext<TestObjectTypes2>();
            test.EnumField = TestEnum.Val3;
            test.DateTimeNullableField = null;
            test.StringField = "sometext";
            test.ReadOnlyField = "Changed";
            IQ.Save(test);

            Assert.AreEqual("UPDATE testTable SET StringField=@StringField,EnumField=@EnumField WHERE IntField=@IntField", TC.Controller.LastQuery);
            Assert.AreEqual("sometext",TC.Controller.LastParameters.ElementAt(0).Value);
            Assert.AreEqual(3,TC.Controller.LastParameters.ElementAt(1).Value);
            Assert.AreEqual(12345, TC.Controller.LastParameters.ElementAt(2).Value);

            Assert.AreEqual(IQEventType.Save | IQEventType.Update | IQEventType.After, test.CapturedEvent, "Event handler worked.");
            Assert.AreEqual(false, test.CapturedDbData.IsDirty("readonlyfield"), "Event handler worked 2.");
            Assert.AreEqual(true, test.WasDirty, "Event handler worked 3.");


            DateTime testDateTime = DateTime.Parse("2/12/1922 4:22 AM");

            IQ.Config.OptimizeParameterNames = true;
            test.EnumField = TestEnum.Val2;
            test.DateTimeNullableField = testDateTime;
            test.StringField = "changed again";
            IQ.Save(test);

            Assert.AreEqual("UPDATE testTable SET StringField=@p0,DateTimeNullableField=@p1,EnumField=@p2 WHERE IntField=@p3", TC.Controller.LastQuery);
            Assert.AreEqual("changed again", TC.Controller.LastParameters.ElementAt(0).Value);
            Assert.AreEqual(testDateTime, TC.Controller.LastParameters.ElementAt(1).Value);
            Assert.AreEqual(2, TC.Controller.LastParameters.ElementAt(2).Value);

        }
    }
}
