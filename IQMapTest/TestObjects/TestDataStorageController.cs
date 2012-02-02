using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMap;
using IQMap.Implementation;

namespace IQMapTest
{

    public class TestDataStorageController : MSSQLDataStorageController
    {
        public override int RunQueryInsert(IDbConnection connection, string query, IEnumerable<IDataParameter> queryParameters = null, IDbTransaction transaction = null)
        {
            try
            {
                base.RunQueryInsert(connection, query, queryParameters, transaction);
            }
            catch
            {

            }
            return 1;
        }
        public override int RunQueryScalar(IDbConnection connection, string query, IEnumerable<IDataParameter> parameters = null, IDbTransaction transaction = null)
        {
            try
            {
                base.RunQueryScalar(connection, query, parameters, transaction);
            }
            catch
            {

            }
            return 1;
        }
    }
}
