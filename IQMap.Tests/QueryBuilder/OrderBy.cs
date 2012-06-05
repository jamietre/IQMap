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

namespace IQMap.Tests.Methods
{
    [TestClass]
    public class OrderByThenBy_
    {

        [TestMethod]
        public void OrderBy()
        {
            var q = IQ.From<Dog>();
            string baseQ = q.ToString();

            var ordered = q.OrderBy("pk");
            Assert.AreEqual(q + " ORDER BY pk", ordered.ToString());
            
            ordered = ordered.OrderBy("breed");
            Assert.AreEqual(q + " ORDER BY breed", ordered.ToString());

            ordered = ordered.OrderByDescending("name");
            Assert.AreEqual(q + " ORDER BY name desc", ordered.ToString());
        }
        [TestMethod]
        public void ThenBy()
        {
            var q = IQ.From<Dog>();
            string baseQ = q.ToString();

            var ordered = q.ThenBy("pk");
            Assert.AreEqual(q + " ORDER BY pk", ordered.ToString());

            ordered = ordered.ThenBy("breed");
            Assert.AreEqual(q + " ORDER BY pk,breed", ordered.ToString());

            ordered = ordered.OrderByDescending("name");
            Assert.AreEqual(q + " ORDER BY name desc", ordered.ToString());

            ordered = ordered.ThenBy("breed");
            Assert.AreEqual(q + " ORDER BY name desc,breed", ordered.ToString());
        }


    }
}
