using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Data;
using IQMap.Impl.Support;
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;

namespace IQMap.Impl
{
    public class DbContext: IDbContext, IQueryProvider
    {
        #region constructors

        public DbContext(IDataStorageController dsController,
            IDbConnection connection,
            IDbTransaction transaction = null,
            DbCommandOptions commandOptions = 0,
            DbBuffering  buffering=0,
            DbReconnect reconnect=0)
        {
            Connection = connection;
            DataStorageController = dsController;
            Transaction = transaction;
            CommandOptions = commandOptions== 0 ?
                IQ.Config.CommandOptions :
                commandOptions;

            Buffering = buffering == 0 ?
                IQ.Config.Buffering :
                buffering;
            Reconnect = reconnect == 0 ?
                IQ.Config.Reconnect :
                reconnect;
        }
            
        #endregion

        #region private properties

        protected IDbTransaction _Transaction;
        protected IDbConnection _Connection;
        protected CommandBehavior _CommandBehavior;
        protected IDataStorageController _DataStorageController;


        #endregion

        #region public properties

        
        /// <summary>
        /// Returns the connection without respecting a transaction or reopen rule.
        /// </summary>
        public IDbConnection ConnectionActual
        {
            get
            {
                return _Connection;
            }
        }
        public IDbConnection Connection
        {
            get
            {
                if (_Connection == null)
                {
                    if (Transaction != null)
                    {
                        if (Transaction.Connection == null 
                            || Transaction.Connection.State != ConnectionState.Open )
                        {
                            throw new InvalidOperationException("A transaction with an invalid connection was present.");
                        }

                        return Transaction.Connection;
                    }
                    else
                    {
                        _Connection = DataStorageController.GetConnection(IQ.Config.DefaultConnectionString);

                    }
                }
                if (_Connection.State == ConnectionState.Closed
                    && Reconnect == DbReconnect.AlwaysReconnect)
                {
                    _Connection.Open();
                }
                return _Connection;

            }
            set
            {
                if (Transaction != null)
                {
                    throw new InvalidOperationException("You can't assign a connection when there's an active transaction.");
                }
                _Connection = value;
            }
        }
        public IDbTransaction Transaction
        {
            get
            {
                return _Transaction;
            }
            set
            {
                _Transaction = value;
                if (_Transaction != null)
                {
                    _Connection = _Transaction.Connection;
                }
            }
        }
        public IDataStorageController DataStorageController
        {
            get
            {
                if (_DataStorageController == null)
                {
                    return IQ.Config.DataStorageController;
                }
                else
                {
                    return _DataStorageController;
                }
            }
            set
            {
                _DataStorageController = value;
            }
        }
        public CommandBehavior CommandBehavior
        {
            get
            {
                return Transaction != null ?
                    CommandBehavior.Default :
                    CommandOptions.HasFlag(DbCommandOptions.CloseConnection) ?
                        CommandBehavior.CloseConnection :
                        CommandBehavior.Default;
                
                
            }
            
        }
        public DbCommandOptions CommandOptions {get;set; }
        public DbReconnect Reconnect { get; set; }
        public DbBuffering Buffering { get; set; }
        
        #endregion

        #region public methods
        public IDbContext Options(params object[] options)
        {
            var op = new OptionParser(options);
            if (op.NonOptionParametersOrNull != null)
            {
                throw new ArgumentException("Only IDbContextData options area allowed to be passed to a DbContext.");
            }
            op.MapTo(this);
            return this;
        }
        public IQueryBuilder<T> From<T>(params object[] options) where T: class
        {
            var op = new OptionParser(options);
            IQueryBuilder<T> query = new QueryBuilder<T>(this, op.QueryOptions);
            
            return query;
        }
        public IQueryBuilder<T> From<T>(string where, params object[] options) where T: class
        {
            var op = new OptionParser(options);
            IQueryBuilder<T> query = new QueryBuilder<T>(this, op.QueryOptions);
            return query.Where(where, op.NonOptionParameters);
        }
        public IQueryBuilder<T> From<T>(int pkValue, params object[] options) where T: class
        {
            var op = new OptionParser(options);
            IQueryBuilder<T> query = new QueryBuilder<T>(this, op.QueryOptions);
            return query.Where(pkValue);
        }
        public IQuery<dynamic> From(string query, params object[] parameters) 
        {
            var op = new OptionParser(parameters);
            var raw = new SqlQueryDef(query, parameters);
            return (IQuery<dynamic>)(new Query(this, raw));
        }

