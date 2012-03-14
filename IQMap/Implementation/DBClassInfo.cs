using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Reflection;

namespace IQMap.Implementation
{
    /// <summary>
    /// Represents the structure of a database-bound class.
    /// </summary>
    [Serializable]
    public class DBClassInfo: IDBClassInfo
    {
        #region constructor

        public DBClassInfo()
        {

        }
        #endregion

        #region private properties

        private ISqlQuery _Query;
        private Type ClassType;
        private Action<IQClassData> iqConstructor=null;
        private Lazy<Dictionary<string, int>> _fieldIndexMap = new Lazy<Dictionary<string, int>>();
        protected Dictionary<string, int> fieldIndexMap
        {
            get
            {
                return _fieldIndexMap.Value;
            }
        }
        private Dictionary<string,Field> temporaryFieldInfo;

        private List<IDBFieldInfo> fieldInfo = new List<IDBFieldInfo>();
        //private List<string> fieldNames = new List<string>();
        //private List<string> sqlFieldNames = new List<string>();
        private IDBFieldInfo _PrimaryKey;

        /// <summary>
        /// CSV of field data from attribute or passed directly
        /// </summary>
        private string fieldMap;

        #endregion

        #region public properties
        
        public IDBFieldInfo PrimaryKey
        {
            get
            {
                return _PrimaryKey;
            }
            set
            {
                _PrimaryKey = value;
                PrimaryKeyDefaultValue = Utils.DefaultValue(value.Type);
            }
        }

        public IList<IDBFieldInfo> FieldInfo
        {
            get
            {
                return new ReadOnlyCollection<IDBFieldInfo>(fieldInfo);
            }

        }

        public IList<string> FieldNames
        {
            get
            {
                return FieldInfo.Select(item => item.Name).ToList();
            }

        }
        public IList<string> SqlNames
        {
            get
            {
                return FieldInfo.Select(item => item.SqlName).ToList();
            }
        }
        public IEnumerable<KeyValuePair<string, string>> FieldNameMap
        {
            get
            {
                for (int i = 0; i < FieldInfo.Count; i++)
                {
                    yield return new KeyValuePair<string, string>(FieldInfo[i].Name, FieldInfo[i].SqlName);
                }
            }
        }
        /// <summary>
        /// When true, the class has been marked as "select all" and query implementors should use "*" or equivalent, instead of selecting fields.
        /// </summary>
        public bool SelectAll { get; protected set; }
        public string TableName { get; protected set; }

        /// <summary>
        /// The base query object from which new queries using "where" criteria will be constructed.
        /// </summary>

        public ISqlQuery Query(QueryType type) {
            if (_Query != null)
            {
                return _Query.Clone(type);
            }
            else
            {
                var query = new SqlQuery(type);
                query.From = TableName;
                query.Select = SelectAll ? "*" : String.Join(",", FieldNames);
                return query;
            }

            
        }

        public bool ExcludeProperties { get; protected set; }
        public MethodInfo IQEventHandlerMethod { get; protected set; }

        public bool TryGetFieldInfo(string fieldName, out IDBFieldInfo info)
        {
            int index;
            if (fieldIndexMap.TryGetValue(fieldName.ToLower(), out  index))
            {
                info = fieldInfo[index];
                return true;
            }
            else
            {
                info = null;
                return false;
            }

        }

        public IDBFieldInfo this[string fieldName]
        {
            get
            {
                return GetFieldInfo(fieldName);
            }
        }

        public IDBFieldInfo this[int fieldIndex]
        {
            get
            {
                return fieldInfo[fieldIndex];
            }
        }

        public int FieldIndex(string fieldName)
        {
            int index;
            if (fieldIndexMap.TryGetValue(fieldName.ToLower(), out index))
            {
                return index;
            }
            else
            {
                return -1;
            }
        }
       
        /// <summary>
        /// The default value for the primary key's datatype.
        /// </summary>
        public object PrimaryKeyDefaultValue { get; protected set; }


        #endregion

        #region public methods

        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tableName"></param>
        /// <param name="mappedFields"></param>
        public void MapClass(Type type, IQClassData data=null)
        {
            ClassType = type;

            GetClassMetadata();
            
            GetFieldInfoFromClassAttribute();

            GetClassPropertiesMethods();

            if (iqConstructor != null)
            {
                CallIQConstructor();
            }

            // Data passed in to this will supercede anything else0
            if (data != null)
            {
                ProcessConstructorData(data);
            }

            MapFromTemporaryFields();

            if (fieldInfo.Count == 0)
            {
                throw new Exception("There were no databound fields in the object.");
            }
            if (PrimaryKey == null)
            {
                throw new Exception("No primary key was found in the object.");
            }
        }
        public bool HasField(string fieldName)
        {
            return fieldIndexMap.ContainsKey(fieldName.ToLower());

        }
        public IDBFieldInfo GetFieldInfo(string fieldName)
        {
            return fieldInfo[fieldIndexMap[fieldName.ToLower()]];
        }

