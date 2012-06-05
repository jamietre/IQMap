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
    public class To_
    {


        [TestMethod]
        public void Simple()
        {
            Dog target=new Dog();

            var firstDog = IQ.From<Dog>().To(target).ElementAt(1);

            Assert.AreEqual("Bulldog", firstDog.Breed, "The output object was correct");
            Assert.AreEqual("Bulldog", target.Breed, "The existing target object was correct");
            Assert.IsTrue(ReferenceEquals(firstDog, target));
            

        }
        [TestMethod]
        public void SubclassedTarget()
        {
            Cat target = new Cat();

            var firstDog = IQ.From<Dog>().Select("*").To(target).First();

            var pk = target.PK;
            Assert.AreEqual(1,target.Age);
            Assert.AreEqual(false,target.Declawed);
            Assert.IsTrue(ReferenceEquals(firstDog, target));

            var firstCat = IQ.From<Cat>().To(target).First();
            Assert.AreEqual(pk,target.PK);
        }
        [TestMethod]
        public void Mixed()
        {
            Dog dog = new Dog();
            // should map the first record to dog
            bool anyDogs = IQ.From<Dog>().To(dog).Any();
            
            Assert.IsTrue(anyDogs);
            Assert.AreEqual(1, dog.PK, "The first record was mapped to Dog");
            
            dog.Name="test";
            anyDogs = IQ.From<Dog>().Where("1=2").Any();
            Assert.IsFalse(anyDogs);
            Assert.AreEqual("test",dog.Name);

            IQ.From<Dog>().To(dog).Take(5).Last();
            
            // this "target" business needs to be fixed
            //Assert.AreEqual(1, dog.PK);
            //Assert.AreEqual("Fido4", dog.Name);
            

        }
    
    }
}
