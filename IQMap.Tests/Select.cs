using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Dynamic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using IQMap;
using IQMap.Impl;

using Assert = NUnit.Framework.Assert;
using TC = IQMap.Tests.TestConfig;

namespace IQMap.Tests
{
    [TestClass]
    public class Select
    {
        [ClassInitialize]
        public static void Setup(TestContext context)
        {

        }
        [TestMethod]
        public void Simple()
        {
            TestObject obj;
            var context = IQ.GetDbContext();
            try
            {
                obj = context.From<TestObject>(101).First();
            }
            catch(Exception e) {
                Console.Write(e.Message);
            }

            Assert.AreEqual("SELECT TOP 1 PK,FirstName,HowMuch,UpdateDate,SomeNumber FROM testTable WHERE PK=@PK",TC.Controller.LastQuery.GetQuery(), "Query looks good");
            //Assert.AreEqual("SELECT TOP 1 * FROM testTable WHERE PK=@PK", TC.Controller.LastQuery.GetQuery(), "Query looks good");
            Assert.AreEqual(1, TC.Controller.LastQuery.Parameters.Count(), "One parm was created");
            Assert.AreEqual(101, TC.Controller.LastQuery.Parameters.ElementAt(0).Value, "Value for parm is correct");
            Assert.AreEqual("@PK", TC.Controller.LastQuery.Parameters.ElementAt(0).ParameterName, "Value for parm is correct");

            DateTime curDate = DateTime.Now;
            try
            {
                obj = IQ.Query<TestObject>("select * FROM testTable where UpdateDate<@now", curDate)
                    .First();
            }
            catch(Exception e) {
                Console.WriteLine(e.Message);
            }

            Assert.AreEqual("select * FROM testTable where UpdateDate<@now", TC.Controller.LastQuery.GetQuery(), "Query looks good");
            Assert.AreEqual(curDate, TC.Controller.LastQuery.Parameters.ElementAt(0).Value, "Value for parm is correct");

        }
        [TestMethod]
        public void MapToExpando()
        {
            IQ.Config.DynamicObjectType = typeof(JsObject);
            string sql = IQMap.Tests.TestConfig.GenerateSqlSelect(typeof(TestObject), 10);

            var expandoList = IQ.Query(sql);

            Assert.AreEqual(10,expandoList.Count(),"List had 10 results");
            dynamic obj1 = expandoList.First();
            Assert.AreEqual(IQMap.Tests.TestConfig.SampleData[typeof(string)], obj1.FirstName, "First name was not null.");

            Assert.AreEqual(typeof(JsObject), obj1.GetType());

        }
        [TestMethod]
        public void Query()
        {
            int result = IQ.Query("select top 1 pk,breed FROM animals where pk=@oldid", 1).First().pk;
            Assert.AreEqual(1, result, "Got a value from expando object");

            IEnumerable<int> ids = IQ.Query<int>("select pk from animals where pk>0 and pk<=10");

            Assert.AreEqual(10, ids.Count(), "Got a list of ints");
            ids = IQ.Query<int>("select pk,breed from animals where pk>0 and pk<=10");
            Assert.AreEqual(10, ids.Count(), "Value-typed generec list used first column");


        }
        //[TestMethod]
        //public void LinqToSql()
        //{
        //    IEnumerable<int> test;
            

                

        //}

    }
}
