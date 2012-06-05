using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap
{
    public interface IDbContextData
    {
        IDataStorageController DataStorageController { get; set; }
        IDbConnection Connection { get; set; }
        IDbTransaction Transaction { get; set; }
        DbCommandOptions CommandOptions { get; set; }
        DbBuffering Buffering { get; set; }
        DbReconnect Reconnect { get; set; }
    }


}
