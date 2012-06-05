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
    public class Map_
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
        public void Values()
        {
            DateTime curTime = DateTime.Now;
            IQ.Query("update " + tempTable + " set DateCreated=@date", curTime);

            var table = IQ.From<Dog>(IQ.GetQueryOptions(tableName: tempTable));
            

            var breeds = table.Select("breed,pk").As<string>();

            Assert.AreEqual(20, breeds.Count());
            Assert.AreEqual("Terrier", breeds.First());

            var ints = table.Select("pk").Where("pk<3").As<int>();
            

            var longVal= table.Where("pk=1").As<int>().First();
            Assert.AreEqual(1, longVal);

            var dateVal = table.Select("DateCreated").Where("pk=1").As<DateTime>().Single();
            Assert.IsTrue(curTime.AddSeconds(-1) < dateVal && curTime.AddSeconds(1) > dateVal);

        }

        [TestMethod]
        public void Objects()
        {
            var dogs = IQ.From<Dog>().As<smallDog>();

            Assert.AreEqual(20, dogs.Count());
            Assert.AreEqual("Terrier", dogs.First().Breed);
            Assert.AreEqual("Shih Tzu", dogs.Last().Breed);
        }

        [TestMethod]
        public void MapFunction()
        {
            var breeds = IQ.From<Dog>().Select(dog => "small " + dog.Breed);
            Assert.AreEqual(20, breeds.Count());
            Assert.AreEqual("small Terrier", breeds.First());
            Assert.AreEqual("small Shih Tzu", breeds.Last());

            var dogs = IQ.From<Dog>().Select((Dog dog) => {
                smallDog sd = new smallDog();
                sd.Breed = "small " + dog.Breed;
                return sd;
            });
            Assert.AreEqual(20, dogs.Count());
            Assert.AreEqual("small Terrier", dogs.First().Breed);
            Assert.AreEqual("small Shih Tzu", dogs.Last().Breed);
        
        }


        class smallDog
        {
            public int PK { get; set; }
            public string Breed { get; set; }

        }

    }
}