        public IQueryBuilder<object> FromPrimaryKey(object obj, params object[] options)
        {
            object[] ignore;
            IQueryBuilder<object> query = FromInstance(obj, out ignore, options);
            int primaryKeyValue = (int)query.ClassInfo.PrimaryKeyField.GetValue(obj);
            return query.Where(primaryKeyValue);
        }
        public IQueryBuilder<object> FromInstance(object obj, params object[] options)
        {
            var op = new OptionParser(options);
            IQueryBuilder<object> query = new QueryBuilder<object>(this, obj, op.QueryOptions);
            return query;
        }
        protected IQueryBuilder<object> FromInstance(object obj, out object[] nonOptionParameters,params object[] options )
        {
            var op = new OptionParser(options);
            IQueryBuilder<object> query = new QueryBuilder<object>(this, obj, op.QueryOptions);
            nonOptionParameters = op.NonOptionParametersOrNull;
            return query;
        }
        protected IClassInfo GetClassInfo(object obj) {
            IClassInfo classInfo = IQ.ClassInfo(obj.GetType());
            if (!classInfo.IsBound) {
                throw new KeyNotFoundException(String.Format("The type '{0}' is not bound.",obj.GetType()));
            }
            return classInfo;
        }

        public void Load(object obj, string where, params object[] parameters)
        {
            object[] finalParms;

            // load events are handled by the enumerator

            FromInstance(obj, out finalParms, parameters)
                .Where(where, finalParms)
                .To(obj)
                .Single();
        }
        
        public bool Delete(object obj, IQueryOptions options=null)
        {
            IQueryBuilder<object> query = FromPrimaryKey(obj, options);
            IClassInfo classInfo = query.ClassInfo;

            if (ClassInfo.IsNew(obj))
            {
                throw new InvalidOperationException("The object in question has a default-valued primary key, you can't delete it.");
            }
            if (!classInfo.DoEvent(obj, IQEventType.BeforeDelete, this))
            {
                return false;
            }

            bool success = query.Delete()>0;
            if (success)
            {
                classInfo.PrimaryKeyField.SetValue(obj, classInfo.PrimaryKeyDefaultValue);
            }

            if (!classInfo.DoEvent(obj, IQEventType.OnDelete, this))
            {
                throw new InvalidOperationException("The operation was cancelled after the database query was executed.");
            }

            return success;
        }
        /// <summary>
        /// options may include: an IDbConnection, an IDbTransaction, CommandBehavior. Save queries should not 
        /// include any other parameters
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool Save(object obj, IQueryOptions options=null)
        {
            // TODO: This method is way too long. Break it down

            IClassInfo classInfo = GetClassInfo(obj);

            if (!classInfo.DoEvent(obj, IQEventType.BeforeSave, this))
            {
                return false;
            }

            // Determine if the object is tracked for changes. If not, we will just save everything.

            IObjectData dbData;
            bool tracked=IQ.MapperCache.TryGetObjectData(obj, out dbData);

            // Determine if the object is a new record based on the primary key value

            bool isNew = ClassInfo.IsNew(obj);
           
            QueryType queryType = isNew ? 
                QueryType.Insert : 
                QueryType.Update;
           
            ISqlQueryMaker query = classInfo.GetQuery(queryType, options);

            bool isDirty = false;
            
            string pk = classInfo.Query.PrimaryKey;

            foreach (var item in classInfo.Fields)
            {
                string name = item.Name;
                if (!item.IsPrimaryKey && !item.IsSqlReadOnly &&
                    (isNew || !tracked || dbData.IsDirty(name)))
                {
                    query.AddUpdateData(classInfo[name].SqlName, classInfo[name].GetValue(obj));
                    isDirty = true;
                }
            }

            bool success = false;

            if (isDirty)
            {
                if (queryType == QueryType.Insert)
                {
                    if (!classInfo.DoEvent(obj, IQEventType.BeforeInsert, this))
                    {
                        return false;
                    }

                    int newPK = DataStorageController.RunQueryInsert(Connection,query,
                        Transaction,CommandBehavior);

                    if (newPK <= 0)
                    {
                        throw new InvalidOperationException("The record could not be inserted.");
                    }
                    
                    classInfo[pk].SetValue(obj, newPK);
                    if (!classInfo.DoEvent(obj, IQEventType.OnInsert, this))
                    {
                        throw new InvalidOperationException("The operation was cancelled after the database query was executed.");
                    }

                    success = true;
                }
                else
                {
                    if (!classInfo.DoEvent(obj, IQEventType.BeforeUpdate, this)) {
                        return false;
                        }

                    query.Where.Add(classInfo.PrimaryKeyField.Name,
                        classInfo.PrimaryKeyField.GetValue(obj));

                    success = DataStorageController.RunQueryScalar(Connection, query,
                        Transaction, CommandBehavior) > 0;

                    if (!classInfo.DoEvent(obj, IQEventType.OnUpdate, this))
                    {

                        throw new InvalidOperationException("The operation was cancelled after the database query was executed.");
                    }
                }
            }
            else
            {
                success = false;
            }

            if (!classInfo.DoEvent(obj, IQEventType.OnSave, this))
            {
                throw new InvalidOperationException("The operation was cancelled after the database query was executed.");
            }

            if (success && tracked)
            {
                dbData.Clean();
            }
            
            return success;
        }

