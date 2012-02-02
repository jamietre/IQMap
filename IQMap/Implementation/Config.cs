using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Web;
using System.Timers;
using IQMap.Implementation;

namespace IQMap.Implementation
{
    public class Config
    {
        public Config()
        {
            GCTimer = new Timer(30);
            GCTimer.Elapsed += new ElapsedEventHandler(GCTimer_Elapsed);
            GCTimer.Start();

        }

        private static Timer GCTimer;
        private static IDataController _DataController;

        public static Dictionary<Type, DbType> DbTypeMap;

        #region public properties
        public event EventHandler GCTime;

        /// <summary>
        /// The frequency, in seconds, with which garbage collection is done
        /// </summary>
        public int GarbageCollectionFrequency
        {
            set
            {
                GCTimer.Interval = value * 1000;
            }
            get
            {
                return (int)Math.Floor(GCTimer.Interval / 1000);
            }
        }

        /// <summary>
        /// SQL Server connection string (creates a default DataController, ignored if DataController is already set)
        /// </summary>
        public string DefaultConnectionString
        {
            get;
            set;
        }
        public IDbConnection DefaultConnection
        {
            get
            {
                return DataController.GetConnection(DefaultConnectionString);
            }
        }
        /// <summary>
        /// Default data controller used for new objects. 
        /// </summary>
        public IDataController DataController
        {
            get
            {
                if (_DataController == null)
                {
                    IDataStorageController mssql = new Implementation.MSSQLDataStorageController();
                    _DataController = new SqlDataController(mssql);
                }
                return _DataController;
            }
            set
            {
                _DataController = value;
            }


        }

        #endregion


        #region private methods


        private void GCTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (GCTime != null)
            {
                GCTime(this, new EventArgs());
            }
        }


        #endregion
    }
}