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

namespace IQMap.SqlQueryBuilder.Impl
{
 
    /// <summary>
    /// Enumerates the results of a strongly-typed query. This is the main IQ object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryBuilder<T>: Query<T>, IQueryBuilder<T>,  IQueryBuilder where T: class
    {

        # region constructors

        public QueryBuilder(IDbContext context, IQueryOptions options = null) :
            base(context)
        {
            Options(options);
        }
        public QueryBuilder(IDbContext context, Query<T> template, IQueryOptions options = null) :
            base(context, template)
        {
            Options(options);
        }

        /// <summary>
        /// This constructor is for creating a QueryBuilder from a runtime type. At some point this should be refactored
        /// so that a nongeneric QueryBuilder object is used instead, but the use cases are mostly internal and this
        /// would be a bunch of work with only stylistic payoff for now. Instead we just use the nongeneric interface 
        /// and instantiate QueryBuilder-object- for those use cases.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sourceObject"></param>
        /// <param name="options"></param>

        public QueryBuilder(IDbContext context, object sourceObject, IQueryOptions options = null) :
            base(context)
        {
            if (typeof(T) != typeof(object))
            {
                throw new InvalidOperationException("QueryBuilder must be instantiated with a source object when generically typed as -object-");
            }
            ClassInfo = IQ.ClassInfo(sourceObject.GetType());
            Options(options);
            
        }
        
        #endregion

        #region private properties


        public override IClassInfo ClassInfo
        {
            get
            {
                

                return base.ClassInfo;
            }
            protected set
            {
                if (!value.IsBound)
                {
                    throw new InvalidOperationException(String.Format("The type \"{0}\" is not bound to a database table; you can run queries, but you can't use the query builder.", base.ClassInfo.Type.ToString()));
                }
                base.ClassInfo = value;
            }

        }
        protected new ISqlQueryMaker GetQuery()
        {
            return (ISqlQueryMaker)base.QueryDef.Clone();
        }


        protected ISqlQueryMaker _QueryDef;
        protected override ISqlQuery InnerQueryDef
        {
            get
            {
                if (_QueryDef == null)
                {
                    _QueryDef = ClassInfo.GetQuery();


                }
                return _QueryDef;
            }
            set
            {
                _QueryDef = (ISqlQueryMaker)value;
            }
        }


        #endregion

        #region public properties

        public new ISqlQueryMaker QueryDef
        {
            get
            {
                return (ISqlQueryMaker)InnerQueryDef;
            }
            set
            {
                InnerQueryDef = value;
            }
        }

        #endregion

        #region public methods

        public IEnumerable<string> Properties
        {
            get {
                return ClassInfo.Data.Keys;
            }
        }

        public IEnumerable<string> Columns
        {
            get {
                HashSet<string> columns = new HashSet<string>(QueryDef.Select.SplitClean(','));
                if (columns.Contains("*"))
                {
                    throw new InvalidOperationException("Columns is not accesible when '*' is part of your select clause.");
                }
                foreach (string col in columns)
                {
                    yield return ColumnOutputName(col);
                }

            }
        }

        public IQueryBuilder<T> Options(IQueryOptions options)
        {
            if (options != null)
            {
                Options(options.TableName, options.PrimaryKey);
            }
            return this;
        }
        public IQueryBuilder<T> Options(string tableName = null, string primaryKey = null)
        {
            if (!String.IsNullOrEmpty(primaryKey))
            {
                QueryDef.PrimaryKey = primaryKey;
            }
            if (!String.IsNullOrEmpty(tableName))
            {
                QueryDef.TableName = tableName;
            }
            return this;
        
        }

        public override bool Any()
        {
            return Take(1).Enumerate().Count() == 1;
        }
        public bool Any(string where, params object[] parameters)
        {
            return Where(where, parameters).Any();
        }
        public override bool One()
        {
            return Take(2).Enumerate().Count() == 1;
        }
        public bool One(string where, params object[] parameters)
        {
            return Where(where, parameters).One();
        }
        /// <summary>
        /// Asserts that the item is contained in the database by comparing primary keys only. This does
        /// not assert that the contents match.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            return this.Where(QueryBuilder.WhereEquals(ClassInfo.Query.PrimaryKey,ClassInfo.PrimaryKeyField.GetValue(item))).Any();
        }
        public override T First()
        {
            return Take(1).Enumerate().First();
        }
        public T First(string where, params object[] parameters)
        {
            return Where(where, parameters).First();
        }
        public override T Single()
        {
            return Take(2).Enumerate().Single();
        }
        public T Single(string where, params object[] parameters)
        {
            return Where(where, parameters).Single();
        }
        public override T SingleOrDefault()
        {
            return Take(2).Enumerate().SingleOrDefault();
        }
        public T SingleOrDefault(string where, params object[] parameters)
        {
            return Where(where, parameters)
                .Take(2)
                .Enumerate()
                .SingleOrDefault();
        }
        
