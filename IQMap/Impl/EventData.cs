using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.Impl
{
    public class ObjectInfo: IEventData
    {
        public ObjectInfo(IQEventType eventType,IDbContext queryController, IObjectData data, IClassInfo info, object obj)
        {
            EventType = eventType;
            QueryController = queryController;
            ObjectData = data;
            ClassInfo = info;
            Source = obj;
        }
        public object Source { get; protected set; }
        public IQEventType EventType { get; protected set; }
        public bool Tracked { get { return ObjectData != null; } }
        public IDbContext QueryController { get; protected set; }
        public IObjectData ObjectData { get; protected set; }
        public IClassInfo ClassInfo { get; protected set; }
        public bool Cancel { get; set; }
    }
    public class ObjectInfo<T> : ObjectInfo, IEventData<T>
    {
        public ObjectInfo(IQEventType eventType, IDbContext queryController, IObjectData data, IClassInfo info, T obj):
            base(eventType,queryController,data,info,obj)
        {

        }
        public new T Source
        {
            get
            {
                return (T)base.Source;
            }
        }
    }
}
