using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
//using System.Data.SqlClient;
using IQMap;

// TODO: Move the Drintl.Data stuff into ORM. 
namespace IQMap.Implementation
{
    public class SqlDataController: IDataController
    {
        public SqlDataController(IDataStorageController storageController) {
            StorageController = storageController;
        }
        protected IDataStorageController StorageController;

        public IDbConnection GetConnection(string connectionString)
        {
            return StorageController.GetConnection(connectionString);
        }

        public bool Save(IDbConnection connection, object obj)
        {
            IDBObjectData dbData = IQ.DBData(obj);

 	        DBClassInfo info = dbData.ClassInfo;

            QueryType queryType = dbData.IsNew() ? QueryType.Insert : QueryType.Update;
            ISqlQuery query = new SqlQuery(queryType);
            query.AddFieldMap(info.FieldNameMap);
            query.From = dbData.TableName;

            bool isDirty=false;
            bool isNew = dbData.IsNew();
            string pk = dbData.ClassInfo.PrimaryKey.Name;
            
            foreach (var item in dbData.ClassInfo.FieldInfo)
            {
                string name = item.Name;
                if (!item.IsPrimaryKey && !item.IsReadOnly && 
                    (isNew || dbData.IsDirty(name)))
                {
                    query.AddUpdateData(name, info[name].GetValue(obj));
                    isDirty = true;
                }
            }
            
            bool success = false;
            IQEventType eventType= IQEventType.Save;
            if (isDirty)
            {

                if (queryType == QueryType.Insert)
                {
                    eventType |= IQEventType.Insert;
                    int newPK = StorageController.RunQueryScalar(connection,query.GetQuery());
                    if (newPK<=0)
                    {
                        throw new Exception("The record could not be inserted.");
                    }
                    dbData.ClassInfo[pk].SetValue(obj, newPK);
                    success = true;
                }
                else
                {
                    eventType |= IQEventType.Update;
                    query.AddWhere(dbData.ClassInfo.PrimaryKey.Name, dbData.ClassInfo.PrimaryKey.GetValue(obj));
                    success = StorageController.RunQueryScalar(connection, query.GetQuery()) > 0;
                }
            } else {
                success=false;
            }

            var eventHandler = dbData.IQEventHandlerFunc;

            if (eventHandler != null)
            {
                eventHandler(eventType, dbData);
            }

            if (success) {
                dbData.Clean();
            }
            return success;
        }


        public T LoadPK<T>(IDbConnection connection, IConvertible primaryKeyValue) where T : new()
        {
            T obj = new T();
            string pkName=IQ.CreateDBData(obj).ClassInfo.PrimaryKey.Name;

            string error = LoadSingleInto<T>(connection,obj, pkName+"="+"@"+pkName,"@"+pkName,primaryKeyValue);
           
            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }
            return obj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public T Load<T>(IDbConnection connection, string query, params object[] parameters)
        {
            T obj = Utils.GetInstanceOf<T>();
            
            string error;

            IQuery q = new SqlQueryRaw(query, parameters);
            if (q.QueryType != QueryType.Invalid) {
                error=LoadSingleInto<T>(connection,obj,q);

            } else {
                // assume it is a where
                error=LoadSingleInto<T>(connection,obj, query, parameters);
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }
            return obj;
        }

        public int DeletePK<T>(IDbConnection connection, IConvertible primaryKeyValue) where T : new()
        {
            IDBClassInfo info = IQ.GetClassInfo<T>();
            string pkName = info.PrimaryKey.Name;
            string query = "delete from " + info.TableName + " where " + pkName + "=" + "@" + pkName;

            IQuery q = new SqlQueryRaw(query, "@" + pkName, primaryKeyValue);
            return Delete<T>(connection,q);
        }

        public int Delete<T>(IDbConnection connection, string query, params object[] parameters) 
        {
            var clsInfo = IQ.GetClassInfo<T>();

            IQuery q = new SqlQueryRaw(query, parameters);
            
            if (q.QueryType != QueryType.Invalid)
            {
                q.From = clsInfo.TableName;
                return Delete<T>(connection,q);
            }
            else
            {
                // assume it is a where
                var deleteQuery = new SqlQuery(QueryType.Delete);
                deleteQuery.From = clsInfo.TableName;
                deleteQuery.AddWhere(query);
                foreach (var parm in q.Parameters) {
                    deleteQuery.AddParameter(parm);
                }

                return Delete<T>(connection,deleteQuery);
            
            }
        }


