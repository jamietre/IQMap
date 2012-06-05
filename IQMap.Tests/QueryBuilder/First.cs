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

namespace IQMap.Tests.QueryBuilder
{
    [TestClass]
    public class First_
    {

        [TestMethod]
        public void First()
        {
            var q = IQ.From<Dog>();

            var dog = q.OrderBy("PK").First();
            Assert.AreEqual(1,dog.PK);

            dog = q.OrderBy("PK").Where("PK<10").Reverse().First();
            Assert.AreEqual(9, dog.PK);

            dog = q.OrderBy("PK").Where("PK=3").First();
            Assert.AreEqual(3, dog.PK);

            Assert.Throws<InvalidOperationException>(() =>
            {
                dog = q.Where("1=2").Single();
            }, "First fails when no matches");

          
        }

        [TestMethod]
        public void FirstOrDefault()
        {
            var q = IQ.From<Dog>().OrderBy("pk");

            var dog = q.Where(1).FirstOrDefault();
            Assert.AreEqual(dog.PK, 1);
            Assert.AreEqual(3, q.FirstOrDefault("pk=3").PK);
            Assert.AreEqual(null, q.FirstOrDefault("pk=33333"));

            Assert.AreEqual(1,q.FirstOrDefault("pk<10").PK);
            
        }
        [TestMethod]
        public void FirstOrNew()
        {
            var q = IQ.From<Dog>().OrderBy("pk");

            var dog = q.Where(1).FirstOrNew();
            Assert.AreEqual(dog.PK, 1);
            Assert.AreEqual(3, q.FirstOrNew("pk=3").PK);
            Assert.AreEqual(0, q.FirstOrNew("pk=33333").PK);
            Assert.AreEqual(1, q.FirstOrNew("pk<10").PK);

        }

    }
}
