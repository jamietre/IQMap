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
    public class WhereClause_
    {

        [TestMethod]
        public void Create()
        {
            
            var item1 = new WhereItem("field1",123);
            
            var parm = new QueryParameter("someparm","testvalue");
            var item2 = new WhereString("field2=@someparm",parm);
            var clause = new WhereClause(JoinType.And, item1, item2);

            Assert.AreEqual("field1=@field1 AND (field2=@someparm)", clause.GetSql());

            Assert.AreEqual(2, clause.Parameters.Count());
        }

        [TestMethod]
        public void Combine()
        {
            var item1 = new WhereItem("field1", 123);

            var parm = new QueryParameter("someparm", "testvalue");
            var item2 = new WhereString("field2=@someparm or field3='value'", parm);
            var item3 = new WhereItem("field4", 123,parameterize:false);

            var clause = new WhereClause(JoinType.And, item1, item2);

            Assert.AreEqual("field1=@field1 AND (field2=@someparm or field3='value')", clause.GetSql());
            var newClause = clause.Clone();
            newClause.Add(item3,JoinType.Or);
            Assert.AreEqual("(field1=@field1 AND (field2=@someparm or field3='value')) OR field4=123", newClause.GetSql());

            Assert.AreEqual(2, newClause.Parameters.Count());
        }
        [TestMethod]
        public void IsEmpty()
        {
            var item = new WhereClause();
            Assert.IsTrue(item.IsEmpty);

            item.Add(new WhereString("a=b"));
            Assert.IsFalse(item.IsEmpty);
        }

        [TestMethod]
        public void Clone()
        {
            var item = new WhereItem("field1", "a string");
            var clone = item.Clone();

            Assert.AreEqual(item, clone);
            clone.Value = "something-else";
            Assert.AreNotEqual(item, clone);
            clone.Value = "a string";
            Assert.AreEqual(item, clone);

            var parm = new QueryParameter("someparm", "testvalue");
            var item2 = new WhereString("field2=@someparm or field3='value'", parm);

            var clause = new WhereClause(JoinType.And,item, item2);
            var clone2 = clause.Clone();
            Assert.AreEqual(clause, clone2);
            parm.Value = "testvalue2";
            Assert.AreNotEqual(clause, clone2);
            parm.Value = "testvalue";
            Assert.AreEqual(clause, clone2);

            item.Value = "n";
            Assert.AreNotEqual(clause, clone2);
            item.Value = "a string";
            Assert.AreEqual(clause, clone2);
        }
    }
}
