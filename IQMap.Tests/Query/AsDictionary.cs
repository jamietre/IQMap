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
    public class AsDictionary
    {

        [TestMethod]
        public void Basic()
        {
            var dict = IQ.From<Dog>().AsDictionaries().First();
            Assert.AreEqual(1, dict["pk"]);

            dict = IQ.From<Dog>().Select("pk").AsDictionaries().First();
            Assert.AreEqual(1, dict["pk"]);
            Assert.IsTrue(dict.ContainsKey("breed"),"The dictionary contains keys for anything bound in the object");
            Assert.IsNull(dict["breed"],"The value in the dict is the default value from the object");
          
        }
    }
}
