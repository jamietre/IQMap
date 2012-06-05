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
    public class QueryBuilder_
    {

       
        public void Simple()
        {
            

            List<Dog> test = new List<Dog>(IQ.From<Dog>().Where("age>10"));

            Assert.AreEqual(10, test.Count(), "Got 10 records.");

        }
        [TestMethod]
        public void Count()
        {
            var count= IQ.From<Dog>().Where("age>10").Count();
            Assert.AreEqual(10, count, "Got 10 records.");
        }


        [TestMethod]
        public void Any()
        {
            
            Assert.IsTrue(IQ.From<Dog>().Any("breed=@b", "Terrier"));
            Assert.IsFalse(IQ.From<Dog>().Any("breed=@b", "Husky"));
        }

        [TestMethod]
        public void Contains()
        {
            var dog = IQ.From<Dog>("Name=@name", "Fido2").Single();

            Assert.IsTrue(IQ.From<Dog>().Contains(dog));
            dog = new Dog();

            Assert.IsFalse(IQ.From<Dog>().Contains(dog));
        }

        [TestMethod]
        public void SingleOrNew()
        {
            var dog = IQ.From<Dog>("Name=@name", "Fido2").SingleOrNew();

            Assert.AreEqual("Fido2", dog.Name);
            Assert.IsFalse (dog.PK==0);

            dog = IQ.From<Dog>("Name=@name", "Fido222").SingleOrNew();
            Assert.AreEqual(null, dog.Name);
            Assert.IsTrue(dog.PK == 0);

        }

        [TestMethod]
        public void Take()
        {
            var dog = IQ.From<Dog>("age>@age", 10);
            List<Dog> take1 = new List<Dog>(dog.Take(5));
            Assert.AreEqual(5, take1.Count());

        }

        [TestMethod]
        public void First()
        {
            var dogList = IQ.From<Dog>("age>@age", 10).OrderBy("age");

            Assert.AreEqual(11, dogList.First().Age );

            Assert.Throws<InvalidOperationException>(() =>{
                dogList = IQ.From<Dog>("age=100");
                var first = dogList.First();
            });

        }

        [TestMethod]
        public void OrderBy()
        {
            var dog = IQ.From<Dog>("age<@age", 10).OrderBy("age asc");
            
            Assert.AreEqual(1, dog.First().Age );

            dog = IQ.From<Dog>("age<@age", 10).OrderBy("age desc");
            
            Assert.AreEqual(9 ,dog.First().Age);
        }


        [TestMethod]
        public void Paging()
        {
            var dogs = IQ.From<Dog>().OrderBy("age asc").Skip(8).Take(4);
            var testc = dogs.Count();

            Assert.AreEqual(4, dogs.Count());
            Assert.AreEqual("Fido8",dogs.First().Name);

       
        }
    }
}
