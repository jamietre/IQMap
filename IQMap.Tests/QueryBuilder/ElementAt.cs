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
    public class ElementAt_
    {


        [TestMethod]
        public void Basic()
        {
            var dog = IQ.From<Dog>().ElementAt(0);

            Assert.AreEqual("Terrier", dog.Breed);

            dog = IQ.From<Dog>().ElementAt(1);

            Assert.AreEqual("Bulldog", dog.Breed);

            dog = IQ.From<Dog>().ElementAt(10);

            Assert.AreEqual("Terrier", dog.Breed);
        }

        [TestMethod]
        public void Compound()
        {
            var baseQuery = IQ.From<Dog>();

            var dog = baseQuery.Take(5).Last();
            Assert.AreEqual(dog.PK, baseQuery.ElementAt(4).PK);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                dog = baseQuery.ElementAt(20);

            }, "Error thrown accessing nonexistent element");
        }
    }
}
