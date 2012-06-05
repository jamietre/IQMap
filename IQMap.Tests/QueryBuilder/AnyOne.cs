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
    public class AnyOne_
    {

        [TestMethod]
        public void Any()
        {

            Assert.IsTrue(IQ.From("select * FROM animals").Any());
            Assert.IsTrue(IQ.From("select * FROM animals where pk=1").Any());
            Assert.IsFalse(IQ.From("select * FROM animals where pk=99999").Any());
            
        }
        [TestMethod]
        public void One()
        {

            Assert.IsTrue(IQ.From("select * FROM animals where pk=3").One());
            Assert.IsFalse(IQ.From("select * FROM animals").One());
            Assert.IsFalse(IQ.From("select * FROM animals where pk=99999").One());
            Assert.IsFalse(IQ.From("select * FROM animals where pk<10").One());
        }


    }
}
