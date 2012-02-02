using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMap;

namespace IQMapTest
{
    /// <summary>
    /// This only works for the TestObject right now
    /// </summary>
    public class oldTestDataStorageController: IDataStorageController
    {
        public Type MockObjectType { get; set; }
        public IEnumerable<IDataParameter> Parameters { get; protected set; }
        public string Query { get; protected set; }
        protected IDataReader MockDataReader { get; set; }

        private void SetData(string query, IEnumerable<IDataParameter> parms, int rows=1)
        {
            Query = query;
            Parameters = parms;
            MockDataReader = IQMapTest.Mocks.TestDataReader.GenerateFakeDataReader(MockObjectType,rows);
        }




     
       
        public IDataReader RunQuery(IDbConnection connection, string query, IEnumerable<IDataParameter> parameters = null, int firstRow = -1, int lastRow = -1, IDbTransaction transaction = null)
        {
            SetData(query, parameters);
            return MockDataReader;
        }

        public IDataReader RunQuery(IDbConnection connection, string query, out int rows, IEnumerable<IDataParameter> parameters = null, int firstRow = -1, int lastRow = -1, IDbTransaction transaction = null)
        {
            SetData(query, parameters);
            rows = 1;
            return MockDataReader; 
        }

        public int Count(IDbConnection connection, string query, IEnumerable<IDataParameter> parameters = null)
        {
            SetData(query, parameters);
            return 1;
        }

        public int RunQueryScalar(IDbConnection connection, string query, IEnumerable<IDataParameter> parameters = null, IDbTransaction transaction = null)
        {
            SetData(query, parameters);
            return 1;
        }

        public int RunQueryInsert(IDbConnection connection, string query, IEnumerable<IDataParameter> parameters = null, IDbTransaction transaction = null)
        {
            SetData(query, parameters);
            return 1;
        }

        public IDbConnection GetConnection(string connectionString)
        {
            return null;
        }
    }
}
