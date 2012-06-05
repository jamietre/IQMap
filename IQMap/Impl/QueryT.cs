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

namespace IQMap.Impl
{
    /// <summary>
    /// Enumerates the results of a strongly-typed query. This is the main object that users interact with. This implemenation should implement
    /// all methods in the most efficient way assuming we cannot rewrite the query. However every method should still work. The IQueryBuilder-t-
    /// subclass allows rewriting the query to optimize it specifically for the operation that is constructed by the methods used before the
    /// query is run in a SQL Server-implementation specific way.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Query<T> : IQuery<T>,  IQuery
    //,IQueryable<T>
    {
        #region constructors

        public Query(IDbContext context)
        {
            Context = context;

        }
        public Query(IDbContext context,Query<T> template)
        {
            Context = context;
            QueryDef = template.QueryDef.Clone();
            template.CopyTo(this);
        }

        public Query(IDbContext context, ISqlQuery queryDef)
        {
            Context = context;
            QueryDef = queryDef;
        }
        #endregion

        #region private properties


        /// <summary>
        /// The active enumerator; we keep a ref so we can dispose of it.
        /// </summary>
        protected  IEnumerator<T> Enumerator;
        protected virtual IEnumerator<T>  GetEnumerator(DbBuffering buffering, T target = default(T))
        {

            //TODO: Target is going to be assigned to the wrong record for ElementAt and Take because it always uses the first
            // in querybuilder, we always select the first one directly.

            var enumerator = new QueryEnumerator<T>(Context.Connection, Context.DataStorageController, GetQuery());
            enumerator.Buffered = buffering;
            enumerator.Target = Target;

            // Do not assign OnLoad event in QueryT because this should only run for nontyped/nonbound objects
            // Override in QueryBuilderT to assign the event handler.

            Enumerator = enumerator;
            return enumerator;
          
        }      
        protected void OnLoad(T obj) 
        {
            if (ClassInfo != null)
            {
                ClassInfo.DoEvent(obj, IQEventType.OnLoad, Context);
            }
        }
        private bool TriedGetClassInfo = false;
        private IClassInfo _ClassInfo;
        public virtual IClassInfo ClassInfo 
        {
            get {
                if (!TriedGetClassInfo)
                {
                    _ClassInfo = IQ.MapperCache.GetClassInfo(typeof(T));
                    TriedGetClassInfo = true;
                }
                return _ClassInfo;
            }
            protected set
            {
                _ClassInfo = value;
                TriedGetClassInfo = value!=null;
            }
        }

        protected virtual ISqlQuery InnerQueryDef
        {
            get;
            set;

        }
        #endregion

        #region public properties
        
        public virtual IDbContext Context
        {
            get;
            set;
        }
        
        public IQuery<T> Clone()
        {
            return CloneImpl();
        }
        protected virtual Query<T> CloneImpl()
        {
            // since we can't re-implement abstract methods with a different type, we "new" for the public method
            // and require implementation in the derived class for the inner method

            throw new InvalidOperationException("This method must be overridden in the derived class.");
        }
        // copy _ClassInfo when cloning because it contains information that is instance-specific when typed as -object- and it's also
        // more efficient that recreating each time.

        protected virtual Query<T> CopyTo(Query<T> target)
        {
            target.ClassInfo = _ClassInfo;
            target.QueryDef = QueryDef.Clone();
            target.Target = Target;
            return target;
        }
        protected virtual void CopyFrom(Query<T> source)
        {

            QueryDef = source.QueryDef.Clone();
            ClassInfo = source._ClassInfo;
            Target = source.Target;
        }


        /// <summary>
        /// An existing object that should be the target of a final map operation
        /// </summary>
        public T Target { get; set; }
        
        public ISqlQuery QueryDef
        {
            get { return InnerQueryDef; }
            protected set { InnerQueryDef = value; }
        }

        #endregion

        #region public methods


        protected virtual ISqlQuery GetQuery()
        {
            return QueryDef.Clone();
        }

       


        /// <summary>
        /// Map to a different type than the bound type of this query
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual IQuery<U> As<U>()
        {
            return new TypedQuery<U,T>(this.Context, this.QueryDef);
        }

        public virtual IQuery<IDictionary<string, object>> AsDictionaries()
        {
            return new TypedQuery<IDictionary<string, object>,T>(this.Context, this.QueryDef);
        }

        public IQuery<T> To(T target)
        {
            var clone = Clone();
            clone.Target = target;
            return clone;
        }
        public virtual bool Any()
        {
            using (var enumerator = GetEnumerator(DbBuffering.Unbuffered))
            {
                return enumerator.MoveNext();
            }
        }
        public virtual bool One()
        {
            using (var enumerator = GetEnumerator(DbBuffering.Unbuffered))
            {
                bool one = enumerator.MoveNext();
                return one && !enumerator.MoveNext();
            }
        }
        // The madness with GetEnumerator().Enumerate() is thus.
        // We don't want to just return "this" because we need to change the buffered setting to ensure that we don't recall
        // the whole query. Likewise why bother cloning the whole object when all we need is a specialized enumerator.
        // so we just have a method on the enumerator to enumerate itself, which also disposes of its owner, allowing us
        // to use unbuffered reads chained to the regular linq methods.

        public virtual T First()
        {
            return GetEnumerable(GetEnumerator(DbBuffering.Unbuffered)).First();
        }

        public virtual T Single()
        {
            return  GetEnumerable(GetEnumerator(DbBuffering.Unbuffered)).Single();
            
        }
        public virtual T SingleOrDefault()
        {
            return GetEnumerable(GetEnumerator(DbBuffering.Unbuffered)).SingleOrDefault();
            
        }
        public virtual T SingleOrNew()
        {
            var enumer= GetEnumerable(GetEnumerator(DbBuffering.Unbuffered));
             return  SingleOrNew(enumer);
            
        }
        protected T SingleOrNew(IEnumerable<T> enumerable)
        {
            T obj=default(T);
            bool hasItem = false;
          
            foreach (var item in enumerable)
            {
                if (hasItem)
                {
                    throw new InvalidOperationException("More than one item in was found in the sequence.");
                }
                hasItem = true;
                obj = item;
            }
            
            if (!hasItem)
            {
                obj = Types.GetInstanceOf<T>();
            }
            return obj;
        }


        public virtual IEnumerable<T> Enumerate()
        {
            return this;
        }


        public virtual int Count()
        {
            return Context.DataStorageController.Count(Context.Connection, GetQuery(), Context.Transaction, Context.CommandBehavior);
        }

        public virtual T Last()
        {
            return GetEnumerable(GetEnumerator(DbBuffering.Unbuffered)).Last();
        }

        public virtual T LastOrDefault()
        {
            return GetEnumerable(GetEnumerator(DbBuffering.Unbuffered)).LastOrDefault();
        }

        public virtual T ElementAt(int position)
        {
            return GetEnumerable(GetEnumerator(DbBuffering.Unbuffered)).ElementAt(position);
            
        }

        public virtual T ElementAtOrDefault(int position)
        {
            return GetEnumerable(GetEnumerator(DbBuffering.Unbuffered)).ElementAtOrDefault(position);
            
        }


        public virtual T FirstOrDefault()
        {
            return GetEnumerable(GetEnumerator(DbBuffering.Unbuffered)).FirstOrDefault();
            
        }


        public virtual T FirstOrNew()
        {

            var enumer = GetEnumerable(GetEnumerator(DbBuffering.Unbuffered));
            return FirstLastOrNew(enumer, true);
            
        }
        protected T FirstLastOrNew(IEnumerable<T> enumerable, bool first)
        {
            T obj = default(T);
            bool hasItem = false;
            foreach (var item in enumerable)
            {
                hasItem = true;
                obj = item;
                if (first)
                {
                    break;
                }
            }
            if (!hasItem)
            {
                obj = Types.GetInstanceOf<T>();
            }
            return obj;
        }
        public virtual T LastOrNew()
        {

            var enumer = GetEnumerable(GetEnumerator(DbBuffering.Unbuffered));

            return FirstLastOrNew(enumer, false);
            
        }

        public override string ToString()
        {
            return GetQuery().ToString();
        }
        public IEnumerator<T> GetEnumerator()
        {

            return GetEnumerator(Context.Buffering);
        }
        public void Dispose()
        {
            if (Enumerator != null)
            {
                Enumerator.Dispose();
            }
        }

        // TODO: Recode these to use enumerator instead of try/catch
        public bool TrySingle(out T item)
        {
            try
            {
                item = Single();
                return true;

            }
            catch
            {
                item = default(T);
                return false;
            }

        }
        public bool TryFirst(out T item)
        {
            try
            {
                item = First();
                return true;

            }
            catch
            {
                item = default(T);
                return false;
            }
        }
        public bool TryLast(out T item)
        {
            try
            {
                item = Last();
                return true;

            }
            catch
            {
                item = default(T);
                return false;
            }
        }

        #endregion

        #region private methods

        protected IEnumerable<T> LinqThis()
        {

            foreach (var item in this)
            {
                yield return item;
            }

        }
       
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerable IQuery.Enumerate()
        {
            return Enumerate();
        }


        /// <summary>
        /// Turn an enumerator on its head
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        protected IEnumerable<T> GetEnumerable(IEnumerator<T> enumerator)
        {
            using (enumerator)
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
            
        }
        #endregion

        #region IQuery interface

        object IQuery.Single()
        {
            return Single();
        }

        object IQuery.SingleOrDefault()
        {
            return SingleOrDefault();
        }

        object IQuery.SingleOrNew()
        {
            return SingleOrNew();
        }

        object IQuery.First()
        {
            return First();
        }

        object IQuery.FirstOrDefault()
        {
            return FirstOrDefault();
        }

        object IQuery.FirstOrNew()
        {
            return FirstOrNew();
        }

        object IQuery.Last()
        {
            return Last();
        }

        object IQuery.LastOrDefault()
        {
            return LastOrDefault();
        }

        object IQuery.LastOrNew()
        {
            return LastOrNew();
        }

        object IQuery.ElementAt(int position)
        {
            return ElementAt(position);
        }

        object IQuery.ElementAtOrDefault(int position)
        {
            return ElementAtOrDefault(position);
        }

        #endregion


        public Type ElementType
        {
            get { return typeof(T); }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get { throw new NotImplementedException(); }
        }

        public IQueryProvider Provider
        {
            get { throw new NotImplementedException(); }
        }

    }

}
