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
        #region constructors

        /// <summary>
        /// Create using the default MSSQL Data Storage Controller
        /// </summary>
        public SqlDataController()
        {
            StorageController = new MSSQLDataStorageController();
        }
        public SqlDataController(IDataStorageController storageController) {
            StorageController = storageController;
        }
        #endregion

        #region private properties

        protected IDataStorageController StorageController;

        #endregion

        #region public methods

        public IEnumerable<T> RunStoredProcedure<T>(string spName, params object[] parameters)
        {
            ParameterParser pp = new ParameterParser(spName, IQSqlDirectiveType.StoredProcedure,parameters);

            IDataReader reader = StorageController.RunStoredProcedure(pp.Connection, spName,pp.Parameters,-1,-1,pp.Transaction,pp.CommandBehavior);
            return reader.MapAll<T>(pp.Buffered);
        }
        public IDbConnection GetConnection(string connectionString)
        {
            return StorageController.GetConnection(connectionString);
        }
        /// <summary>
        /// options may include: an IDbConnection, an IDbTransaction, CommandBehavior. Save queries should not 
        /// include any other parameters
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool Save(object obj, params object[] options)
        {
            ParameterParser pp = new ParameterParser("",options);
            if (pp.Parameters.Count > 0)
            {
                throw new IQException("The only allowed parameters for a Save are IDbConnection, IDbTransaction, and CommandBehavior");
            }

            IDBObjectData dbData = IQ.DBData(obj);
 	        DBClassInfo info = dbData.ClassInfo;
            
            IQEventType eventType = IQEventType.Save;
            var eventHandler = dbData.IQEventHandlerFunc;
            if (eventHandler != null)
            {
                eventHandler(eventType | IQEventType.Before, dbData);
            }


            QueryType queryType = dbData.IsNew() ? QueryType.Insert : QueryType.Update;

            ISqlQuery query = IQ.CreateQuery(queryType);
            query.AddFieldMap(info.FieldNameMap);
            query.TableName = dbData.TableName;

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



            if (isDirty)
            {

                if (queryType == QueryType.Insert)
                {
                    eventType |= IQEventType.Insert;

                    int newPK = StorageController.RunQueryInsert(pp.Connection,query.GetQuery(),query.Parameters,
                        transaction: pp.Transaction);
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

                    query.AddWhereParam(dbData.ClassInfo.PrimaryKey.Name, dbData.ClassInfo.PrimaryKey.GetValue(obj));
                    success = StorageController.RunQueryScalar(pp.Connection, query.GetQuery(),query.Parameters, 
                        transaction: pp.Transaction) > 0;
                }
            } else {
                success=false;
            }


            if (eventHandler != null)
            {
                eventHandler(eventType | IQEventType.After, dbData);
            }

            if (success) {
                dbData.Clean();
            }

            if (pp.CommandBehavior == CommandBehavior.CloseConnection)
            {
                pp.Connection.Close();
            }
            return success;
        }
       


        public T Single<T>(object query, params object[] parameters)
        {
            T target;
            if (!TryGetSingle<T>(query,out target,  false, parameters))
            {
                throw new IQException("No record was record was returned by the Single<T> query");
            }
            return target;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public T FirstOrDefault<T>(object query, params object[] parameters)
        {
            T target;
            TryGetSingle<T>(query, out target, true, parameters);
            return target;
        }
        public T First<T>(object query, params object[] parameters)
        {
            T target;
            if (!TryGetSingle<T>(query,out target, true, parameters))
            {
                throw new IQException("No record returned by the First<T> query");
            }
            return target;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public T SingleOrDefault<T>(object query, params object[] parameters)
        {
            T target;
            TryGetSingle<T>(query,out target, false, parameters);
            return target;
        }

        public int Delete<T>(object query, params object[] parameters) 
        {

            var wrapper = new SqlQueryParser<T>(QueryType.Delete, query, parameters);
            return wrapper.RunQueryScalar(StorageController);

        }

        public bool TrySingle<T>(object query, out T target, params object[] parameters)
        {
            return TryGetSingle<T>(query,out target, false, parameters);
        }
        public bool TrySingle<T>(object query, T target, params object[] parameters)
        {
            return TryGetSingle<T>(query,target,  false, parameters);
        }
        public IEnumerable<T> Select<T>(object query,params object[] parameters) 
        {
            var wrapper = new SqlQueryParser<T>( QueryType.Select, query, parameters);
            return wrapper.RunQuery(StorageController).MapAll<T>(wrapper.Buffered);
        }

        public int Count<T>(object query,params object[] parameters) 
        {
            var wrapper = new SqlQueryParser<T>(QueryType.Select, query, parameters);
            return StorageController.Count(wrapper.Connection, wrapper.Query.GetQuery(), wrapper.Query.Parameters);
        }

        public IDataReader Query(string query, params object[] parameters)
        {
            var wrapper = new SqlQueryParser<object>(0, query, parameters);
            return wrapper.RunQuery(StorageController);
        }

        public int QueryScalar(string query, params object[] parameters)
        {
            var wrapper = new SqlQueryParser<object>( 0, query, parameters);
            return wrapper.RunQueryScalar(StorageController);
        }

        #endregion

        #region private methods

        protected bool TryGetSingle<T>(
            object query, 
            out T target, 
            bool ignoreExtraRows, 
            params object[] parameters)
        {
            var wrapper = new SqlQueryParser<T>(QueryType.Select, query, parameters);
            wrapper.ExpectSelectQuery();

            T output=default(T);
            bool looped = false;
            using (IDataReader reader = wrapper.RunQuery(StorageController))
            {
                while (reader.Read())
                {
                    if (looped)
                    {
                        throw new IQException("More than one record was returned by the Single<T> query");
                    }
                    output = reader.Map<T>();
                    looped = true;
                    if (ignoreExtraRows)
                    {
                        break;
                    }
                }
            }
            if (!looped)
            {
                target = (T)Utils.DefaultValue(typeof(T));
                return false;
            } else {
                target = output;
                return true;
            }
        }
        /// <summary>
        /// Map to an existing instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="ignoreExtraRows"></param>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected bool TryGetSingle<T>(
          object query,
          T target,
          bool ignoreExtraRows,
          params object[] parameters)
        {
            var wrapper = new SqlQueryParser<T>(QueryType.Select, query, parameters);
            wrapper.ExpectSelectQuery();

            bool looped = false;
            using (IDataReader reader = wrapper.RunQuery(StorageController))
            {
                while (reader.Read())
                {
                    if (looped)
                    {
                        throw new IQException("More than one record was returned by the Single<T> query");
                    }
                    reader.Map(target);
                    looped = true;
                    if (ignoreExtraRows)
                    {
                        break;
                    }
                }
            }
            return looped;
        }
       
        #endregion


    }
}