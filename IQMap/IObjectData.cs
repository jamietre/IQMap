using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IQMap
{
    public interface IObjectData
    {

        object Owner {get;}

        IDataStorageController SQLDataController { get; set; }

        /// <summary>
        /// Indicates that the owner object has been destroyed
        /// </summary>
        /// <returns></returns>
        bool Orphaned {get;}
        IClassInfo ClassInfo {get;}

        /// <summary>
        /// Set the object state to "clean" discarding info about previous state
        /// </summary>
        void Clean();
        /// <summary>
        /// Restore the object to its initial clean state, discarding any changed data
        /// </summary>
        void Reset();
        /// <summary>
        /// Returns true if any fields have changed
        /// </summary>
        /// <returns></returns>
        bool IsDirty();
        bool IsDirty(string fieldName);
        /// <summary>
        /// The object has never been saved to the database (primary key= default value for data type)
        /// </summary>
        /// <returns></returns>
        bool IsNew();
        /// <summary>
        /// Copy all the databound fields on one instance to another. If the target is dirty, will error.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        void CopyTo(object destination);
        /// <summary>
        /// Return a new instance of the bound type with the same data. The new object will not maintin dirty state
        /// info from the source (it will always be clean)
        /// </summary>
        /// <returns></returns>
        object Clone();
        IEnumerable<string> DirtyFieldNames { get; }

        IConvertible PrimaryKeyValue { get; }
        void SetPrimaryKey(IConvertible value);

    }
}