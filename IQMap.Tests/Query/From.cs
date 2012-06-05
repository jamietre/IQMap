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
    public class From_
    {
        static string tempTable;
        [ClassCleanup]
        public static void Cleanup()
        {
            TestConfig.dropTestTable(tempTable);
        }
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            tempTable = TestConfig.createTestTable();
        }


        [TestMethod]
        public void CloseConnectionNeverReconnect()
        {

            var context = IQ.GetDbContext(CommandBehavior.CloseConnection, DbReconnect.NeverReconnect);

            var q = context.From<Dog>("age>2 and age<=4");

            Assert.AreEqual(2, q.Count());
            Assert.Throws<InvalidOperationException>(() =>
            {
                // connection closed
                Assert.AreEqual(2, q.Count());
            });
            context.Dispose();
        }
        [TestMethod]
        public void DefaultNeverReconnect()
        {

            var context = IQ.GetDbContext(CommandBehavior.Default, DbReconnect.NeverReconnect);
            DbContext ctxInstance = (DbContext)context;

            var q = context.From<Dog>("age>2 and age<=4");

            Assert.AreEqual(2, q.Count());
            Assert.AreEqual(2, q.Count()); // works - connection sustained.
            context.Dispose();
            Assert.AreEqual(ConnectionState.Closed, ctxInstance.ConnectionActual.State);
        }
        [TestMethod]
        public void PassTransaction()
        {
            // Try passing DbCommand options
            var transaction = IQ.Connection.BeginTransaction();

            var context = IQ.GetDbContext(CommandBehavior.Default, DbReconnect.NeverReconnect);
            DbContext ctxInstance = (DbContext)context;

            IDataReader reader = context
                .Options(CommandBehavior.Default,transaction)
                .RunSql("select top 5 * from animals order by age");

            reader.Read();
            reader.Read();

            Assert.AreEqual(2, reader.Map<Dog>().Age);
            reader.Dispose();
            Assert.AreEqual(ConnectionState.Open, ctxInstance.Transaction.Connection.State);

            context.Options(CommandBehavior.CloseConnection);
            int count = context.From<Dog>().Count();
            Assert.AreEqual(20, count);
            
            // When a transaction is active, it cannot be closed
            Assert.AreEqual(ConnectionState.Open, ctxInstance.Transaction.Connection.State);
            context.CommitTransaction();

            Assert.AreEqual(null, ctxInstance.Transaction);

        }
        
        [TestMethod]
        public void TableParameters()
        {
            var context = IQ.GetDbContext(CommandBehavior.Default, DbReconnect.NeverReconnect);
            var  q = context.From<Dog>("age>2 and age<=4",IQ.GetQueryOptions(tableName: tempTable));
            Assert.AreEqual(2, q.Count());

            // try swapping PK
            var oneDog = context.From<Dog>(5, IQ.GetQueryOptions(primaryKey: "age")).Single();
            Assert.AreEqual(5, oneDog.Age);

        }
       
    }
}
