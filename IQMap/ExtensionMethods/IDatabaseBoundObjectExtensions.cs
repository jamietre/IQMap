using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using IQMap.Implementation;

namespace IQMap
{
    public static class IDatabaseBoundObjectExtensions
    {

        #region IDatabaseBoundObject extension methods

        public static void Clean(this IDatabaseBoundObject obj)
        {
            IQ.DBData(obj).Clean();
        }
        public static IEnumerable<string> DirtyFieldNames(this IDatabaseBoundObject obj)
        {
            return IQ.DBData(obj).DirtyFieldNames;
        }
        public static bool IsNew(this IDatabaseBoundObject obj)
        {
            return IQ.DBData(obj).IsNew();
        }
        public static bool IsDirty(this IDatabaseBoundObject obj)
        {
            return IQ.DBData(obj).IsDirty();
        }

        public static bool IsDirty(this IDatabaseBoundObject obj, string fieldName)
        {
            return IQ.DBData(obj).IsDirty(fieldName);
        }

        public static IDBObjectData DBData(this IDatabaseBoundObject obj)
        {
            return IQ.DBData(obj);
        }

        public static bool Save(this IDatabaseBoundObject obj)
        {
            return IQ.Save(obj);
        }

        #endregion

        #region private methods


        #endregion

    }
}
