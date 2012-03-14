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
using IQMap.Implementation;
using IQMapTest.Mocks;
using Assert = NUnit.Framework.Assert;
using TC = IQMapTest.TestConfig;

namespace IQMapTest
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
            try
            {
                obj= IQ.First<TestObject>(101);
            }
            catch { }

            Assert.AreEqual("SELECT PK,FirstName,HowMuch,UpdateDate,SomeNumber FROM testTable WHERE PK=@PK",TC.Controller.LastQuery, "Query looks good");
            Assert.AreEqual(1, TC.Controller.LastParameters.Count(), "One parm was created");
            Assert.AreEqual(101, TC.Controller.LastParameters.ElementAt(0).Value, "Value for parm is correct");
            Assert.AreEqual("@PK", TC.Controller.LastParameters.ElementAt(0).ParameterName, "Value for parm is correct");

            DateTime curDate = DateTime.Now;
            try
            {
                obj = IQ.First<TestObject>("select * FROM testTable where UpdateDate<@now", curDate);
            }
            catch { }

            Assert.AreEqual("select * FROM testTable where UpdateDate<@now", TC.Controller.LastQuery, "Query looks good");
            Assert.AreEqual(curDate, TC.Controller.LastParameters.ElementAt(0).Value, "Value for parm is correct");

        }
        [TestMethod]
        public void MapToExpando()
        {
            string sql = IQMapTest.TestConfig.GenerateSqlSelect(typeof(TestObject), 10);

            var expandoList = IQ.Query(sql).MapAll<ExpandoObject>();

            Assert.AreEqual(10,expandoList.Count(),"List had 10 results");
            dynamic obj1 = expandoList.First();
            Assert.AreEqual(IQMapTest.TestConfig.SampleData[typeof(string)], obj1.FirstName, "First name was not null.");

        }

        //[TestMethod]
        //public void LinqToSql()
        //{
        //    IEnumerable<int> test;
            

                

        //}

    }
}
