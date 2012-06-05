using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Reflection;
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;
using IQMap.Impl.Support;
using IQObjectMapper;

namespace IQMap.Impl
{
    public class ClassInfo: IQObjectMapper.Impl.ClassInfo, IClassInfo
    {
        #region constructor

        public ClassInfo(): base()
        {

        }

        #endregion

        #region private properties

        private ISqlQueryMaker _Query;
        private IDictionary<string, string> _SqlNamesDict;
        

        #endregion

        #region public properties

        public bool Track { get; set; }

        public int SqlDataSizeEstimate {get; protected set;}

        public IDictionary<IQEventType, List<MethodInfo>> Events {get;set;}
        
        public ISqlQueryMaker Query
        {
            set
            {
                _Query = value;
            }
            get
            {
                if (_Query == null)
                {
                    throw new InvalidOperationException("This class type is not bound.");
                } 
                return _Query;

            }
        }
        public void ConfigurePrimaryKey()
        {

            if (this.IsBound) {
                if (String.IsNullOrEmpty(Query.PrimaryKey))
                {
                    throw new InvalidOperationException(String.Format("There is no primary key associated with the class {0} for a class-bound query.",
                        Types.TypeName(Type)));
                }
                 
                PrimaryKeyField = this[Query.PrimaryKey];
                PrimaryKeyField.IsPrimaryKey = true;
                PrimaryKeyDefaultValue = Types.DefaultValue(PrimaryKeyField.ReturnType);
                
            }
        }
        public IDelegateInfo PrimaryKeyField
        {
            get; protected set;
        }
        
        public new IList<IDelegateInfo> Fields
        {
            get
            {
                return new ReadOnlyCollection<IDelegateInfo>(base.Fields.Cast<IDelegateInfo>().ToList());
            }

        }
        public IDictionary<string,string> SqlNameMap
        {
            get
            {
                if (_SqlNamesDict == null)
                {
                    _SqlNamesDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < Fields.Count; i++)
                    {
                        _SqlNamesDict.Add(Fields[i].SqlName, Fields[i].Name);
                    }
                }
                return _SqlNamesDict;
            }
        }
        //public bool TryGetFieldBySqlName(string sqlName, out IDelegateInfo del) {
        //    int index;
        //    if (SqlNamesDict.TryGetValue(sqlName, out index))
        //    {
        //        del = Fields[index];
        //        return true;
        //    }
        //    else
        //    {
        //        del = null;
        //        return false;
        //    }
        //}
        /// <summary>
        /// When true, the class has been marked as "select all" and query implementors should use "*" or equivalent, instead of selecting fields.
        /// </summary>
        public bool SelectAll { get; set; }

        /// <summary>
        /// The base query object from which new queries using "where" criteria will be constructed.
        /// </summary>

        public bool IsBound
        {
            get
            {
                return _Query != null;
            }
        }
        public ISqlQueryMaker GetQuery()
        {
            return GetQuery(Query.QueryType);
        }
        public ISqlQueryMaker GetQuery(QueryType queryType, IQueryOptions options=null)
        {
            if (!IsBound)
            {
                throw new InvalidOperationException("This type is not bound to a query");
            }
            else
            {
                var clone = Query.Clone(queryType);
                if (options != null)
                {
                    if (!String.IsNullOrEmpty(options.PrimaryKey))
                    {
                        clone.PrimaryKey = options.PrimaryKey;
                    }
                    if (!String.IsNullOrEmpty(options.TableName))
                    {
                        clone.TableName = options.TableName;
                    }
                }
                
                return clone;
            }
        }
        public bool TryGetValue(string fieldName, out IDelegateInfo info)
        {
            IQObjectMapper.IDelegateInfo baseInfo;
            if (base.TryGetValue(fieldName, out baseInfo))
            {
                info = (IDelegateInfo)baseInfo;
                return true;
            }
            else
            {
                info = null;
                return false;
            }

        }

        /// <summary>
        /// The default value for the primary key's datatype.
        /// </summary>
        public object PrimaryKeyDefaultValue
        {
            get; protected set;
        }

        /// <summary>
        /// Call all registered event handlers for an event type. If the "Cancel" property of the argument is set, this
        /// will return false with the expectation that the event is halted.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="eventType"></param>
        public bool DoEvent(object target, IQEventType eventType, IDbContext controller)
        {
            var dbData = IQ.MapperCache.GetTrackedObjectData(target);

            var parm = new ObjectInfo(
                eventType,
                controller,
                dbData,
                this,
                target);
                
            List<MethodInfo> list;
            object[] args = new object[1] { parm };
            // mask out the details

            if (Events.TryGetValue(eventType, out list))
            {
                foreach (var method in list)
                {
                    try
                    {
                        method.Invoke(target, args);
                    }
                    catch (Exception e)
                    {
                        if (e.InnerException != null)
                        {
                            throw e.InnerException;
                        }
                        else
                        {
                            throw e;
                        }
                    }
                }
            }
            return !parm.Cancel;
        }
        public new IDelegateInfo this[string fieldName]
        {
            get
            {
                return (IDelegateInfo)base[fieldName];
            }
        }

        #endregion

        #region public methods



        #endregion       

        #region static methods

        public static bool IsNew(object obj)
        {
            IClassInfo cinfo = IQ.MapperCache.GetClassInfo(obj.GetType());
            if (!cinfo.IsBound)
            {
                throw new InvalidOperationException("The object is not a bound IQClass");
            }
            return cinfo.PrimaryKeyField.GetValue(obj).Equals(cinfo.PrimaryKeyDefaultValue);
        }
        public static bool IsTracked(object obj)
        {
            IClassInfo cinfo = IQ.MapperCache.GetClassInfo(obj.GetType());
            if (cinfo.Track)
            {
                return true;
            }
            else
            {
                IObjectData dbData;
                return IQ.MapperCache.TryGetObjectData(obj, out dbData);
            }
        }



        #endregion
    }


}
