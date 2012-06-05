using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IQMap.Impl.Support
{
    /// <summary>
    /// Exposes a read-only collection from a set of IEnumerable-T-
    /// </summary>
    public class LazyReadOnlyCollection<T>: ICollection<T>
    {
        public LazyReadOnlyCollection(IEnumerable<IEnumerable<T>> sources)
        {
            Sources = sources;
            EnumeratorFunc = EnumerateCollections;
        }
        public LazyReadOnlyCollection(params T[] objects) {
            Objects = objects;
            EnumeratorFunc = EnumerateObjects;
        }
        protected IEnumerable<IEnumerable<T>> Sources;
        protected T[] Objects;
        // when an operation causing access to the whole list is performed, cache it for future use.
        // need to figure out how this should deal with duplicates, we don't actually want them but
        // to be lazy it is impossible to verify in advance/

        //protected HashSet<T> _Cache;

        public void Add(T item)
        {
            throw new InvalidOperationException("This is a read-only collection.");
        }

        public void Clear()
        {
            throw new InvalidOperationException("This is a read-only collection.");
        }

        public bool Contains(T item)
        {
            return EnumeratorFunc().Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int index = 0;
            foreach (var item in EnumerateCollections())
            {
                array[arrayIndex + index++] = item;
            }
        }

        public int Count
        {
            get {
                //_Cache = new HashSet<T>(EnumerateCollections());
                //return _Cache.Count;
                return EnumeratorFunc().Count();
            }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw new InvalidOperationException("This is a read-only collection.");
        }

        public IEnumerator<T> GetEnumerator()
        {
            return EnumeratorFunc().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected IEnumerable<T> EnumerateCollections()
        {
            foreach (var collection in Sources)
            {
                foreach (var item in collection)
                {
                    yield return item;
                }
            }
        }
        protected IEnumerable<T> EnumerateObjects()
        {
            foreach (var item in Objects)
            {
                yield return item;
            }

        }
        protected Func<IEnumerable<T>> EnumeratorFunc;
    }
}
