using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IQMap
{
    [Flags]
    public enum IQEventType
    {
        Save = 1,
        Insert=2,
        Update=4,
        Load=8,
        Delete=16
    }
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class IQEventHandler: Attribute 
    {
        
    }
}