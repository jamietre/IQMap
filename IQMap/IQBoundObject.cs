using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap
{
    public abstract class IQObjectPrototype
    {
        protected abstract void IQConstructor(IClassInfoConstructor dbInfo);

        //protected abstract void BeforeUpdate(IEventData args);
        //protected abstract void AfterUpdate(IEventData args);

        //protected abstract void BeforeInsert(IEventData args);
        //protected abstract void AfterInsert(IEventData args);

        //protected abstract void BeforeSave(IEventData args);
        //protected abstract void AfterSave(IEventData args);
        
        //protected abstract void BeforeDelete(IEventData args);
        //protected abstract void AfterDelete(IEventData args);

        //protected abstract void BeforeLoad(IEventData args);
        //protected abstract void AfterLoad(IEventData args);
    }
}
