using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IQMap
{
 [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IQIgnore: Attribute 
    {
    }
}