        public override T SingleOrNew()
        {
            return base.SingleOrNew(Take(2));

        }
        public T SingleOrNew(string where, params object[] parameters)
        {
            return base.SingleOrNew(Where(where, parameters).Take(2));
        }


        public override IEnumerable<T> Enumerate()
        {
            // do we need to overried this? Will this handle itself differently?
            return Clone();
        }

        public IQueryBuilder<T> Take(int rows)
        {
       
            if (rows < 0)
            {
                throw new ArgumentOutOfRangeException("rows cannot be less than 0.");
            }

            var query = Clone();
            var curTotal = query.QueryDef.TotalRows;

            query.QueryDef.TotalRows = curTotal == 0 ?
                rows :
                Math.Min(curTotal, rows);
            
            return query;
        }
        
        public IQueryBuilder<T> TakeWhile(string where, params object[] parameters)
        {
            return Where(where, parameters);
        }


        public int Count(string where, params object[] parameters)
        {
            return Where(where, parameters).Count();
        }



        public override T Last()
        {
            if (CanReverse)
            {
                return Reverse().First();
            }
            else
            {
                return base.Last();
            }
        }
        public T Last(string where, params object[] parameters)
        {
            return Where(where, parameters).Last();
        }

        public override T LastOrDefault()
        {
            if (CanReverse)
            {
                return Reverse().FirstOrDefault();
            }
            else
            {
                return base.LastOrDefault();
            }
        }
        public T LastOrDefault(string where, params object[] parameters)
        {
            return Where(where,parameters).LastOrDefault();
        }
        public override T ElementAt(int position)
        {
            T output;
            
            if (Skip(position).Take(1).TryFirst(out output)) 
            {
                return output;
            } else {
                throw new ArgumentOutOfRangeException("There was no element at position " + position);
            }
        }

        public override T ElementAtOrDefault(int position)
        {
            return Skip(position).FirstOrDefault();
        }


        public override T FirstOrDefault()
        {
            return Take(1).Enumerate().FirstOrDefault();
        }
        public T FirstOrDefault(string where, params object[] parameters)
        {
            return Where(where,parameters).FirstOrDefault();
        }

        public override T FirstOrNew()
        {
             T output;
            
            if (Take(1).TryFirst(out output)) 
            {
                return output;
            } else {
                return Types.GetInstanceOf<T>();
            }

        }
        public T FirstOrNew(string where, params object[] parameters)
        {
            return Where(where, parameters).FirstOrNew();
        }
        public override T LastOrNew()
        {
            if (CanReverse)
            {
                return Reverse().FirstOrNew();
            }
            else
            {
                return base.LastOrNew();
            }
        }
        public T LastOrNew(string where, params object[] parameters)
        {
            return Where(where, parameters).LastOrNew();
        }

