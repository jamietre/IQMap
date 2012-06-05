using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap
{
    //public interface IQable : IEnumerable
    //{
    //    SqlQuery.IQuery Query;
    //}
    //public interface IQable<out T> : IEnumerable<T>, IEnumerable, IQable
    //{
          
    //}
    public interface IQObject
    {
       // bool Validate();
    }
    /// <summary>
    /// An interface used as a return type for internal cases where we only care if something exists
    /// </summary>
    public interface IQTestOnly
    {

    }
}
