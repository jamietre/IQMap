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
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;

namespace IQMap.Tests
{
    [TestClass]
    public class OrderByClause_
    {
        [TestMethod]
        public void Create()
        {
            var item = new OrderByItem("field1", SortOrder.Ascending);
            var clause = new OrderByClause("field1 asc");
            Assert.AreEqual(item, clause);

            clause.Add("field2 desc");
            Assert.AreEqual("field1,field2 desc", clause.ToString());

           clause = new OrderByClause("field1 desc, field2");
           Assert.AreEqual("field1 desc,field2", clause.ToString());
        }

        [TestMethod]
        public void Reverse()
        {
            var clause = new OrderByClause("field1 desc, field2");
            clause.Reverse();
           
            Assert.AreEqual("field1,field2 desc", clause.ToString());
           
        }

        [TestMethod]
        public void IsEmpty()
        {
            var clause = new OrderByClause();
            Assert.IsTrue(clause.IsEmpty);

            var item = new OrderByItem("field1", SortOrder.Descending);
            clause.Add(item);

            Assert.IsFalse(clause.IsEmpty);
        }

        [TestMethod]
        public void Clone()
        {
            var clause = new OrderByClause("field1 desc, field2");
            var clone = clause.Clone();

            Assert.AreEqual("field1 desc,field2", clone.ToString());
            Assert.AreEqual(clause,clone);
            clone.Reverse();
            Assert.AreNotEqual(clause,clone);
            clone.Reverse();
            Assert.AreEqual(clause, clone);
        }
        [TestMethod]
        public void Add()
        {
            IOrderByClause clause = new OrderByClause("field1 desc, field2");
            var item = new OrderByItem("field3", SortOrder.Descending);
            clause.Add(item);

            Assert.AreEqual("field1 desc,field2,field3 desc", clause.ToString());

            // can't add existing field
            Assert.Throws<ArgumentException>(() =>
            {
                clause.Add("field2");
            });

            clause.AddAlways("field1", SortOrder.Ascending);
            Assert.AreEqual("field2,field3 desc,field1", clause.ToString());

            item.Reverse();
            Assert.AreEqual("field2,field3,field1", clause.ToString());

        }
    }
}
