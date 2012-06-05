using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Timers;

namespace IQMap.Impl
{
    /// <summary>
    /// Override of IQObjectMapper factory to return our inherited versions of the ClassInfo and
    /// DelegateInfo classes.
    /// </summary>
  
    public class MapperCache: IQObjectMapper.Impl.MapperCache
    {
        #region constructor

        public MapperCache()
            : base()
        {

        }

        #endregion

        #region private properties

        /// <summary>
        /// ObjectMetaData is the record of the initial values of object when tracking changes. This cache keeps
        /// track of this and must be GC'd because there's no way to notify of changes or destruction events for
        /// POCOs. We use a timer to periodically clean up this by checking WeakReferences to the original object;
        /// once the underlying object has been GC'd we can remove it's data here.
        /// </summary>
        private ConcurrentDictionary<object, IObjectData> ObjectValueData =
            new ConcurrentDictionary<object, IObjectData>();

        #endregion

        #region public methods

        public override IQObjectMapper.Impl.IClassInfoBuilder GetClassInfoBuilder()
        {
            return new ClassInfoBuilder<ClassInfo>(IQObjectMapper.ObjectMapper.DefaultOptions, typeof(DelegateInfo<,>));
        }

        public IClassInfo GetClassInfo(Type type, IReflectionOptions options = null)
        {
            return (IClassInfo)base.GetClassInfo(type, options);
        }

        /// <summary>
        /// Returns null if the object is not tracked
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public IObjectData GetTrackedObjectData(object obj)
        {
            var classInfo = (ClassInfo)GetClassInfo(obj.GetType());
            IObjectData data;
            if (classInfo.Track)
            {
                data = GetOrCreateObjectData(obj);
            }
            else
            {
                bool tracked = TryGetObjectData(obj, out data);
            }
            return data;
        }

        /// <summary>
        /// Always returns object data
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public IObjectData GetOrCreateObjectData(object obj)
        {
            
            IObjectData data;
            if (!TryGetObjectData(obj, out data))
            {
                data = CreateObjectData(obj);
            }
            return data;
        }
        
        /// <summary>
        /// Create new
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public IObjectData CreateObjectData(object obj)
        {
            // technically we could allow metadata to be created for something implementing IDictionary<string,object> 
            // but usually this would represent something that's arleady been mapped. The conservative approach
            // is to reject them. This can be evaluated in the future.

            if (obj is IDictionary<string, object> || !IQObjectMapper.Types.IsMappable(obj.GetType()))
            {
                return null;
            }
            IObjectData newObjectData = new DbObjectData(obj);
            ObjectValueData[obj] = newObjectData;
            return newObjectData;
        }

        /// <summary>
        /// Try to get stored value data for an instance
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool TryGetObjectData(object obj, out IObjectData data)
        {
            if (ObjectValueData.TryGetValue(obj, out data))
            {
                if (ReferenceEquals(data.Owner, obj))
                {
                    return true;
                }
            }
            // Not found - must check everything in the DB since GetHashCode isn't guaranteeed to be unique or to stay the same
            // There are probably ways to optimize this, in fact, it may not even be necessary, but it should be pretty
            // inexpensive unless dealing with huge numbers of objects

            foreach (KeyValuePair<object, IObjectData> kvps in ObjectValueData)
            {
                if (ReferenceEquals(kvps.Value.Owner, obj))
                {
                    data = kvps.Value;
                    return true;
                }
                else if (kvps.Value.Orphaned)
                {
                    RemoveFromDict(kvps.Key);
                }
            }
            data = null;
            return false;
        }

        #endregion

        #region Garbage Collection subsystem

        /// <summary>
        /// The frequency, in seconds, with which garbage collection is done
        /// </summary>
        public int GarbageCollectionFrequency
        {
            set
            {

                if (value > 0)
                {
                    GCTimer.Stop();
                    GCTimer.Interval = value * 1000;
                    GCTimer.Elapsed -= new ElapsedEventHandler(GCTimer_Elapsed);
                    GCTimer.Elapsed += new ElapsedEventHandler(GCTimer_Elapsed);

                    GCTimer.Start();
                }
                else
                {
                    GCTimer.Stop();
                }

            }
            get
            {
                return (int)Math.Floor(GCTimer.Interval / 1000);
            }
        }

        private Timer _GCTimer;

        private Timer GCTimer
        {
            get
            {
                if (_GCTimer == null)
                {
                    _GCTimer = new Timer(30000);
                }
                return _GCTimer;
            }
        }

        private void GarbageCollect()
        {
            foreach (KeyValuePair<object, IObjectData> kvps in ObjectValueData)
            {
                if (kvps.Value.Orphaned)
                {
                    RemoveFromDict(kvps.Key);
                }
            }
        }

        private void GCTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            GarbageCollect();
        }

        private void RemoveFromDict(object key)
        {
            IObjectData data;
            ObjectValueData.TryRemove(key, out data);

        }

        #endregion


    }
}
