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

namespace IQMap.Tests.QueryT
{
    [TestClass]
    public class AnyOne_
    {

        [TestMethod]
        public void Any()
        {
            var table = IQ.From<Cat>();
            Assert.IsTrue(table.Where("pk<10").Any());
            Assert.IsFalse(table.Where("pk=99999").Any());

        }
        [TestMethod]
        public void One()
        {
            var table = IQ.From<Cat>();
            Assert.IsTrue(table.Where("pk=3").One());
            Assert.IsFalse(table.Where("pk=99999").One());
            Assert.IsFalse(table.Where("pk<10").One());
        }
    }
}
