using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using IQMap;
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;
using IQObjectMapper;

namespace IQMap.Impl
{
    /// <summary>
    /// Override of the ClassInfoBuilder object that implements the derived types for ClassInfo and DelegateInfo
    /// In IQMap, these types carry additional information about each class (e.g. how to create an SQL query).
    /// </summary>
    public class ClassInfoBuilder<T>: IQObjectMapper.Impl.ClassInfoBuilder<T>, 
        IClassInfoConstructor
        where T: ClassInfo, new()
    {
        public ClassInfoBuilder(IMapOptions options, Type type)
            : base(options, type)
        {

        }

        public override IQObjectMapper.IClassInfo MapClass(Type type)
        {
                    
            T cInfo = (T)base.MapClass(type);

            if (Constructor != null)
            {
                CallIQConstructor();
            }
            if (_Query != null)
            {
                cInfo.Query = Query;
            }
            cInfo.Events = Events;
            cInfo.Track = Track;

            cInfo.ConfigurePrimaryKey();


            // Build the Select -- always select the PK first

            if (IsBound && !Query.SelectAll && (Query.Select == "*" || Query.Select == ""))
            {
                string selectSql = Query.PrimaryKey ;
                foreach (var fld in cInfo.Fields)
                {
                    if (!fld.IsPrimaryKey)
                    {
                        selectSql += (selectSql == "" ? "" : ",") + fld.SqlName;
                    }
                }


                Query.Select = selectSql;
            }

            return cInfo;

        }

        /// <summary>
        /// ComposeClass basically copies the ClassInfo object and adds Data. In IQMap we've extended ClassInfo so this
        /// needs to copy reference to everything from the original. This is not a clone, rather ClassInfo is a flyweight
        /// that references the underlying values for all those things.
        /// </summary>
        /// <param name="classInfo"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public override IQObjectMapper.IClassInfo ComposeClass(IQObjectMapper.IClassInfo classInfo, IClassInfoData data)
        {
            T cInfo = (T)base.ComposeClass(classInfo, data);
            T source = (T)classInfo;
            if (source.IsBound)
            {
                cInfo.Query = source.Query;
            }
            cInfo.Events = source.Events;
            cInfo.Track = source.Track;
            cInfo.ConfigurePrimaryKey();
            return cInfo;
        }
        // these properties will be mapped to the output ClassInfo

        
        
        private ISqlQueryMaker _Query;
        private IDictionary<IQEventType, List<MethodInfo>> _Events;
        protected MethodInfo Constructor = null;

        protected bool IsBound
        {

            get {
            return _Query != null;
        }
        }


        #region public methods/properties 

        // IClassInfoConstructor members

        public IDictionary<IQEventType, List<MethodInfo>> Events
        {
            get
            {
                if (_Events == null)
                {
                    _Events = new Dictionary<IQEventType, List<MethodInfo>>();
                }
                return _Events;
            }
            protected set
            {
                _Events = value;

            }
        }

        public bool Track { get; set; }
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
                    _Query = new SqlQueryMaker();
                }
                return _Query;

            }
        }

        public void AddEventHandler(IQEventType eventType, Action<IEventData> handler)
        {

            MethodInfo method = handler.Method;
            AddAventHandlerImpl(eventType, method);

        }

        protected void AddAventHandlerImpl(IQEventType eventType, MethodInfo method)
        {
            List<MethodInfo> list;
            if (!Events.TryGetValue(eventType, out list))
            {
                list = new List<MethodInfo>();

                Events[eventType] = list;
            }
            list.Add(method);
        }


        #endregion

        protected override void GetClassMetadata()
        {

            object[] attributes = (object[])Type.GetCustomAttributes(true);
            foreach (var attr in attributes)
            {
                if (attr is IQClass)
                {
                    IQClass metaData = (IQClass)attr;

                    if (!String.IsNullOrEmpty(metaData.TableName))
                    {
                        Query.TableName = metaData.TableName;
                    }
                    if (!String.IsNullOrEmpty(metaData.PrimaryKey))
                    {
                        Query.PrimaryKey = metaData.PrimaryKey;
                    }
                    ExcludeByDefault = metaData.ExcludeByDefault;

                }

            }
        }


        protected override void MapFromTemporaryFields()
        {
            base.MapFromTemporaryFields();

            foreach (var field in Data.Values.Cast<IDelegateInfo>())
            {
                if (field.IsPrimaryKey)
                {
                    if (!String.IsNullOrEmpty(Query.PrimaryKey))
                    {
                        throw new InvalidOperationException("A different primary key '" + Query.PrimaryKey +
                            "' has already been identified, cannot make '" + field.Name + "' the pk.");
                    }
                    Query.PrimaryKey = field.SqlName ;
                }
            }

            // validate SqlNames & build the Select statement

            HashSet<string> fieldNames = new HashSet<string>(Data.Values.Cast<IDelegateInfo>().Select(item=>item.Name));


            foreach (var fld in Data.Values.Cast<IDelegateInfo>())
            {

                if (fld.SqlName != fld.Name && 
                    fieldNames.Contains(fld.SqlName))
                {
                    throw new InvalidOperationException(String.Format("The SqlName \"{0}\" is also a field name; you can only have on property mapped per SQL field.",
                        fld.SqlName));
                }

            }


        }

        /// <summary>
        /// Get attribute data
        /// </summary>
        /// <param name="member"></param>
        /// <param name="attributes"></param>
        /// <param name="field"></param>
        protected override void GetPropertyMetadata(MemberInfo member, object[] attributes, IQObjectMapper.Impl.DelegateInfo fieldTemp)
        {
            base.GetPropertyMetadata(member, attributes, fieldTemp);

            bool ignore = false;
            string name = member.Name;
            bool alreadyMapped = TempDelegateInfo.ContainsKey(name);
            IDelegateInfo field = (IDelegateInfo)fieldTemp;

            foreach (var attr in attributes)
            {
                if (attr is IQIgnore)
                {
                    if (alreadyMapped)
                    {
                        throw new InvalidOperationException("The field '" + name + "' is marked for ignore, but has been identified in a constructor already.");
                    }
                    ignore = true;
                    break;
                }
                if (attr is IQPrimaryKey)
                {
                    field.IsPrimaryKey = true;
                }
                IQField fldAttr = null;

                if (attr is IQField)
                {
                    fieldTemp.Include = true;
                    fldAttr = (IQField)attr;
                }

                string sqlName = field.SqlName;
                if (fldAttr != null)
                {
                    if (!String.IsNullOrEmpty(fldAttr.SqlName))
                    {
                        sqlName = fldAttr.SqlName;
                    }
                    if (fldAttr.PrimaryKey)
                    {
                        field.IsPrimaryKey = true;
                    }
                    if (fldAttr.IgnoreNull)
                    {
                        field.IgnoreNull = true;
                    }
                    if (fldAttr.ReadOnly)
                    {
                        field.IsSqlReadOnly = true;
                    }
                }
                field.SqlName = sqlName;
            }
            if (ignore)
            {
                field.Include = false;
            }

        }

        protected override void GetMethodMetadata(MethodInfo member, object[] attributes)
        {

            foreach (var attr in attributes)
            {
                if (attr is IQIgnore)
                {
                    return;
                }
            }

            ParameterInfo[] parmInfo = member.GetParameters();
            if (parmInfo.Length == 1 && parmInfo[0].ParameterType == typeof(IClassInfoConstructor))
            {
                if (member.IsStatic
                    || member.IsPublic
                    || member.ReturnType != typeof(void))
                {
                    throw new InvalidCastException("A method with a single IClassInfoConstructor parameter is present, "
                        + "but its signature is not Func<void,IClassInfoConstructor> or it is not a private instance method. "
                        + "If this is not supposed to be a constructor, mark it with IQIgnore,");
                }
                // Allow a derived constructor to override the base class constructor; but we will always take the first one we find 
                // (e.g. fall back to the base class constructor). 

                if (member.DeclaringType == Type || Constructor == null)
                {
                    Constructor = member;
                }
            }

        }
        /// <summary>
        /// Obtain and process data from an IQ constructor 
        /// </summary>
        public void CallIQConstructor()
        {
            var target = Types.GetInstanceOf(Type);

            Constructor.Invoke(target, new object[] { this });

        }


    }
}
