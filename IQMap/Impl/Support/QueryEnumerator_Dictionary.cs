using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQObjectMapper;
using IQObjectMapper.Adapters;

namespace IQMap.Impl.Support
{
    public class QueryEnumerator_Dictionary<T> : IEnumerator<IDictionary<string, object>>
    {
        public QueryEnumerator_Dictionary(IDbConnection connection, IDataStorageController controller, ISqlQuery query)
        {
            InnerEnumerator = new QueryEnumerator<T>(connection, controller, query);
        }
        
        protected QueryEnumerator<T> InnerEnumerator;

        #region private properties

        private static Options DictionaryAdapterOptions = new Options { 
            CanAlterProperties = false, 
            UpdateSource = false 
        };

        #endregion

        #region public properties

        public DbBuffering Buffered
        {
            get
            {
                return InnerEnumerator.Buffered;
            }
            set
            {
                InnerEnumerator.Buffered = value;
            }
        }
        public T Target
        {
            get
            {
                return InnerEnumerator.Target;
            }
            set
            {
                InnerEnumerator.Target = value;
            }
        }

        #endregion

        public IDictionary<string, object> Current
        {
            get {
                return new PropertyDictionaryAdapter(InnerEnumerator.Current,DictionaryAdapterOptions);
            }
        }

        public void Dispose()
        {
            InnerEnumerator.Dispose();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            return InnerEnumerator.MoveNext();
        }

        public void Reset()
        {
            InnerEnumerator.Reset();
        }

        #region private  method

        #endregion
    }
}
