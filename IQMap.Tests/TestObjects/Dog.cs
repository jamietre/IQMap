using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap;

namespace IQMap.Tests
{

    public class Animal
    {

        public int? Age { get; set; }
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public float? Weight { get; set; }

        [IQIgnore]
        public int IgnoredProperty { get { return 1; } }
    }

    [IQClass]
    public class Dog: Animal
    {
        protected virtual void Constructor(IClassInfoConstructor data)
        {
            data.Query.TableName = "animals";
            data.Query.PrimaryKey = "PK";
            data.AddEventHandler(IQEventType.OnSave, OnSave);
        }
        public void OnSave(IEventData data)
        {
            SavedEventData = data;
            SavedEventType = data.EventType;
        }
        [IQIgnore]
        public IEventData SavedEventData;
        [IQIgnore]
        public IQEventType SavedEventType;
        public string Breed { get; set; }
        public int PK { get; set; }
    }
    [IQClass]
    public class Cat : Dog, IQObject
    {
        public bool? Declawed { get; set; }
    }

    [IQClass]
    public class Wolf: Animal, IQObject
    {
        protected virtual void Constructor(IClassInfoConstructor data)
        {
            data.Query.TableName = "animals";
            data.Query.PrimaryKey = "PK";
        }
        public int PK { get; set; }

        [IQField(SqlName = "Breed")]
        public string BreedName { get; set; }
    }
}
