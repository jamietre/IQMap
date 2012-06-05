using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace IQMap.Impl
{
    [Flags]
    public enum OptionTypes
    {
        DbOption=1,
        QueryOption=2
    }
    /// <summary>
    /// Parse a list of objects into things that are database or query options, and other stull
    /// (likely query parms)
    /// </summary>
    public class OptionParser: IOptionParser 
    {
        public OptionParser(IEnumerable<object> options) {
            Parse(options);

        
        }
        protected object[] _NonOptionParameters;
        protected object[] _OptionParameters;

        #region public properties

        public string ObjectListToString(IEnumerable<object> list) {
            string result = "";
            foreach (var item in list)
            {
                result = result + (result == "" ? "" : ", ") + item.ToString();
            }
            return result;
        }
        public object[] NonOptionParametersOrNull
        {
            get
            {
                return NonOptionParameters.Length == 0 ?
                    null :
                    NonOptionParameters;
            }
        }
        public object[] NonOptionParameters
        {
            get
            {
                return _NonOptionParameters;
            }
            protected set
            {
                _NonOptionParameters = value;
            }
        }
        public object[] OptionParameters()
        {
            return OptionParameters(OptionTypes.QueryOption | OptionTypes.DbOption);
        }
        public object[] OptionParameters(OptionTypes types)
        {
                List<object> parms = new List<object>();
                if (types.HasFlag(OptionTypes.DbOption)) {
                    if (Connection != null)
                    {
                        parms.Add(Connection);
                    }
                    if (Transaction != null)
                    {
                        parms.Add(Transaction);
                    }
                    if (DataStorageController != null)
                    {
                        parms.Add(DataStorageController);
                    }
                    if (CommandOptions !=0)
                    {
                        parms.Add(CommandOptions);
                    }
                    if (Buffering != 0)
                    {
                        parms.Add(Buffering);
                    }
                    if (Reconnect != 0)
                    {
                        parms.Add(Reconnect);
                    }
                }
                if (types.HasFlag(OptionTypes.QueryOption))
                {
 
                    if (QueryOptions != null)
                    {
                        parms.Add(QueryOptions);
                    }
                }

                return parms.ToArray();
        }
        
        public QueryOptions QueryOptions { get; set; }

        public IDbConnection Connection { get; set; }
        public IDbConnection ConnectionOrDefault
        {
            get
            {
                return Connection ?? IQ.Config.GetConnection();
            }
        }
        public DbBuffering Buffering { get; set; }
        public DbBuffering BufferingOrDefault
        {
            get
            {
                return Buffering==0 ? DbBuffering.Buffered : Buffering;
            }
        }
        public DbReconnect Reconnect { get; set; }
        public DbReconnect ReconnectOrDefault
        {
            get
            {
                return Reconnect == 0 ? DbReconnect.AlwaysReconnect : Reconnect;
            }
        }
        public IDataStorageController DataStorageController { get; set; }
        public IDataStorageController DataStorageControllerOrDefault
        {
            get
            {
                return DataStorageController ?? IQ.Config.DataStorageController;
            }
        }
        public DbCommandOptions CommandOptions { get; set; }
        public DbCommandOptions CommandOptionsOrDefault
        {
            get
            {
                return CommandOptions == 0 ? IQ.Config.CommandOptions : CommandOptions;
            }
        }
        public IDbTransaction Transaction { get; set; }
        public string TableName
        {
            get
            {
                return QueryOptions.TableName;
            }
            set
            {
                QueryOptions.TableName = value;
            }

        }
        public string PrimaryKey
        {
            get
            {
                return QueryOptions.PrimaryKey;
            }
            set
            {
                QueryOptions.PrimaryKey = value;
            }

        }
        
        #endregion

        #region public methods
        public void Parse(IEnumerable<object> options)
        {
            List<object> nonOptions = new List<object>();
            if (options != null)
            {
                foreach (object item in UnwindOptionList(options))
                {
                    if (item is IDbConnection)
                    {
                        Connection = (IDbConnection)item;
                    }
                    else if (item is IDbTransaction)
                    {
                        Transaction = (IDbTransaction)item;
                    }
                    else if (item is DbBuffering)
                    {
                        Buffering = (DbBuffering)item;
                    }
                    else if (item is DbReconnect)
                    {
                        Reconnect = (DbReconnect)item;
                    }
                    else if (item is IDataStorageController)
                    {
                        DataStorageController = (IDataStorageController)item;
                    }
                    else if (item is DbCommandOptions)
                    {
                        CommandOptions = (DbCommandOptions)item;
                    }
                    else if (item is CommandBehavior)
                    {
                        var cb = (CommandBehavior)item;
                        switch (cb)
                        {
                            case CommandBehavior.CloseConnection:
                                CommandOptions = DbCommandOptions.CloseConnection;
                                break;
                            case CommandBehavior.Default:
                                CommandOptions = DbCommandOptions.LeaveConnetion;
                                break;
                        }
                    }
                    else if (item is QueryOptions)
                    {
                        QueryOptions = (QueryOptions)item;
                    }
                    else if (item!=null)
                    {
                        nonOptions.Add(item);
                    }
                }
            }
            NonOptionParameters = nonOptions.ToArray();

        }
        public void MapTo(IDbContextData optionUser)
        {
            if (Connection != null)
            {
                optionUser.Connection = Connection;
            }
            if (Transaction != null)
            {
                optionUser.Transaction = Transaction;
            }
            if (DataStorageController != null)
            {
                optionUser.DataStorageController = DataStorageController;
            }
            if (CommandOptions != 0)
            {
                optionUser.CommandOptions = CommandOptions;
            }
            if (Buffering != 0)
            {
                optionUser.Buffering = Buffering;
            }
            if (Reconnect != 0)
            {
                optionUser.Reconnect = Reconnect;
            }
        }
        public void MapTo(IQueryOptions optionUser)
        {
            if (!String.IsNullOrEmpty(TableName))
            {
                optionUser.TableName = TableName;
            }
            if (!String.IsNullOrEmpty(PrimaryKey))
            {
                optionUser.PrimaryKey = PrimaryKey;
            }
        }

        #endregion

        private static IEnumerable<object> UnwindOptionList(IEnumerable<object> options)
        {
            if (options == null)
            {
                yield break;
            }
            foreach (object item in options)
            {
                if (item != null)
                {
                    if (item is IEnumerable<object>)
                    {
                        foreach (var subItem in UnwindOptionList((IEnumerable<object>)item))
                        {
                            yield return subItem;
                        }
                    }
                    else
                    {
                        yield return item;
                    }
                }
                else
                {
                    yield return item;
                }
            }
        }

       
    }
}