        public IQueryBuilder<T> OrderBy(string order)
        {
            var query = Clone();
            query.QueryDef.OrderBy.Clear();
            query.QueryDef.OrderBy.Add(order);
            return query;
        }

        public IQueryBuilder<T> Select(string select, params object[] parameters)
        {
            ParameterParser pp = new ParameterParser(select, parameters);

            var query = Clone();
            query.QueryDef.Select = pp.GetQuery(QueryType.Invalid);
            query.QueryDef.AddParameter(pp.Parameters);
            return query;
        }

        public IQueryBuilder<T> Skip(int rows)
        {
            if (rows < 0)
            {
                throw new ArgumentOutOfRangeException("You can't skip backwards; rows must be 0 or positive.");
            }
            if (rows == 0)
            {
                return this;
            }
            var query = Clone();
            query.QueryDef.FirstRow+=rows;
            return query;

        }

        public IQueryBuilder<T> Reverse()
        {
            // We can only reverse by changing the OrderBy clause if no limits yet exist. This is because "reverse" still
            // applies to all results. Once a limit has been set, revers should only apply to the subset, and we can't
            // build a query like that, so defer it to linq.

            if (CanReverse)
            {
                var query = Clone();
                query.QueryDef.OrderBy.Reverse();
                return query;
            }
            else
            {
                throw new InvalidOperationException("You can't reverse query once the selection set has been limited using Take or Skip. Use Enumerate() to access the Linq Reverse()");
            }
        }

        public bool CanReverse
        {
            get
            {
                return QueryDef.FirstRow == 0 && QueryDef.TotalRows == 0 && !QueryDef.OrderBy.IsEmpty;
            }
        }

        protected T OptimizedLast()
        {
            if (CanReverse)
            {
                return Reverse().First();
            }
            else
            {
                return Enumerate().Last();
            }
        }



        public IQueryBuilder<T> OrderByDescending(string order)
        {
            var clone = Clone();
            clone.QueryDef.OrderBy.Clear();
            clone.QueryDef.OrderBy.Add(order, SortOrder.Descending);
            return clone;
        }

        public IQueryBuilder<T> ThenBy(string order)
        {
            var clone = Clone();
            clone.QueryDef.OrderBy.Add(order, SortOrder.Ascending);
            return clone;
        }

        public IQueryBuilder<T> ThenByDescending(string order)
        {
            var clone = Clone();
            clone.QueryDef.OrderBy.Add(order, SortOrder.Descending);
            return clone;
        }

        public IQueryBuilder<T> GroupBy(string group)
        {
            var clone = Clone();
            clone.QueryDef.GroupBy = group;
            return clone;
        }

        public IQueryBuilder<T> Having(string group)
        {
            var clone = Clone();
            clone.QueryDef.Having = group;
            return clone;
        }

        public IQueryBuilder<T> Where(string where, params object[] parameters)
        {
            var parser = new ParameterParser(where, parameters);


            var query = Clone();
            query.QueryDef.Where.Add(parser.GetWhereClause());

            return query;
        }

        public IQueryBuilder<T> Where(int pkValue)
        {
            var query = Clone();
            query.QueryDef.Where.Add(QueryDef.PrimaryKey, pkValue, ComparisonType.Equal);
            return query;
        }

        public int Delete()
        {
            var deleteQuery = QueryDef.Clone(QueryType.Delete);
            return Context.DataStorageController.RunQueryScalar(Context.Connection, deleteQuery, Context.Transaction, Context.CommandBehavior);
        }

        public int Delete(string where, params object[] parameters)
        {
            return Where(where, parameters).Delete();
        }

        public new IQueryBuilder<T> Clone()
        {
            return (IQueryBuilder<T>)CloneImpl();
        }
        protected override Query<T> CloneImpl()
        {
            var qb = new QueryBuilder<T>(Context, this);
            return CopyTo(qb);
        }



        #endregion

        #region private methods