        public virtual IDbContext BeginTransaction()
        {
            if (Transaction != null)
            {
                throw new InvalidOperationException("This data controller is already in a transaction.");
            }
            Transaction = Connection.BeginTransaction();
            return this;
        }
        public virtual void CommitTransaction()
        {
            if (Transaction == null)
            {
                throw new InvalidOperationException("There is no active transaction to commit!");
            }
            
            Transaction.Commit();
            if (CommandOptions == DbCommandOptions.CloseConnection)
            {
                Connection.Dispose();
            }
            Transaction = null;
        }
        public virtual void RollbackTransaction()
        {
            if (Transaction == null)
            {
                throw new InvalidOperationException("There is no active transaction to roll back!");
            }
            Transaction.Rollback();
        }

        public virtual void Dispose()
        {
            if (Transaction != null)
            {
                RollbackTransaction();
                throw new InvalidOperationException("The query controller was disposed while a transaction was active; it was rolled back.");
            }
            if (ConnectionActual != null && ConnectionActual.State == ConnectionState.Open)
            {
                ConnectionActual.Dispose();
            }
        }


        public IDataReader RunSql(string query, params object[] parameters)
        {
            var raw = new SqlQueryDef(query, parameters);

            return DataStorageController.RunQuery(Connection,
                raw,
                Transaction,
                CommandBehavior);
        }

        public IEnumerable<T> Query<T>(string where, params object[] parameters)
        {
            return RunSql(where, parameters).MapAll<T>();
        }

        public IEnumerable<dynamic> Query(string where, params object[] parameters)
        {

            return RunSql(where, parameters).MapAll<IDynamicMetaObjectProvider>();
        }


        public int QueryScalar(string query, params object[] parameters)
        {
            var raw = new SqlQueryDef(query, parameters);

            return DataStorageController.RunQueryScalar(Connection,
                raw,
                Transaction,
                CommandBehavior);
        }
        #endregion

        internal void MapParameterOptions(IEnumerable<object> options)
        {
            var op = new OptionParser(options);
            if (op.NonOptionParameters.Length > 0)
            {
                throw new ArgumentException("There were unexpected options passed with your query: " + op.ObjectListToString(op.NonOptionParameters));
            }
            op.MapTo(this);

        }




        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            throw new NotImplementedException();
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}
