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
    public class Create
    {

        static string tempTable;
        [ClassCleanup]
        public static void Cleanup()
        {
            TestConfig.dropTestTable(tempTable);
        }
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            tempTable = TestConfig.createTestTable();
        }

        
        [TestMethod]
        public void Insert()
        {
            // use alternate table def for methods that alter data during testing
            var opts = IQ.GetQueryOptions(tableName: tempTable);
            var table = IQ.From<Dog>(opts);

            var dog = new Dog();
            dog.Name = "Fido";
            IQ.Save(dog,opts);

            var sameDog = table.Where("pk=@pk", dog.PK).Single();
            Assert.AreEqual("Fido",sameDog.Name);

            dog = new Dog();
            dog.Name="Spot";
            IQ.Save(dog,opts);

             sameDog = table.First("pk=@pk",dog.PK);
             Assert.AreEqual("Spot", sameDog.Name);
        }


        [TestMethod]
        public void SqlName()
        {
            var wolf = IQ.From<Wolf>().First();

            Assert.AreEqual("Terrier",wolf.BreedName);


        }
     

    }
}
