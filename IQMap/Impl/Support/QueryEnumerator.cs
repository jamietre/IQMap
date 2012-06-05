using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Data;
using IQObjectMapper;
using IQObjectMapper.Adapters;
using IQMap.SqlQueryBuilder;
using IQMap.QueryCache;

namespace IQMap.Impl.Support
{
    
    /// <summary>
    /// The enumerator for Query`T` -- casts the result of a database query to a typed object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryEnumerator<T> : IEnumerator<T> 
    {
      
        public QueryEnumerator(IDbConnection connection, IDataStorageController dataController, ISqlQuery query)
        {
            Connection = connection;
            DataStorageController = dataController;
            Query = query;
            Buffered = DbBuffering.Buffered;
        }
        public QueryEnumerator(QueryEnumerator<T> cloneFrom)
        {
            Connection = cloneFrom.Connection;
            DataStorageController = cloneFrom.DataStorageController;
            Query = cloneFrom.Query;
            Buffered = cloneFrom.Buffered;
            Data = cloneFrom.Data;
        }
        #region private properties
  

        protected IDbConnection Connection;
        protected IDataStorageController DataStorageController;
        protected IDataReader DataReader;


        //protected DataReaderWrapper DataReaderWrapper;
        // will be assigned either a cache, or the datareader wrapper
        protected IList<T> _Cache;
        protected IEnumerable<T> _Data;
        protected IList<T> Cache { 
            get
            {
                return new ReadOnlyCollection<T>(_Cache);
            
            }
             set
            {
                _Cache = value;
            } 
        }
        protected IEnumerable<T> Data
        {
            get
            {
                return _Data;
            }
            set
            {
                _Data = value;
                _Enumerator = _Data.GetEnumerator();
            }
        }
        protected IEnumerator<T> _Enumerator;
        protected IEnumerator<T> Enumerator
        {
            get
            {
                if (_Data == null)
                {
                    RunQuery();
                }
                return _Enumerator;
            }
        }

        protected CommandBehavior _CommandBehavior;
        protected T _Current;

        #endregion

        #region public properties


        public ISqlQuery Query;
        /// <summary>
        /// An existing object to be the target of the map
        /// </summary>
        public T Target { get; set; }
        /// <summary>
        /// An action which is called when the data is actually loaded into an object to be returned
        /// </summary>
        public Action<T> OnLoad { get; set; }
        public DbBuffering Buffered { get; set; }
        public IDbTransaction Transaction { get; set; }
        public CommandBehavior CommandBehavior
        {
            get
            {
                if (_CommandBehavior == 0)
                {
                    return Transaction != null ?
                        CommandBehavior.Default :
                        CommandBehavior.CloseConnection;
                }
                else
                {
                    return _CommandBehavior;
                }
            }
            protected set
            {
                _CommandBehavior = value;
            }
        }

        #endregion

       
        public T Current
        {
            get
            {
                return Enumerator.Current;
            }
        }

        public void Dispose()
        {
            if (DataReader != null)
            {
                DataReader.Dispose();
            }
            DataReader = null;
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            bool next = Enumerator.MoveNext();
            if (!next && Buffered == DbBuffering.Unbuffered)
            {
                Dispose();
            }
            return next;
        }

        public void Reset()
        {
            if (_Enumerator != null)
            {
                if (Buffered != DbBuffering.Buffered)
                {
                    Dispose();
                    _Enumerator = null;
                }
                else
                {
                    Enumerator.Reset();                    
                }
            }
        }

        protected void RunQuery()
        {
            IDataReader DataReader;
            if (Buffered!=DbBuffering.Buffered || !SqlQueryBuilderConfig.TryGetCachedData(Query, out DataReader))
            {
                DataReader = DataStorageController.RunQuery(Connection, Query, Transaction, CommandBehavior);
                DataReader = SqlQueryBuilderConfig.AddToCache(Query,DataReader);
            }
           
            // datareaderadapter will return a new Dictionary<string,object> when T is IDictionary<string,object>
            // we must map to an interim structure because of name-changes via SqlName
            
            var cInfo = IQ.ClassInfo<T>();

            var dra = new DataReaderAdapter<T>(DataReader, 
                cInfo != null ? 
                    new Options { FieldNameMap = cInfo.SqlNameMap }:
                    null);

            dra.OnLoad = OnLoad;
            dra.Target = Target;



            //IEnumerable<T> wrapper = GetFinalSequence(dra);
           
            if (Buffered==DbBuffering.Buffered)
            {
                var list = new List<T>();
                list.AddRange(dra);
                Data = list;
            }
            else
            {
                Data = dra;
            }   
        }

    }
}
