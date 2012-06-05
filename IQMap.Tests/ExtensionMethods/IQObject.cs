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
    public class IQObject_
    {

        [TestMethod]
        public void LoadPk()
        {
            Cat obj = new Cat();

            Assert.Throws<InvalidOperationException>(() =>
            {
                obj.Load(99999);
            }, "Missing PK fails");

            obj.Load(2);

            Assert.AreEqual(2, obj.PK);
            Assert.AreEqual("Bulldog", obj.Breed);



        }
        [TestMethod]
        public void LoadWhere()
        {
            Cat obj = new Cat();
            Assert.Throws<InvalidOperationException>(() =>
            {
                obj.Load("breed=@breed", "Terrier");
            }, "More than one match fails");

            obj.Load("pk=@pk", 2);

            Assert.AreEqual(2, obj.PK);
            Assert.AreEqual("Bulldog", obj.Breed);
        }


    }
}
