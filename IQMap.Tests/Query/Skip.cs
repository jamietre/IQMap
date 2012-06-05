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
    public class Skip_
    {

        [TestMethod]
        public void Simple()
        {
            var q = IQ.From<Dog>();

            Assert.AreEqual(15, q.Skip(5).Enumerate().Count(), "There are 15 dogs remaining.");
            Assert.AreEqual(15, q.Skip(5).Count(), "There are 15 dogs remaining.");
            Assert.AreEqual(6, q.Skip(5).First().PK, "Got the right element.");
            Assert.AreEqual(2, q.Skip(1).Enumerate().First().PK, "Got the right element");
            Assert.AreEqual(2, q.Skip(1).First().PK, "Got the right element");
            Assert.AreEqual(4, q.Skip(1).Skip(2).Take(1).First().PK, "Got the right element");

        }


    }
}

