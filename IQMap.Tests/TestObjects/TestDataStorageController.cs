using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using IQMap;
using IQMap.Impl;
using IQMap.SqlQueryBuilder;

namespace IQMap.Tests
{

    public class TestDataStorageController : MSSQLDataStorageController
    {
        public override int RunQueryInsert(IDbConnection connection,
            ISqlQueryDef query, 
            IDbTransaction transaction = null,
            CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            int newId;
            try
            {
                newId = base.RunQueryInsert(connection, query,transaction, commandBehavior);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                newId = 9999;
            }
            return newId;
        }
        public override int RunQueryScalar(IDbConnection connection,
            ISqlQueryDef query, 
            IDbTransaction transaction = null,
            CommandBehavior commandBehavior = CommandBehavior.Default )
        {
            int value;
            try
            {
                value =base.RunQueryScalar(connection, query, transaction, commandBehavior);
            }
            catch(Exception e) 
            {
                Console.WriteLine(e.Message);
                value = 9999;
            }
            return value;
        }
    }
}
