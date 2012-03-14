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
        Delete=16,
        Before = 32,
        After = 64
    }
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public class IQEventHandler: Attribute 
    {
        
    }
}