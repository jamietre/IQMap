using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IQMap;
using IQMap.Impl;
using Assert = NUnit.Framework.Assert;
using TC = IQMap.Tests.TestConfig;

namespace IQMap.Tests
{
    [TestClass]
    public class Events
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


        class EventDog: Dog, IQObject
        {
            protected void IQConstructor(IClassInfoConstructor dbInfo)
            {
                base.Constructor(dbInfo);

                dbInfo.AddEventHandler(IQEventType.BeforeDelete, Event);
                dbInfo.AddEventHandler(IQEventType.BeforeInsert, Event);
                dbInfo.AddEventHandler(IQEventType.BeforeSave, Event);
                dbInfo.AddEventHandler(IQEventType.BeforeUpdate, Event);
                dbInfo.AddEventHandler(IQEventType.OnDelete, Event);
                dbInfo.AddEventHandler(IQEventType.OnInsert, Event);
                dbInfo.AddEventHandler(IQEventType.OnLoad, Event);
                dbInfo.AddEventHandler(IQEventType.OnSave, Event);
                dbInfo.AddEventHandler(IQEventType.OnUpdate, Event);
            }

            public List<IEventData> Events = new List<IEventData>();
            protected void Event(IEventData data)
            {
                if (RejectNextEvent == data.EventType)
                {
                    data.Cancel = true;
                }
                Events.Add(data);
            }
            [IQIgnore]
            public IQEventType RejectNextEvent { get; set; }
        }
       
            
        [TestMethod]
        public void SaveEvents()
        {
            // use alternate table def for methods that alter data during testing
            var opts = IQ.GetQueryOptions(tableName: tempTable);

            var dog = new EventDog();
            dog.Name = "SomeDog";
            dog.Age = 10;
            IQ.Save(dog,opts);

            
            Assert.AreEqual(4,dog.Events.Count,"4 events found before/on save/insert");

            // verify event sequence
            IQEventType[] types = dog.Events.Select(item => item.EventType).ToArray();
            Assert.AreEqual(new IQEventType[] {IQEventType.BeforeSave, IQEventType.BeforeInsert, IQEventType.OnInsert, IQEventType.OnSave},
                types, "Event handler worked.");

            dog.Events.Clear();
            dog.Age = 11;
            // try out extension method
            dog.Save(opts);

            types = dog.Events.Select(item => item.EventType).ToArray();
            Assert.AreEqual(new IQEventType[] { IQEventType.BeforeSave, IQEventType.BeforeUpdate, IQEventType.OnUpdate, IQEventType.OnSave },
                types, "Event handler worked.");
        }

        [TestMethod]
        public void DeleteEvents()
        {
            // use alternate table def for methods that alter data during testing
            var opts = IQ.GetQueryOptions(tableName: tempTable);

            var dog = new EventDog();
            dog.Name = "SomeDog";
            dog.Age = 10;
            IQ.Save(dog,opts);
            
            var id = dog.PK;
            
            dog.Events.Clear();
            bool deleted = dog.Delete(opts);

            Assert.IsTrue(deleted);
            Assert.AreEqual(2, dog.Events.Count, "Two events on delete");

            Assert.Throws<InvalidOperationException>(() =>
            {
                dog.Delete();

            }, "Can't delete with zero-valued primary key");

           

            dog.Save(opts);
            Assert.AreEqual(id + 1, dog.PK, "ID incremented by one for a new save");

            dog.RejectNextEvent = IQEventType.BeforeDelete;
            deleted = dog.Delete(opts);

            Assert.IsFalse(deleted, "Was not actually deleted");
            dog.RejectNextEvent = IQEventType.OnDelete;
            Assert.Throws<InvalidOperationException>(() =>
            {
                dog.Delete();
            }, "Rejecting the after-delete event throws an exception");

        }
    }
}

