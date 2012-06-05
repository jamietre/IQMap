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
    public class SelectItem_
    {


        [TestMethod]
        public void Create()
        {
            var item = new SelectItem("colname","MeaningfulName");
            Assert.AreEqual("colname AS MeaningfulName", item.GetSql());
            Assert.AreEqual("colname", item.Field);

        }

        [TestMethod]
        public void CreateParse()
        {
            var item = new SelectItem("colname as aliasname");
            Assert.AreEqual("colname", item.Field);
            Assert.AreEqual("aliasname", item.Alias);

            item = new SelectItem("colname");
            Assert.AreEqual("colname", item.Field);
            Assert.AreEqual(null, item.Alias);


            Assert.Throws<ArgumentException>(() =>
            {
                item = new SelectItem("colname aliasname");
            });

            Assert.Throws<ArgumentException>(() =>
            {
                item = new SelectItem("colname as an aliasname");
            });

            Assert.Throws<ArgumentException>(() =>
            {
                item = new SelectItem("colname = aliasname");
            });


        }

        [TestMethod]
        public void IsEmpty()
        {
            var item = new SelectItem();
            Assert.IsTrue(item.IsEmpty);

            item.Field = "test";
            Assert.IsFalse(item.IsEmpty);
        }

        [TestMethod]
        public void Clone()
        {
            var item = new SelectItem("field1");
            var clone = item.Clone();

            Assert.IsTrue(item.Equals(clone));
            Assert.AreEqual("field1", item.GetSql());

            item.Alias = "alias1";
            Assert.AreEqual("field1", clone.GetSql());
            Assert.AreEqual("field1 AS alias1", item.Clone().GetSql());
        }
    }
}
