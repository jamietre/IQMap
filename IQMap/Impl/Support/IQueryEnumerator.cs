using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.Impl.Support
{
    public interface IQueryEnumerator
    {
       
        DbBuffering Buffered { get; set; }
        IDbTransaction Transaction { get; set; }
        CommandBehavior CommandBehavior { get; set; }
    }
    public interface IQueryEnumerator<T>: IQueryEnumerator
    {
        T Target { get; set; }
        Action<T> OnLoad { get; set; }
    }
}
