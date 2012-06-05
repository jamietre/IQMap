using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Dynamic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using IQMap;
using IQMap.Impl;
using Assert = NUnit.Framework.Assert;
using TC = IQMap.Tests.TestConfig;

namespace IQMap.Tests
{
    [TestClass]
    public class IEnumerable_
    {

        [TestMethod]
        public void OrNew()
        {
            List<int> test = new List<int> { 1, 2, 3, 4, 5 };
            Assert.AreEqual(5, test.OrNew().Count());
            test.Clear();
            Assert.AreEqual(1, test.OrNew().Count());
            Assert.AreEqual(0, test.OrNew().First());

            List<Dog> dogs = new List<Dog>();

            Assert.AreEqual(typeof(Dog), dogs.OrNew().First().GetType());

        }
       
    }
}
