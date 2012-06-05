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
    public class Single_
    {

        [TestMethod]
        public void Single()
        {
            var q = IQ.From<Dog>();

            var dog = q.Where(1).Single();
            Assert.AreEqual(dog.PK, 1);

            dog = null;
            Assert.Throws<InvalidOperationException>(() =>
            {
                dog = q.Where("1=2").Single();
            }, "Single fails when no matches");

            Assert.Throws<InvalidOperationException>(() =>
            {
                dog = q.Where("pk<3").Single();
            }, "Single fails when 2 returned");

            Assert.AreEqual(null, dog);

            Assert.Throws<InvalidOperationException>(() =>
            {
                dog = q.Where("pk<10").Single();
            }, "Single throws when more than one is returned.");
        }

        [TestMethod]
        public void SingleOrDefault()
        {
            var context = IQ.GetDbContext();


            var dog = context.From<Dog>(1).SingleOrDefault();
            Assert.AreEqual(dog.PK, 1);

            dog = context.From<Dog>("1=2").SingleOrDefault();
            Assert.AreEqual(null,dog);

            Assert.Throws<InvalidOperationException>(() =>
            {
                dog = context.From<Dog>("pk<10").SingleOrDefault();
            },"SingleOrDefault throws when more than one is returned.");
            
        }
        [TestMethod]
        public void SingleOrNew()
        {
            var q = IQ.From<Dog>();

            var dog = q.Where(1).SingleOrNew();
            Assert.AreEqual(dog.PK, 1);

            dog = q.Where("1=2").SingleOrNew();
            Assert.AreEqual(dog.PK,0,"SingleOrNew returns a new element when no matches");

            Assert.Throws<InvalidOperationException>(() =>
            {
                dog = q.Where("pk<3").SingleOrNew();
            }, "Single fails when 2 returned");

        }

    }
}
