using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap
{
    [Flags]
    public enum DbCommandOptions
    {
        CloseConnection=1,
        LeaveConnetion=2

    }
}
