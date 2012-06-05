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
    public class WhereItem_
    {

        [TestMethod]
        public void Create()
        {

            var item = new WhereItem("field1",123);
            Assert.AreEqual("field1=@field1", item.GetSql());
            Assert.AreEqual(1,item.Parameters.Count());


            item = new WhereItem("field1", "a string",ComparisonType.LikeEndsWith,false);
            Assert.AreEqual("field1 LIKE '%a string'", item.ToString());
        }


        [TestMethod]
        public void IsEmpty()
        {
            var item = new WhereItem();
            Assert.IsTrue(item.IsEmpty);

            item.Field = "test";
            Assert.IsFalse(item.IsEmpty);
        }

        [TestMethod]
        public void Clone()
        {
            var item = new WhereItem("field1","a string");
            var clone = item.Clone();

            Assert.AreEqual(item,clone);
            clone.Value="something-else";
            Assert.AreNotEqual(item,clone);
            clone.Value = "a string";
            Assert.AreEqual(item,clone);

        }
    }
}