        public string SqlName(string fieldName)
        {
            return FieldInfo[fieldIndexMap[fieldName.ToLower()]].SqlName;
        }

        #endregion

        #region private methods

        private void GetClassMetadata()
        {
            object[] attributes = (object[])ClassType.GetCustomAttributes(true);
            foreach (var attr in attributes)
            {
                if (attr is IQClass)
                {
                    IQClass metaData = (IQClass)attr;
                    if (!String.IsNullOrEmpty(metaData.FieldMap))
                    {
                        fieldMap = metaData.FieldMap;
                    }
                    if (!String.IsNullOrEmpty(metaData.TableName))
                    {
                        TableName = metaData.TableName;
                    }
                    ExcludeProperties = metaData.ExcludeProperties;

                }

            }
        }
        private void GetFieldInfoFromClassAttribute()
        {
            temporaryFieldInfo = new Dictionary<string, Field>();
            // Check for a CSV field map to identify which properties to include. Otherwise, use attributes.
            if (!string.IsNullOrEmpty(fieldMap))
            {
                string[] fields = fieldMap.Split(',');
                foreach (var fld in fields)
                {
                    string fldNameClean = fld.Trim();
                    Field fldTemp = new Field();
                    fldTemp.Name = fldNameClean;

                    if (fldNameClean.IndexOf("(") > 0)
                    {
                        string[] parts = fldNameClean.Split(new char[] { '(', ',', ')', '=' }, StringSplitOptions.RemoveEmptyEntries);
                        fldNameClean = parts[0];
                        for (int p = 1; p < parts.Length; p++)
                        {
                            switch (parts[p].ToLower())
                            {
                                case "pk":
                                    fldTemp.PK = true;
                                    break;
                                case "sqlname":
                                    fldTemp.SqlName = parts[++p];
                                    break;
                                case "ignorenull":
                                    fldTemp.IgnoreNull = true;
                                    break;
                                //case "isnullable":
                                //    fldTemp.IsNullable = true;
                                //    break;
                                case "readonly":
                                    fldTemp.ReadOnly = true;
                                    break;
                                default:
                                    throw new Exception("Unknown field option '" + parts[p] + "' found with field '" + fldNameClean + "'");
                            }
                        }
                    }

                    temporaryFieldInfo[fldNameClean.ToLower()] = fldTemp;

                }
            }
        }
        /// <summary>
        /// Obtain and process data from a static IQ constructor 
        /// </summary>
        private void CallIQConstructor()
        {
            IQClassData data = new IQClassData();
            iqConstructor(data);
            ProcessConstructorData(data);
        }

