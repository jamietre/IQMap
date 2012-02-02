using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IQMap
{
    public interface IDBObjectData
    {

        object Owner {get;}
        string TableName { get; set; }
        Action<IQEventType, IDBObjectData> IQEventHandlerFunc { get; set; }
        IDataStorageController SQLDataController { get; set; }

        /// <summary>
        /// When true, this has been configured an bound to the data of its owner. When false, it has no info on
        /// the original state of its owner's data
        /// </summary>
        bool Initialized {get;}
        /// <summary>
        /// Indicates that the owner object has been destroyed
        /// </summary>
        /// <returns></returns>
        bool Orphaned {get;}
        Implementation.DBClassInfo ClassInfo {get;}

        void Clean();
        bool IsDirty();
        bool IsDirty(string fieldName);
        bool IsNew();

        IEnumerable<string> DirtyFieldNames {get;}

        IConvertible PrimaryKeyValue { get; }
        void SetPrimaryKey(IConvertible value);

    }
}