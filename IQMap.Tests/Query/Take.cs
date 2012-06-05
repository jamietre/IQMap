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
    public class Take_
    {

        [TestMethod]
        public void Simple()
        {
            var q = IQ.From<Dog>();

            Assert.AreEqual(15, q.Take(15).Count(), "There are 15 dogs remaining.");
            Assert.AreEqual(15, q.Take(15).Enumerate().Count(), "There are 15 dogs remaining.");

            Assert.AreEqual(2, q.Skip(5).Take(2).Count());
            Assert.AreEqual(2, q.Skip(5).Take(2).Enumerate().Count());
        }
        
        [TestMethod]
        public void Complex()
        {
            var q = IQ.From<Dog>().OrderBy("pk");

            var crazy1 = q.Skip(5).Take(10).Skip(3).Take(3);

            Assert.AreEqual(3, crazy1.Count());
            Assert.AreEqual(3, crazy1.Enumerate().Count());
            
            Assert.AreEqual(3, crazy1.Take(5).Count());
            Assert.AreEqual(3, crazy1.Take(5).Enumerate().Count());

            var cur = crazy1.First();
            Assert.AreEqual(9, cur.PK, "Should be the 9th record");


            Assert.AreEqual(11, crazy1.Enumerate().Reverse().First().PK, "11 should be the last one selected");


        }

        [TestMethod]
        public void ReverseWithTake()
        {
            var dogs = IQ.From<Dog>()
               .Where("pk<@pk", 5)
               .OrderBy("breed")
               .ThenBy("pk")
               .Reverse()
               .First();

        }
    }
}