        protected void ProcessConstructorData(IQClassData data)
        {
            ISqlQuery query = data.Query;
            if (query != null)
            {
                _Query = query;
                // Sync the two table names -- TODO fix this, perhaps always create a query instead of having sep.
                // properties?
                if (String.IsNullOrEmpty(_Query.TableName))
                {
                    _Query.TableName = data.TableName;
                }
                else if (String.IsNullOrEmpty(data.TableName))
                {
                    data.TableName = _Query.TableName;
                }
            }
            if (!String.IsNullOrEmpty(data.TableName))
            {
                if (!String.IsNullOrEmpty(TableName) && TableName.ToLower() != data.TableName.ToLower())
                {
                    throw new Exception("A table name was specified in the constructor, but another one was already indetified for the class.");
                }
                TableName = data.TableName;
            }
            if (!String.IsNullOrEmpty(data.PrimaryKey))
            {
                string nameLower = data.PrimaryKey.ToLower();
                Field fldTemp;
                if (!temporaryFieldInfo.TryGetValue(nameLower, out fldTemp))
                {
                    fldTemp = new Field();
                    temporaryFieldInfo[nameLower] = fldTemp;
                    fldTemp.Name = data.PrimaryKey;                       
                }
                fldTemp.PK = true;
            }
        }

       
        private void GetClassPropertiesMethods()
        {

            // It's a regular object. It cannot be extended, but set any same-named properties.
            IEnumerable<MemberInfo> members = ClassType
                .GetMembers(BindingFlags.Public | 
                            BindingFlags.NonPublic | 
                            BindingFlags.Instance |
                            BindingFlags.Static);


            //List<Field> tempList = new List<Field>();

            bool hasMap = fieldIndexMap.Count > 0;

            foreach (var member in members)
            {
                if (member is PropertyInfo)
                {
                    PropertyInfo prop = (PropertyInfo)member;

                    // Skip properties that don't have both read and write methods, and ones with no public get.
                    if (!prop.CanWrite || !prop.CanRead ||
                        !Utils.IsMappableType(prop.PropertyType))
                    {
                        continue;
                    }
                    Field fldTemp;

                    string nameLower = prop.Name.ToLower();

                    bool alreadyMapped = true;
                    // Use the fieldmap info if it exists already as a starting point - but override with any attribute data
                    if (!temporaryFieldInfo.TryGetValue(nameLower, out fldTemp)) {
                        fldTemp = new Field();
                        alreadyMapped = false;
                    }

                    // Always update name from property, even if field was already mapped from constructor
                    fldTemp.Name = prop.Name;
                    fldTemp.PropInfo = prop;
                    fldTemp.HasPublicGetter = prop.GetGetMethod() != null;

                    object[] attributes = (object[])member.GetCustomAttributes(true);
                    bool ignore = false;
                    //bool nullable = false;

                    foreach (var attr in attributes)
                    {
                        if (attr is IQIgnore)
                        {
                            if (alreadyMapped)
                            {
                                throw new Exception("The field '" + nameLower + "' is marked for ignore, but has been identified in a constructor alread.");
                            }
                            ignore = true;
                            break;
                        }
                        if (attr is IQPrimaryKey)
                        {
                            fldTemp.PK = true;
                        }
                        IQField fldAttr=null;
                        
                        if (attr is IQField)
                        {
                            fldTemp.FieldAttr = true;
                            fldAttr = (IQField)attr;
                        }                  

                        string sqlName = fldTemp.SqlName;
                        if (fldAttr != null)
                        {
                            if (!String.IsNullOrEmpty(fldAttr.SqlName))
                            {
                                sqlName = fldAttr.SqlName;
                            }
                            if (fldAttr.PK )
                            {
                                fldTemp.PK = true;
                            }
                            if (fldAttr.IgnoreNull)
                            {
                                fldTemp.IgnoreNull = true;
                            }
                            //if (fldAttr.IsNullable)
                            //{
                            //    fldTemp.IsNullable = true;
                            //}
                            if (fldAttr.ReadOnly)
                            {
                                fldTemp.ReadOnly = true;
                            }
                        }
                        fldTemp.SqlName = sqlName;
                    }
                    if (!ignore)
                    {
                        temporaryFieldInfo[nameLower] = fldTemp;
                    }
                }
                else if (member is MethodInfo)
                {
                    MethodInfo method = (MethodInfo)member;
                    object[] attributes = (object[])member.GetCustomAttributes(true);

                    foreach (var attr in attributes)
                    {
                        if (attr is IQEventHandler)
                        {
                            IQEventHandlerMethod = method;
                        }
                        if (attr is IQConstructor)
                        {
                            try
                            {
                                iqConstructor = (Action<IQClassData>)Delegate
                                    .CreateDelegate(typeof(Action<IQClassData>), method);
                            }
                            catch
                            {
                                throw new Exception("The IQ Constructor must have signature 'static void IQConstructor(IQClassData data)'");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Take all the info we've gathered form constructors, parameters, attributes, etc. and build the map
        /// </summary>
        protected void MapFromTemporaryFields()
        {

            foreach (var item in temporaryFieldInfo.Values)
            {
                // Logic: using default (!excludeproperties) handling, anything with a public getter is included automatically.
                // In addition, only things identified in some way should be included.
                if ((!ExcludeProperties && item.HasPublicGetter) 
                    || item.FieldAttr || item.PK)
                {
                    string nameLower = item.Name.ToLower();

                    //fieldNames.Add(item.Name);
                    //sqlFieldNames.Add(item.SqlName);

                    // use the best SqlName - if not present in the default data source, then get from the property name
                    string sqlName = String.IsNullOrEmpty(item.SqlName) ? item.Name : item.SqlName;

                    IDBFieldInfo fldInfo = new DBFieldInfo(item.PropInfo, sqlName,
                        isPk: item.PK,
                        ignoreNull: item.IgnoreNull,
                        isReadOnly: item.ReadOnly);


                    if (item.PK)
                    {
                        if (PrimaryKey != null)
                        {
                            throw new Exception("A different primary key '" + PrimaryKey.Name +
                                "' has already been identified, cannot make '" + item.Name + "' the pk.");
                        }
                        PrimaryKey = fldInfo;
                    }
                   
                    fieldInfo.Add(fldInfo);
                    fieldIndexMap[nameLower] = fieldInfo.Count - 1;

                }
            }

            if (PrimaryKey == null && fieldInfo.Count>0)
            {
                PrimaryKey = fieldInfo[0];
                ((DBFieldInfo)fieldInfo[0]).IsPrimaryKey = true;
            }

            temporaryFieldInfo = null;
        }
        struct Field
        {
            public string Name;
            public string SqlName;
            public bool ReadOnly;
            public bool FieldAttr;
            public bool PK;
            public bool HasPublicGetter;
            public bool IgnoreNull;
            public PropertyInfo PropInfo;
            //public bool IsNullable;
        }
        #endregion
    }
}