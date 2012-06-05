using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IQMap.Impl.Support;

namespace IQMap.Impl
{
    /// <summary>
    /// Basic typed implementation of the abstract Query-T-
    /// This will not allow IQueryBuilder methods but will still return strongly-typed data. This is used in situations 
    /// where you have real POCO (e.g. there is no SQL metadata) and you just want to cast the output of a query from a different
    /// IQObject. 
    /// 
    /// Put another way, this is only used by the As-T- method, but it could be used elsewhere.
    /// </summary>
    /// <typeparam name="T">The target type</typeparam>
    /// <typeparam name="U">The original type</typeparam>
    public class TypedQuery<T,U> : Query<T>
    {
        public TypedQuery(IDbContext context, ISqlQuery queryDef) :
            base(context, queryDef)
        {


        }
        /// </summary>
        /// <param name="context"></param>
        /// <param name="query"></param>
        public TypedQuery(IDbContext context, Query<T> query) :
            base(context, query)
        {


        }

        protected override IEnumerator<T> GetEnumerator(DbBuffering buffering, T target = default(T))
        {
            
            if (typeof(T)==typeof(IDictionary<string,object>)) {
                //TODO: Target is going to be assigned to the wrong record for ElementAt and Take because it always uses the first
                // in querybuilder, we always select the first one directly.
                if (target != null)
                {
                    throw new InvalidOperationException("Mapping to a target object in the same operation as AsDictionary is not supported.");
                }
                var enumerator = new QueryEnumerator_Dictionary<U>(Context.Connection, Context.DataStorageController, GetQuery());
                enumerator.Buffered = buffering;

                // Do not assign OnLoad event in QueryT because this should only run for nontyped/nonbound objects
                // Override in QueryBuilderT to assign the event handler.

                Enumerator = (IEnumerator<T>)enumerator;

                return Enumerator;
            }else {
                return base.GetEnumerator(buffering,target);
            }
          

        }
        
    }
}
