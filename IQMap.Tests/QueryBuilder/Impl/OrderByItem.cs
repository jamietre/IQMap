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
    public class OrderByItem_
    {


        [TestMethod]
        public void Create()
        {
            var item = new OrderByItem("field1", SortOrder.Ascending);
            Assert.AreEqual("field1", item.GetSql());
            Assert.AreEqual("field1", item.ToString());

            item.Order = SortOrder.Descending;
            Assert.AreEqual("field1 desc",item.ToString());

            Assert.IsFalse(item.IsEmpty);
        }

        [TestMethod]
        public void Reverse()
        {
            var item = new OrderByItem("field1", SortOrder.Ascending);
            item.Reverse();
            Assert.AreEqual("field1 desc", item.ToString());
            Assert.AreEqual(SortOrder.Descending, item.Order);
           
        }

        [TestMethod]
        public void IsEmpty()
        {
            var item = new OrderByItem();
            Assert.IsTrue(item.IsEmpty);

            item.Field = "test";
            item.Order = SortOrder.Ascending;
            Assert.IsFalse(item.IsEmpty);
            Assert.AreEqual("test", item.GetSql());

            item.Order = SortOrder.Descending;
            Assert.AreEqual("test desc", item.GetSql());
        }

        [TestMethod]
        public void Clone()
        {
            var item = new OrderByItem("field1", SortOrder.Descending);
            var clone = item.Clone();

            Assert.IsTrue(item.Equals(clone));
            clone.Reverse();
            Assert.IsFalse(item.Equals(clone));
            clone.Reverse();
            Assert.IsTrue(item.Equals(clone));
        }
    }
}
