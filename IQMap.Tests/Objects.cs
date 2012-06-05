using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IQMap;
using IQMap.Impl;
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;
using IQObjectMapper;

using Assert = NUnit.Framework.Assert;
using TC = IQMap.Tests.TestConfig;

namespace IQMap.Tests
{
    [TestClass]
    public class Objects
    {
        [TestMethod]
        public void Basic()
        {
            //Assert.Throws<Exception>(() => {
            //    TestObjectBugs testBugs = new TestObjectBugs();
                
            //}, "Can't create instance of primary-keyless object");

            var test = new TestObject();
            IQ.Track(test);

            Assert.IsFalse(test.IsDirty(), "New object isn't dirty");
            Assert.IsTrue(test.IsNew(), "New object is new");

            test.FirstName = "Jamie";
            Assert.IsTrue(test.IsDirty("FirstName"), "Changing name dirtied it");
            Assert.IsTrue(test.IsDirty(), "Changing a field dirtied the whole object");

            int dirtyCount=0;
            foreach (var item in test.DirtyFieldNames()) {
                dirtyCount++;
                Assert.AreEqual("FirstName",item,"The field that's dirty is firstname");
            }
            Assert.AreEqual(1, dirtyCount, "There was one dirty field");


        }

        [TestMethod]
        public void DefaultType()
        {

            var test = new TestObjectDefault();
            var data = IQ.MapperCache.GetOrCreateObjectData(test);

            Assert.IsTrue(data.ClassInfo.PrimaryKeyField.Name == "DefaultPK");
            Assert.AreEqual(data.ClassInfo.FieldNames.Count(), 6, "Has five fields");

            data.SetPrimaryKey(123);
            Assert.AreEqual(123, test.DefaultPK, "PK was set");
       }
        [TestMethod]
        public void DefaultTypeIgnore()
        {

            var test = new TestObjectDefaultIgnore();
            var data = IQ.MapperCache.GetOrCreateObjectData(test);

            Assert.AreEqual(data.ClassInfo.FieldNames.Count(), 5, "Has five fields");
        }
        [TestMethod]

        public void LoadQuery()
        {


            var test = IQ.RunSql(@"select 1 as PK, 'jamie' as firstname, cast('1/1/2012 12:12:00' as datetime) as updatedate, 
                14 as somenumber, 122.29 as howmuch").MapNext<TestObject>();

            Assert.AreNotEqual(default(int), test.PK);
            Assert.AreNotEqual(default(string), test.FirstName);
            Assert.AreNotEqual(default(DateTime), test.UpdateDate);
            Assert.AreNotEqual(default(long), test.SomeNumber);
            Assert.AreNotEqual(default(float), test.HowMuch);
        }


        // See notes:  http://stackoverflow.com/q/8593871
        // Our implementation is different. We only deal with properties having a public getter (unless explicitly marked)
        [TestMethod]
        public void TestAbstractInheritance()
        {
            string sql = "select 1 Internal,2 ProtectedSet,3 [Public],"
                    + "4 Concrete,5 PrivateGetSet, 6 ProtectedGet";
           
            var order = IQ.Query<ConcreteOrder>(sql)
                        .First();

            Assert.AreEqual(1, order.Internal);
            Assert.AreEqual(2, order.ProtectedVal);
            Assert.AreEqual(3, order.Public);
            Assert.AreEqual(4, order.Concrete);
            Assert.AreEqual(5, order.PrivateGetSetVal);
            Assert.AreEqual(6, order.ProtectedGetVal);

        }

        [TestMethod]
        public void Constructors()
        {
            TestObjectConstructor test= new TestObjectConstructor();
            var classInfo = IQ.MapperCache.GetClassInfo(test.GetType());

            Assert.AreEqual("testTableConstructor", classInfo.Query.TableName);
            ISqlQueryMaker query = classInfo.GetQuery();
            Assert.AreEqual("testTableConstructorView", query.From);

            try
            {
                test = IQ.From<TestObjectConstructor>(12345).First();
            }
            catch { }

            Assert.AreEqual("SELECT TOP 1 PK,FirstName,HowMuch FROM testTableConstructorView WHERE (defaultWhere=1) AND pk=@pk", TC.Controller.LastQuery.GetQuery());
            // TODO: Add tests for complex queries based on the oroginal query object
        }
        [TestMethod]
        public void Copy()
        {
            var test = new TestObject();
            IQ.MapperCache.CreateObjectData(test);

            test.FirstName = "Jamie";
            TestObject testClone = (TestObject)test.Clone();
            Assert.IsTrue(test.IsDirty());
            Assert.IsFalse(testClone.IsDirty());
            Assert.AreEqual("Jamie", test.FirstName);
        }
    }
}
