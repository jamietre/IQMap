﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMapTest.Mocks;
using IQMap;

namespace IQMapTest
{
    [IQClass(TableName="testTable",ExcludeProperties=true)]
    public class TestObject : IQObject
    {
        public TestObject()
        {
           
        }

        [IQField(PK= true)]
        public int PK { get; protected set; }

        [IQField]
        public string FirstName { get; set; }

        [IQField]
        public float HowMuch { get; set; }
        
        [IQField]
        public DateTime UpdateDate { get; set; }
        
        [IQField] 
        public long SomeNumber { get; set; }

        public string NotAField { get; set; }


    }
}
