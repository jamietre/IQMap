using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap
{
    public interface IEventData
    {
        object Source { get; }
        bool Tracked { get; }
        IQEventType EventType { get; }
        IDbContext QueryController { get; }
        IObjectData ObjectData { get;  }
        IClassInfo ClassInfo { get;  }
        bool Cancel { get; set; }
        
    }

    public interface IEventData<T>: IEventData
    {
        new T Source { get; }
    }
}
