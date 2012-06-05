using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Dynamic;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IQMap;
using IQMap.Impl;
using Assert = NUnit.Framework.Assert;
using TC = IQMap.Tests.TestConfig;

namespace IQMap.Tests
{
    [TestClass]
    public class Delete_
    {
        private static string tempTableName;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            tempTableName = TestConfig.createTestTable();
        }
        [ClassCleanup]
        public static void Cleanup()
        {
            TestConfig.dropTestTable(tempTableName);
        }

        [TestMethod]
        public void DeleteWithWhere()
        {

            var td = IQ.GetQueryOptions(tableName: tempTableName);
            
            // Use ToList to force enumeration so we can check cache
            Assert.AreEqual(20, IQ.From<Dog>(td).ToList().Count(), "Got 20 records.");

            IQ.From<Dog>("age", 2, td).Delete();
            Assert.AreEqual(19, IQ.From<Dog>(td).ToList().Count(), "Got 19 records.");
        }
     



    }
}
