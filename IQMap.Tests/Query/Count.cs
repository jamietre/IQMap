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
    public class Count_
    {

        [TestMethod]
        public void Simple()
        {
            var q = IQ.From<Dog>();

            Assert.AreEqual(20, q.Count(), "There are 20 dogs.");

            Assert.AreEqual(10, q.Where("pk<=10").Count(), "10 dogs were selected.");

        }

        [TestMethod]
        public void Compound()
        {
            var q = IQ.From<Dog>();

            Assert.AreEqual(15, q.Skip(5).Count(), "Got 15 after skipping 5.");

            var newQ = q.Skip(5).Take(2);
            Assert.AreEqual(2, newQ.Count(), "2 dogs were selected.");
            Assert.AreEqual(6, newQ.First().PK, "The right one was there.");


            
        }
      

    }
}
