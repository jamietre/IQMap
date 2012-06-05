using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using IQMap.Impl.Support;
using IQObjectMapper;

namespace IQMap.Impl
{
    /// <summary>
    /// An object defining the binding between an ORM object and the database.
    /// </summary>
    public class DbObjectData: IObjectData 
    {
        /// <summary>
        /// fieldList should be a CSV of the properties to include. (PK) after a field name identifies it as the primary key. 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="tableName"></param>
        /// <param name="fieldList"></param>

        public DbObjectData(object owner)
        {
            OwnerRef = new WeakReference(owner);
            ClassInfo = IQ.MapperCache.GetClassInfo(owner.GetType());

            Clean();
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

        public IEnumerable<string> DirtyFieldNames
        {
            get
            {
                foreach (var fieldName in ClassInfo.FieldNames)
                {
                    if (IsDirty(fieldName))
                    {
                        yield return fieldName;
                    }
                }
            }
        }

        public IConvertible PrimaryKeyValue {
            get
            {
                return (IConvertible)ClassInfo.PrimaryKeyField.GetValue(Owner);

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

        public IClassInfo ClassInfo
        {
            get;
            protected set;
        }

        #endregion

        #region public methods

        public void SetPrimaryKey(IConvertible value)
        {
            ClassInfo.PrimaryKeyField.SetValue(Owner, value);

        }
        public void Clean()
        {
            initialValues.Clear();
            foreach (var fieldName in ClassInfo.FieldNames)
            {
                initialValues.Add(ClassInfo[fieldName].GetValue(OwnerRef.Target));
            }
        }
        public void Reset()
        {
            foreach (var fieldName in ClassInfo.FieldNames)
            {
                var cinfo = ClassInfo[fieldName];
                cinfo.SetValue(OwnerRef.Target, initialValues[cinfo.Index]);
            }
        }
        public bool IsNew()
        {
            return AreEqual(ClassInfo.PrimaryKeyDefaultValue, ClassInfo.PrimaryKeyField.GetValue(OwnerRef.Target));
        }

        public bool IsDirty()
        {
            foreach (var fieldName in ClassInfo.FieldNames)
            {
                if (IsDirty(fieldName))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsDirty(string fieldName)
        {
            var fldInfo = ClassInfo[fieldName];
            return !AreEqual(initialValues[fldInfo.Index], fldInfo.GetValue(OwnerRef.Target));
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

        public object Clone()
        {
            object clone = Types.GetInstanceOf(Owner.GetType());
            var dbData = IQ.MapperCache.CreateObjectData(clone);
            CopyTo(clone);
            dbData.Clean();
            return clone;
        }

        public void CopyTo(object destination)
        {
            if (Owner.GetType() != destination.GetType())
            {
                throw new IQException("The target object type was not '" + Owner.GetType() + "'");
            }

            foreach (var info in ClassInfo.Fields)
            {
                info.SetValue(destination, info.GetValue(Owner));
            }
        }

        #endregion


        
    }
}