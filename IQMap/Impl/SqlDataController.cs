using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
//using System.Data.SqlClient;
using IQMap;
using IQMap.SqlQuery;
using IQMap.SqlQuery.Impl;
using IQMap.Impl.Support;
using IQObjectMapper;

// TODO: Move the Drintl.Data stuff into ORM. 
namespace IQMap.Impl
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
        protected IDbConnection _Connection;
        //protected bool TransactionOwner;
        #endregion

        #region public methods

        public IDbConnection Connection
        {
            get
            {
                if (_Connection == null )
                {
                    if (Transaction != null) {
                        return Transaction.Connection;
                    } else {
                        _Connection = GetConnection(IQ.Config.DefaultConnectionString);
                    }
                }

                return _Connection;
                
            }
            set
            {
                if (Transaction != null)
                {
                    throw new Exception("You can't assign a connection when there's an active transaction.");
                }
            }
        }
        public IDbTransaction Transaction { get; set; }

        public bool Any(string query, params object[] parameters)
        {
            IQTestOnly target;
            return TryGetSingle<IQTestOnly>(query, out target, true, parameters);
        }

        /// <summary>
        /// Return true of anything matches the criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        //public bool Any<T>(string where, params object[] parameters)
        //{
        //    var wrapper = new SqlQueryParser<T>(QueryType.Where, where, Transaction, parameters);
        //    wrapper.ExpectSelectQuery();
        //    //wrapper.QueryFull.Top = 1;
        //    wrapper.QueryFull.Select = "1";
        //    int target;
        //    return TryGetSingle<int>(wrapper,false,out target);

        //}
        public IEnumerable<T> Where<T>(string where, params object[] parameters)
        {
            var wrapper = new SqlQueryParser<T>(QueryType.Where, where, Transaction, parameters);
            return MapAll<T>(wrapper.RunQuery(StorageController),wrapper.Buffered);
        }
        public IEnumerable<T> Select<T>(object query, params object[] parameters)
        {
            var wrapper = new SqlQueryParser<T>(QueryType.Select, query, Transaction,parameters);
            return MapAll<T>(wrapper.RunQuery(StorageController),wrapper.Buffered);
        }
        //public IEnumerable<T> RunStoredProcedure<T>(string spName, params object[] parameters)
        //{
        //    ParameterParser pp = new ParameterParser(spName, IQSqlDirectiveType.StoredProcedure,parameters,Transaction);

        //    IDataReader reader = StorageController.RunStoredProcedure(pp.Connection, spName,pp.Parameters,-1,-1,pp.Transaction,pp.CommandBehavior);
        //    return reader.MapAll<T>(pp.Buffered ?? true);
        //}
        public IDbConnection GetConnection(string connectionString)
        {
            return StorageController.GetConnection(connectionString);
        }
        //public bool Save<T>(string table, string where, params object[] options)
        //{
        //    throw new NotImplementedException();
        //}

       


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
            if (TryGetSingle<T>(query, out target, false, parameters))
            {
                return target;
            }
            else
            {
                return default(T);
            }
        }

        public T SingleOrNew<T>(object query, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        public int Delete<T>(object query, params object[] parameters) 
        {

            var wrapper = new SqlQueryParser<T>(QueryType.Delete, query, Transaction, parameters);
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


        public int Count<T>(object query,params object[] parameters) 
        {
            var wrapper = new SqlQueryParser<T>(QueryType.Select, query, Transaction, parameters);
            return StorageController.Count(wrapper.Connection,wrapper.Query);
        }

        public IDataReader Query(string query, params object[] parameters)
        {
            var wrapper = new SqlQueryParser<object>(0, query, Transaction, parameters);
            return wrapper.RunQuery(StorageController);
        }

        public int QueryScalar(string query, params object[] parameters)
        {
            var wrapper = new SqlQueryParser<object>(0, query, Transaction, parameters);
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
            var wrapper = new SqlQueryParser<T>(QueryType.Select, query, Transaction, parameters);
            wrapper.ExpectSelectQuery();
            //wrapper.Query.Top = 1;
            return TryGetSingle(wrapper,ignoreExtraRows,out target);
        }

        protected bool TryGetSingle<T>(ISqlQueryParser wrapper, bool ignoreExtraRows, out T target)
        {

            T output=default(T);
            bool looped = false;
            //var classInfo = IQ.GetClassInfo<T>();

            using (IDataReader reader = wrapper.RunQuery(StorageController))
            {
                            
                DataReaderWrapper drWrap = new DataReaderWrapper(reader);
                MapperWrapper<T> mapWrap = new MapperWrapper<T>(drWrap);

                foreach (var item in mapWrap)
                {
                    if (looped)
                    {
                        throw new IQException("More than one record was returned by the Single<T> query");
                    }

                    //classInfo.DoEvent(output, IQEventType.Before | IQEventType.Load, this);

                    if (Types.IsValueTarget<T>())
                    {
                        output = ObjectMapper.MapValue<T>((IConvertible)reader.GetValue(0));
                    }
                    else
                    {
                        var recordWrap = new DataRecordWrapper(reader);
                        output = recordWrap.Map<T>();
                    }
                    //classInfo.DoEvent(output, IQEventType.After | IQEventType.Load, this);

                    looped = true;
                    if (ignoreExtraRows)
                    {
                        break;
                    }
                }
            }
            if (!looped)
            {
                target = (T)Types.DefaultValue(typeof(T));
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
            var wrapper = new SqlQueryParser<T>(QueryType.Select, query, Transaction, parameters);
            wrapper.ExpectSelectQuery();

            bool looped = false;
            
            using (IDataReader reader = wrapper.RunQuery(StorageController))
            {
                DataReaderWrapper drWrap = new DataReaderWrapper(reader);
                MapperWrapper<T> mapWrap = new MapperWrapper<T>(drWrap);
                
                foreach (var item in mapWrap) {

                    if (looped)
                    {
                        throw new IQException("More than one record was returned by the Single<T> query");
                    }
                    target = item;
                    looped = true;
                    if (ignoreExtraRows)
                    {
                        break;
                    }
                }
            }
            return looped;
        }


        protected IEnumerable<T> MapAll<T>(IDataReader reader, bool buffered = true)
        {
            var enumerated = EnumerateReader<T>(reader);
            if (!buffered)
            {
                return enumerated;
            }
            else
            {
                List<T> list = new List<T>();
                foreach (var item in enumerated)
                {
                    list.Add(item);
                }
                reader.Dispose();
                return list;
            }
        }
        protected IEnumerable<T> EnumerateReader<T>(IDataReader reader)
        {
            var wrapper = new DataReaderWrapper(reader);
            foreach (var item in wrapper)
            {
                yield return item.Map<T>();
            }
            reader.Dispose();
        }
        #endregion

        /// <summary>
        /// When called with Dispose, it ensures that any open connection is rolled back
        /// </summary>

        public void Dispose()
        {
            if (Transaction != null)
            {
                RollbackTransaction();
                Connection.Dispose();
                throw new Exception("An active transaction was rolled back because the SqlDataController was disposed. "
                +   "If you don't want this to happen, commit the transaction before disposing.");
            }
            else
            {
                Connection.Dispose();
            }
        }
        public IDataController BeginTransaction()
        {
            if (Transaction != null)
            {
                throw new Exception("This data controller is already in a transaction.");
            }
            Transaction = Connection.BeginTransaction();
            return this;
        }
        public void CommitTransaction()
        {
            if (Transaction != null)
            {
                throw new Exception("There is no active transaction to commit!");
            }
            Transaction.Commit();
            Transaction = null;
        }
        public void RollbackTransaction()
        {
            if (Transaction != null)
            {
                throw new Exception("There is no active transaction to roll back!");
            }
            Transaction.Rollback();
        }
    }
}