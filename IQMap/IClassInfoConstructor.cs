using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;

namespace IQMap
{
    public interface IClassInfoConstructor
    {
        /// <summary>
        /// When true, queries will not attempt to select individual fields but use *.
        /// </summary>
        //bool SelectAll { get; set; }
        /// <summary>
        /// When true, instances of this object will be tracked automatically
        /// </summary>
        bool Track { get; set; }
        ISqlQueryMaker Query { get; set; }

        void AddEventHandler(IQEventType eventType, Action<IEventData> handler);

    }
}
