using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IQMap;
using IQMap.Impl;
using IQMap.Impl.Support;
using Assert = NUnit.Framework.Assert;


namespace IQMap.Tests.ParameterParser_
{
    [TestClass]
    public class Basic
    {

        [TestMethod]
        public void SimpleQueries()
        {
            string sql = "select * FROM table where col = @parm1, col2 = @parm2";
            var pp = new ParameterParser(sql, 1,"something");

            Action sameTest = () =>
            {
                var p1 = pp.Parameters.FirstOrDefault(item => item.ParameterName == "@parm1");
                var p2 = pp.Parameters.FirstOrDefault(item => item.ParameterName == "@parm2");

                Assert.AreEqual(2, pp.Parameters.Count);
                Assert.AreEqual(1, p1.Value);
                Assert.AreEqual("something", p2.Value);
            };
            sameTest();

            pp = new ParameterParser(sql, "parm2", "something", "parm1", 1);
            sameTest();

            pp = new ParameterParser(sql, new
            {
                parm1= 1,
                parm2= "something"
            });
            sameTest();
            
            pp = new ParameterParser(sql, new
            {
                parm1= 1,
            },new
            {
                parm2= "something"
            });

            sameTest();

        }
        [TestMethod]
        public void ExtraData()
        {
            string sql = "select * FROM table where col = @parm1, col2 = @parm2";
            
            Assert.Throws<ArgumentException>(() => {
                var pp = new ParameterParser(sql, new {parm2="p2value", parm1="p1value"},null);
            });
            
            var pp2= new ParameterParser(sql, new { parm2 = "p2value", parm1 = "p1value" });

            Assert.AreEqual(2, pp2.Parameters.Count);
        }
        [TestMethod]
        public void CaseSensitivity()
        {
            IQ.Config.IgnoreCaseParameters = false;
            var sql="select * FROM table where col = @Parm1";

            var pp = new ParameterParser(sql, "p1value");
            Assert.AreEqual(pp.Parameters[0].ParameterName, "@Parm1");
            Assert.AreEqual(pp.Parameters[0].Value, "p1value");

            Assert.Throws<ArgumentException>(() =>
            {
                pp = new ParameterParser(sql, "@parm1",123);
            });

            Assert.Throws<ArgumentException>(() =>
            {
                pp = new ParameterParser(sql, new
                    {
                        parm1=123
                    });
            });

            IQ.Config.IgnoreCaseParameters = true;
            pp = new ParameterParser(sql, "@parm1",123);
            Assert.AreEqual(pp.Parameters[0].ParameterName, "@Parm1");
            Assert.AreEqual(pp.Parameters[0].Value, 123);

        }


        [TestMethod]
        public void NullValues()
        {
            ParameterParser pp = new ParameterParser("select * FROM table where col = @parm1", 1);

            Assert.AreEqual(1, pp.Parameters.Count);
            Assert.AreEqual(1, pp.Parameters[0].Value);
            // need 2 parms - null values after the first parameter are considered.
            Assert.Throws<ArgumentException>(() =>
            {
                pp = new ParameterParser("select * FROM animals where age = @parm1 and age = @parm2", 1);
            });

        }
    }
}