        /// <summary>
        /// Removes the prefix or uses "as" for an SQL column. This is temporary, once we are using the "SelectItem" instead of a string
        /// for select it won't be needed
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        protected string ColumnOutputName(string col)
        {
            string colLow = col.ToLower();
            int selectAs = col.IndexOf(" as ");
            if (selectAs > 0)
            {
                return col.Substring(selectAs + 4).Trim();
            } 

            int dotPos = col.LastIndexOf(".");
            if (dotPos>=0) {
                return col.Substring(dotPos+1);
            }
            return col;
        }

        protected override IEnumerator<T> GetEnumerator(DbBuffering buffering, T target = default(T))
        {
            QueryEnumerator<T> enumerator = (QueryEnumerator<T>)base.GetEnumerator(buffering, target);
            enumerator.OnLoad = OnLoad;
            return enumerator;
        }
        


        #endregion

        #region IQueryBuilder interface

        IQueryBuilder IQueryBuilder.Where(string where, params object[] parameters)
        {
            return Where(where, parameters);
        }

        IQueryBuilder IQueryBuilder.Where(int pkValue)
        {
            return Where(pkValue);
        }

        IQueryBuilder IQueryBuilder.Select(string select, params object[] parameters)
        {
            return Select(select, parameters);
        }

        IQueryBuilder IQueryBuilder.OrderBy(string order)
        {
            return OrderBy(order);
        }

        IQueryBuilder IQueryBuilder.OrderByDescending(string order)
        {
            return OrderByDescending(order);
        }

        IQueryBuilder IQueryBuilder.ThenBy(string order)
        {
            return ThenBy(order);
        }

        IQueryBuilder IQueryBuilder.ThenByDescending(string order)
        {
            return ThenByDescending(order);
        }

        IQueryBuilder IQueryBuilder.GroupBy(string group)
        {
            return GroupBy(group);
        }

        IQueryBuilder IQueryBuilder.Having(string group)
        {
            return Having(group);
        }


        IQueryBuilder IQueryBuilder.Reverse()
        {
            return Reverse();
        }

        IQueryBuilder IQueryBuilder.Skip(int rows)
        {
            return Skip(rows);
        }

        IQueryBuilder IQueryBuilder.Take(int rows)
        {
            return Take(rows);
        }

        IQueryBuilder IQueryBuilder.Options(string tableName, string primaryKey)
        {
            return Options(tableName, primaryKey);
        }

        IQueryBuilder IQueryBuilder.Options(IQueryOptions options)
        {
            return Options(options);
        }

        bool IQueryBuilder.Contains(object item)
        {
            return Contains((T)item);
        }

        object IQueryBuilder.Single(string where, params object[] parameters)
        {
            return Single(where, parameters);
        }

        object IQueryBuilder.SingleOrDefault(string where, params object[] parameters)
        {
            return SingleOrDefault(where, parameters);
        }

        object IQueryBuilder.SingleOrNew(string where, params object[] parameters)
        {
            return SingleOrNew(where, parameters);
        }

        object IQueryBuilder.First(string where, params object[] parameters)
        {
            return First(where, parameters);
        }

        object IQueryBuilder.FirstOrDefault(string where, params object[] parameters)
        {
            return FirstOrDefault(where, parameters);

        }

        object IQueryBuilder.FirstOrNew(string where, params object[] parameters)
        {
            return FirstOrNew(where, parameters);
        }

        object IQueryBuilder.Last(string where, params object[] parameters)
        {
            return Last(where, parameters);
        }

        object IQueryBuilder.LastOrDefault(string where, params object[] parameters)
        {
            return LastOrDefault(where, parameters);
        }

        object IQueryBuilder.LastOrNew(string where, params object[] parameters)
        {
            return LastOrNew(where, parameters);
        }

        IQueryBuilder IQueryBuilder.Clone()
        {
            return Clone();
        }

        #endregion


    }

}
