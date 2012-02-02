using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;

namespace IQMap.Implementation
{
    /// <summary>
    /// An object defining the binding between an ORM object and the database.
    /// </summary>
    public class DBObjectData: IDBObjectData 
    {
        /// <summary>
        /// fieldList should be a CSV of the properties to include. (PK) after a field name identifies it as the primary key. 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="tableName"></param>
        /// <param name="fieldList"></param>

        public DBObjectData(object owner, string tableName="", string mappedFields="")
        {
            OwnerRef = new WeakReference(owner);
            ClassInfo = GetClassInfo(owner.GetType(), tableName,mappedFields);
            TableName = ClassInfo.TableName;
            if (ClassInfo.IQEventHandlerMethod != null)
            {
                IQEventHandlerFunc = (Action<IQEventType, IDBObjectData>)Delegate
                                    .CreateDelegate(typeof(Action<IQEventType, IDBObjectData>), owner, ClassInfo.IQEventHandlerMethod);
            }
        }

        private static ConcurrentDictionary<Type, DBClassInfo> ClassInfoCache
            = new ConcurrentDictionary<Type, DBClassInfo>();

        public static DBClassInfo GetClassInfo(Type type, string tableName="",string mappedFields="")
        {
            DBClassInfo classInfo;

            if (!ClassInfoCache.TryGetValue(type, out classInfo))
            {
                classInfo = new DBClassInfo();
                classInfo.MapClass(type, tableName, mappedFields);
                ClassInfoCache[type] = classInfo;
            }
            return classInfo;

        }
        #region private properties;
        protected WeakReference OwnerRef
        {
            get;
            set;
        }
        private List<object> _initialValues;
        protected List<object> initialValues
        {
            get
            {
                if (_initialValues==null)
                {
                    _initialValues = new List<object>();
                    Clean();
                }
                return _initialValues;
            }
        }
        

        #endregion

        #region public properties

        public IConvertible PrimaryKeyValue {
            get
            {
                return (IConvertible)ClassInfo.PrimaryKey.GetValue(Owner);

            }
        }

        public bool Orphaned
        {
            get
            {
                return !OwnerRef.IsAlive;
            }
        }
        public object Owner
        {
            get
            {
                return (object)OwnerRef.Target;
            }
        }
        public string TableName { get; set; }
        public  Action<IQEventType, IDBObjectData> IQEventHandlerFunc { get;  set; }

        public IDataStorageController SQLDataController { get; set; }
        /// <summary>
        /// When true, this has been configured an bound to the data of its owner. When false, it has no info on
        /// the original state of its owner's data
        /// </summary>
        public bool Initialized
        {
            get
            {
                return _initialValues != null;
            }
        }

        public DBClassInfo ClassInfo
        {
            get;
            protected set;
        }

        #endregion

        #region public methods
        public void SetPrimaryKey(IConvertible value)
        {
            ClassInfo.PrimaryKey.SetValue(Owner, value);

        }
        public void Clean()
        {
            initialValues.Clear();
            foreach (var fieldName in ClassInfo.FieldNames)
            {
                initialValues.Add(ClassInfo[fieldName].GetValue(OwnerRef.Target));
            }
        }
        public bool IsNew()
        {
            return AreEqual(ClassInfo.PrimaryKeyDefaultValue, ClassInfo.PrimaryKey.GetValue(OwnerRef.Target));
        }

        public bool IsDirty()
        {
            for (int i = 0; i < initialValues.Count; i++)
            {
                if (IsDirty(i))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsDirty(string fieldName)
        {
            int index = ClassInfo.FieldIndex(fieldName);
            return IsDirty(index);
        }

        protected bool IsDirty(int index)
        {
            return !AreEqual(initialValues[index], ClassInfo[index].GetValue(OwnerRef.Target));
        }
        protected bool AreEqual(object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null)
            {
                return true;
            }
            else if (obj1 == null || obj2 == null)
            {
                return false;
            }
            else
            {
                return obj1.Equals(obj2);
            }

        }
        public IEnumerable<string> DirtyFieldNames
        {
            get
            {
                for (int i = 0; i < initialValues.Count; i++)
                {
                    if (IsDirty(i))
                    {
                        yield return ClassInfo.FieldNames[i];
                    }
                }
            }
        }
       
        #endregion


        
    }
}