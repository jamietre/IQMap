using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Data;
using IQMap.Impl;
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;
using IQMap.Impl.Support;
using IQObjectMapper;
using System.Dynamic;

namespace IQMap.Impl
{
    /// <summary>
    /// A generic implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Query:  Query<dynamic>, IQuery 
    {
        public Query(IDbContext context, ISqlQuery queryDef) :
            base(context,queryDef)
        {


        }
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        public Query(IDbContext context, Query query) :
            base(context, query)
        {
            
          
        }


       
    }

  
}
