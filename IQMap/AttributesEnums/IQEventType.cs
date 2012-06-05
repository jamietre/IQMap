using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap
{
    public enum IQEventType
    {
        BeforeSave = 1,
        OnSave = 2,
        BeforeUpdate = 3,
        OnUpdate=4,
        BeforeInsert=5,
        OnInsert=6,
        BeforeDelete=7,
        OnDelete=8,
        OnLoad = 9
    }
}
