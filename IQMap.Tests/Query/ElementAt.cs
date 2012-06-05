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

namespace IQMap.Tests.QueryT
{
    [TestClass]
    public class ElementAt_
    {


        [TestMethod]
        public void Basic()
        {
            var dogs = IQ.From("select * FROM animals");

            dynamic dog = dogs.ElementAt(0);

            Assert.AreEqual("Terrier", dog.Breed);

            dog = dogs.ElementAt(1);

            Assert.AreEqual("Bulldog", dog.Breed);

            dog = dogs.ElementAt(10);

            Assert.AreEqual("Terrier", dog.Breed);


        }

        [TestMethod]
        public void Compound()
        {
            var baseQuery = IQ.From("select * FROM animals where pk < 10 order by PK");

            var dog = baseQuery.Take(5).Last();
            Assert.AreEqual(dog.Pk, baseQuery.ElementAt(4).Pk);

            Cat cat = baseQuery.As<Cat>().Reverse().ElementAt(2);
            Assert.AreEqual(7, cat.PK);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                cat = baseQuery.As<Cat>().ElementAt(20);

            }, "Error thrown accessing nonexistent element");
        }
    }
}
