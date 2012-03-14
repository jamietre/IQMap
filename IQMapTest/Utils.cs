using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = NUnit.Framework.Assert;
using IQMapTest.Mocks;
using IQMap;
using IQMap.Implementation;

namespace IQMapTest
{
    [TestClass]
    public class Utils_
    {
        int intVar = 1;
        bool boolVar = true;
        DateTime? dateTimeNullable = DateTime.Now;
        DateTime dateTime = DateTime.Now;
        string stringVar = "test";
        char charVal = 'a';
        decimal decimalVal = 1.0m;
        decimal? decimalNullable = 1.1m;
        TestObject classVar = new TestObject();
        float floatVar = 1.24f;
        byte byteVar = 12;
        byte[] byteArr = new byte[] { 0x10, 0xff, 0x50 };
        
        [TestMethod]
        public void IsNumericType()
        {


            Assert.IsTrue(Utils.IsNumericType(intVar));
            Assert.IsFalse(Utils.IsNumericType(boolVar));
            Assert.IsFalse(Utils.IsNumericType(dateTimeNullable));
            Assert.IsFalse(Utils.IsNumericType(dateTime));
            Assert.IsFalse(Utils.IsNumericType(stringVar));
            Assert.IsFalse(Utils.IsNumericType(charVal));
            Assert.IsFalse(Utils.IsNumericType(charVal));
            Assert.IsTrue(Utils.IsNumericType(decimalVal));
            Assert.IsTrue(Utils.IsNumericType(decimalNullable));
            Assert.IsFalse(Utils.IsNumericType(classVar));
            Assert.IsTrue(Utils.IsNumericType(floatVar));
            Assert.IsTrue(Utils.IsNumericType(byteVar));
            Assert.IsFalse(Utils.IsNumericType(byteArr));
        }

        [TestMethod]
        public void IsMappableType()
        {
            Assert.IsTrue(Utils.IsMappableType(intVar));
            Assert.IsTrue(Utils.IsMappableType(boolVar));
            Assert.IsTrue(Utils.IsMappableType(dateTimeNullable));
            Assert.IsTrue(Utils.IsMappableType(dateTime));
            Assert.IsTrue(Utils.IsMappableType(stringVar));
            Assert.IsTrue(Utils.IsMappableType(charVal));
            Assert.IsTrue(Utils.IsMappableType(charVal));
            Assert.IsTrue(Utils.IsMappableType(decimalVal));
            Assert.IsTrue(Utils.IsMappableType(decimalNullable));
            Assert.IsTrue(Utils.IsMappableType(floatVar));
            Assert.IsTrue(Utils.IsMappableType(byteVar));
            Assert.IsTrue(Utils.IsMappableType(byteArr));
            
            Assert.IsFalse(Utils.IsMappableType(classVar));
            
       }
        [TestMethod]
        public void GetInstanceOf()
        {
            Assert.AreEqual(0, Utils.GetInstanceOf<int>());
            Assert.AreEqual(0m, Utils.GetInstanceOf<int>());
            Assert.AreEqual(default(DateTime), Utils.GetInstanceOf<DateTime>());
            Assert.AreEqual(null, Utils.GetInstanceOf<DateTime?>());
            Assert.IsTrue(Utils.GetInstanceOf<TestObject>().GetType() == typeof(TestObject));
        }

       
    }
}
