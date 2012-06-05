using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Data;
using System.Linq;
using System.Web;

using System.Dynamic;
using IQMap.Impl;


namespace IQMap.Impl
{
    public class Config : IOptions, IReflectionOptions
    {
        public Config()
        {
           
            DynamicObjectType = typeof(ExpandoObject);

            // set mapper options

            IncludeFields = false;
            IncludeProperties = true;
            OverwriteExisting = true;
            FailOnMismatchedTypes = true;
            Buffering = DbBuffering.Buffered;
            Reconnect = DbReconnect.AlwaysReconnect;

            CommandOptions = DbCommandOptions.CloseConnection;
            IgnoreCaseParameters = true;

            GarbageCollectionFrequency = 30;
            
        }
       
        //private static IDataController _DataController;
        private static IDataStorageController _DataStorageController;

        

        private static Type _DynamicObjectType;
        private static bool _IgnoreCaseParameters;

        #region public properties


        public bool IgnoreCaseParameters
        {
            get
            {
                return _IgnoreCaseParameters;
            }
            set
            {
                _IgnoreCaseParameters = value;
                _ParmNameComparer = null;
            }
        }


        /// <summary>
        /// The frequency, in seconds, with which garbage collection is done
        /// </summary>
        public int GarbageCollectionFrequency
        {
            set
            {

                IQ.MapperCache.GarbageCollectionFrequency = value;
            }
            get
            {
                return IQ.MapperCache.GarbageCollectionFrequency;
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
        public IDbConnection GetConnection()
        {
            return DataStorageController.GetConnection(DefaultConnectionString);
        }

        public IDataStorageController DataStorageController
        {
            get
            {
                if (_DataStorageController == null)
                {
                    _DataStorageController = new Impl.MSSQLDataStorageController();
                }
                return _DataStorageController;
            }
            set
            {
                _DataStorageController = value;
            }
        }
      
        /// <summary>
        /// When mapping to a dynamic object, what type to create. Must inherit IDynamicDataMetaProvider
        /// </summary>
        public Type DynamicObjectType
        {
            get
            {
                return _DynamicObjectType;
            }
            set
            {
                if (!typeof(IDynamicMetaObjectProvider).IsAssignableFrom(value))
                {
                    throw new InvalidCastException("The DynamicObjectType must implement IDynamicMetaObjectProvider");
                }
                _DynamicObjectType = value;
                IQObjectMapper.ObjectMapper.DefaultOptions.DynamicObjectType = value;

            }
        }

        /// <summary>
        /// When true, automatically-generated parameters will use short names
        /// </summary>
        public bool OptimizeParameterNames
        {
            get
            {
                return SqlQueryBuilder.Impl.ParameterCollection.OptimizeParameterNames;
            }
            set
            {
                SqlQueryBuilder.Impl.ParameterCollection.OptimizeParameterNames = value;
            }
        }
        public DbBuffering Buffering { get; set; }
        public DbReconnect Reconnect {get;set;}
        public DbCommandOptions CommandOptions
        {
            get;
            set;
        }
        #endregion

        #region internal methods

        internal IEqualityComparer<string> ParmNameComparer
        {
            get
            {
                if (_ParmNameComparer == null)
                {
                    _ParmNameComparer = new Support.LocalStringComparer(!IQ.Config.IgnoreCaseParameters);
                }
                return _ParmNameComparer;
            }
        }
        internal IEqualityComparer<string> _ParmNameComparer;

        #endregion
        
        #region private methods



        #endregion

        public bool IncludeMethods { get; set; }
        public bool IncludeFields
        {
            get;
            set;
        }

        public bool IncludeProperties
        {
            get;
            set;
        }

        public bool OverwriteExisting
        {
            get;
            set;
        }

        public bool FailOnMismatchedTypes
        {
            get;
            set;
        }
        public bool IncludePrivate { get; set; }
        public bool CaseSensitive { get; set; }
        public bool DeclaredOnly { get; set; }


    }
}