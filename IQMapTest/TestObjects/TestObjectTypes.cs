using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMapTest.Mocks;
using IQMap;

namespace IQMapTest
{
    public enum TestEnum
    {
        Val1=1,
        Val2=2,
        Val3=3
    }
    public class TestObjectTypes
    {
        public int IntField { get; protected set; }
        public string StringField { get; set; }
        public float FloatField { get; set; }
        public decimal DecimalField { get; set; }
        public DateTime DateTimeField { get; set; }
        public DateTime? DateTimeNullableField { get; set; }
        public byte[] ByteArrayField { get; set; }
        public bool BooleanField { get; set; }
        public Guid GuidField { get; set; }
        public TestEnum EnumField { get; set; }

    }
    [IQClass("testTable")]
    public class TestObjectTypes2
    {
        [IQField(PK=true)]
        public int IntField { get; protected set; }

        public string StringField { get; set; }
        public float FloatField { get; set; }
        public decimal DecimalField { get; set; }
        public DateTime DateTimeField { get; set; }
        public DateTime? DateTimeNullableField { get; set; }
        public byte[] ByteArrayField { get; set; }
        public bool BooleanField { get; set; }
        public Guid GuidField { get; set; }
        public TestEnum EnumField { get; set; }
        
        [IQField(ReadOnly=true)]
        public string ReadOnlyField { get; set; }

        [IQIgnore]
        public IQEventType CapturedEvent { get; protected set; }
        [IQIgnore]
        public IDBObjectData CapturedDbData { get; protected set; }
        [IQIgnore]
        public bool  WasDirty { get; protected set; }

        [IQEventHandler]
        public void IQEvent(IQEventType eventType, IDBObjectData data)
        {
            CapturedEvent = eventType;
            CapturedDbData = data;
            WasDirty = data.IsDirty("Readonlyfield");
        }
    }
}