        private int Delete<T>(IDbConnection connection, IQuery query)
        {
             return StorageController.RunQueryScalar(connection,query.GetQuery(),query.Parameters);
        }

        public bool TryLoad<T>(IDbConnection connection, string query, out T obj, params object[] parameters)
        {

            T target = Utils.GetInstanceOf<T>();

            string error = LoadSingleInto<T>(connection,target,query, parameters);
            if (!string.IsNullOrEmpty(error))
            {
                obj = default(T);
                return false;
            }
            else
            {
                obj = target;
                return true;
            }
        }

        public bool TryLoadPK<T>(IDbConnection connection, IConvertible primaryKeyValue, out T obj) where T : new()
        {
            T target = new T();
            string error = LoadSingleInto<T>(connection,target, IQ.CreateDBData(target).ClassInfo.PrimaryKey.Name, primaryKeyValue);
            if (!string.IsNullOrEmpty(error))
            {
                obj = default(T);
                return false;
            }
            else
            {
                obj = target;
                return true;
            }
        }

        public IEnumerable<T> LoadMultiple<T>(IDbConnection connection, string query, bool buffered, params object[] parameters) 
        {
            return connection.Query(query, parameters).MapAll<T>(buffered);
        }
        //protected IEnumerable<T> LoadMultiple<T>(IDbConnection connection, IQuery query, bool buffered)
        //{
        //    return connection.
        //    using (IDataReader reader = StorageController.RunQuery(connection, query.GetQuery(), query.Parameters))
        //    {
        //        while (reader.Read())
        //        {

        //            T target = reader.Map<T>();
        //            yield return target;
        //        }
        //    }
        //}

        public IDataReader Query(IDbConnection connection, string query, params object[] parameters)
        {
            SqlQueryRaw q = new SqlQueryRaw(query, parameters);
            return StorageController.RunQuery(connection,q.GetQuery(),q.Parameters);
        }

        public int QueryScalar(IDbConnection connection, string query, params object[] parameters)
        {
            SqlQueryRaw q = new SqlQueryRaw(query, parameters);
            return StorageController.RunQueryScalar(connection,q.GetQuery(),q.Parameters);
        }


        #region private methods

        /// <summary>
        /// returns nu
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>

        protected string LoadSingleInto<T>(IDbConnection connection, T target, string query, params object[] parameters)
        {
            IDBObjectData dbData = IQ.DBData(target);
            DBClassInfo info = dbData.ClassInfo;
            IQuery temp = new SqlQueryRaw(query, parameters);
            IQuery selectQuery;

            if (temp.QueryType == QueryType.Invalid)
            {
                // assume that query is a where clause
                
                ISqlQuery newQuery = new SqlQuery(QueryType.Select);
                newQuery.AddFieldMap(info.FieldNameMap);
                newQuery.Select = String.Join(",", info.SqlNames);
                newQuery.From = dbData.TableName;
                newQuery.AddWhere(query);
                foreach (var parm in temp.Parameters)
                {
                    newQuery.AddParameter(parm);
                }
                selectQuery = newQuery;
            }
            else
            {
                selectQuery = temp;
            }



            return LoadSingleInto<T>(connection,target, selectQuery);
        }


        protected string LoadSingleInto<T>(IDbConnection connection, T target, IQuery query) 
        {
            if (query.QueryType != QueryType.Select)
            {
                throw new Exception("You can only use a SELECT query to load data into an object.");
            }

            string error = "No match found";
            using (IDataReader reader = StorageController.RunQuery(connection,query.GetQuery(),query.Parameters))
            {
                bool looped = false;
                while (reader.Read())
                {
                    if (looped)
                    {
                        error = "Multuple records found for primary key select.";
                        break;
                    }
                    reader.Map(target);
                    looped = true;
                }
                error = "";
            }

            return error;
        }






        //private static HashSet<string> supportedTypes = new HashSet<string>(new string[] {
        //        "bit","bigint","int","smallint","float","decimal","money","smallmoney","date",
        //        "datetime","datetime2","datetimeoffset","varchar","nvarchar","text","ntext",
        //        "real","numeric","smalldatetime","time","timestamp","tinyint"
        //    });
        //private bool IsSupportedSqlType(string sqlType)
        //{
        //    return supportedTypes.Contains(sqlType.ToLower());
        //}
        //private bool IsNullableSqlType(object sqlType)
        //{
        //    return true;
        //}

       

        #endregion


    }
}