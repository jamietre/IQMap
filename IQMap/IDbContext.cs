using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Dynamic;

namespace IQMap
{


    public interface IDbContext : IDbContextData, IDisposable
    {
        
        CommandBehavior CommandBehavior { get; }
        IDbContext BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
        IDbContext Options(params object[] options);

        // Methods that do not alter the query can be bound directly to the context


        IQueryBuilder<T> From<T>(params object[] options) where T : class;
        IQueryBuilder<T> From<T>(string where, params object[] options) where T : class;
        IQueryBuilder<T> From<T>(int pkValue, params object[] options) where T : class;
        IQuery<dynamic> From(string query, params object[] options);

        /// <summary>
        /// Create a QueryBuilder from a runtime type that selects the primary key (if nonzero). The object must have metadata describing it's SQL bindings; e.g. 
        /// be marked with IQClass and have at least TableName and PrimaryKey defined.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        IQueryBuilder<object> FromInstance(object obj, params object[] options);
        IQueryBuilder<object> FromPrimaryKey(object obj, params object[] options);

        IDataReader RunSql(string query, params object[] parameters);

        IEnumerable<T> Query<T>(string where, params object[] parameters);
        IEnumerable<dynamic> Query(string where, params object[] parameters);

        int QueryScalar(string query, params object[] parameters);

        bool Save(object obj, IQueryOptions options=null);
        bool Delete(object obj, IQueryOptions options=null);
        void Load(object obj, string where, params object[] parms);

    }
}
