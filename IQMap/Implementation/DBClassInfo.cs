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

        private Lazy<Dictionary<string, int>> _fieldIndexMap = new Lazy<Dictionary<string, int>>();
        protected Dictionary<string, int> fieldIndexMap
        {
            get
            {
                if (!_fieldIndexMap.IsValueCreated)
                {
                    throw new Exception("You haven't mapped to an object yet, cannot access FieldInfo");
                }
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

        public IDBFieldInfo GetFieldInfo(string fieldName)
        {
            return fieldInfo[fieldIndexMap[fieldName.ToLower()]];
        }

        public string SqlName(string fieldName)
        {
            return FieldInfo[fieldIndexMap[fieldName.ToLower()]].SqlName;
        }

        public void MapClass(Type type, string tableName = "", string mappedFields = "")
        {

            GetClassMetadata(type);

            // calling MapClass with a mappedField parm will override attribute data

            if (!String.IsNullOrEmpty(mappedFields))
            {
                fieldMap = mappedFields;
            }
            if (!string.IsNullOrEmpty(tableName))
            {
                TableName = tableName;
            }
            temporaryFieldInfo = new Dictionary<string, Field>();
            // Check for a CSV field map to identify which properties to include. Otherwise, use attributes.
            if (!string.IsNullOrEmpty(fieldMap))
            {
                //Dictionary<string, int> indexMap = _fieldIndexMap.Value;
 

                string[] fields = fieldMap.Split(',');
                foreach (var fld in fields)
                {
                    string fldClean = fld.Trim();
                    string fldCleanLower = fldClean.ToLower();
                    Field fldTemp = new Field();
                    fldTemp.Name = fldClean;
                    fldTemp.SqlName = fldClean;
                    fldTemp.InMap = true;

                    if (fldClean.IndexOf("(") > 0)
                    {
                        string[] parts = fldClean.Split(new char[] { '(', ',', ')', '=' }, StringSplitOptions.RemoveEmptyEntries);
                        fldClean = parts[0];
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
                                    fldTemp.ConvertNullToDefault = true;
                                    break;
                                case "readonly":
                                    fldTemp.ReadOnly = true;
                                    break;
                                default:
                                    throw new Exception("Unknown field option '" + parts[p] + "' found with field '" + fldClean + "'");
                            }
                        }
                    }

                    temporaryFieldInfo[fldCleanLower] = fldTemp;

                }
            }

            GetDatabaseFields(type);

            if (fieldInfo.Count == 0)
            {
                throw new Exception("There were no databound fields in the object.");
            }
            if (PrimaryKey == null)
            {
                throw new Exception("No primary key was found in the object.");
            }
        }

        #endregion

        #region private methods

        private void GetClassMetadata(Type type)
        {
            object[] attributes = (object[])type.GetCustomAttributes(true);
            foreach (var attr in attributes)
            {
                if (attr is IQMetaData)
                {
                    IQMetaData metaData = (IQMetaData)attr;
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
        private void GetDatabaseFields(Type type)
        {
            Dictionary<string, int> indexMap = _fieldIndexMap.Value;

            // It's a regular object. It cannot be extended, but set any same-named properties.
            IEnumerable<MemberInfo> members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);


            List<Field> tempList = new List<Field>();

            bool hasMap = indexMap.Count > 0;

            foreach (var member in members)
            {
                if (member is PropertyInfo)
                {
                    PropertyInfo prop = (PropertyInfo)member;

                    // Skip properties that don't have both read and write methods, and ones with no public get.
                    if (!prop.CanWrite || !prop.CanRead)
                    {
                        continue;
                    }
                    Field fldTemp;

                    string nameLower = prop.Name.ToLower();

                    // Use the fieldmap info if it exists already as a starting point - but override with any attribute data
                    if (!temporaryFieldInfo.TryGetValue(nameLower, out fldTemp)) {
                        fldTemp = new Field();
                    }

                    fldTemp.Name = prop.Name;
                    fldTemp.PropInfo = prop;
                    fldTemp.HasPublicGetter = prop.GetGetMethod() != null;

                    object[] attributes = (object[])member.GetCustomAttributes(true);
                    bool ignore = false;

                    foreach (var attr in attributes)
                    {
                        if (attr is IQIgnore)
                        {
                            if (fldTemp.InMap)
                            {
                                throw new Exception("The field '" + nameLower + "' is both in the field map, and marked for ignore.");
                            }
                            ignore = true;
                            break;
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
                            if (fldAttr.IsPrimaryKey )
                            {
                                fldTemp.PK = true;
                            }
                            if (fldAttr.IgnoreNull)
                            {
                                fldTemp.ConvertNullToDefault = true;
                            }
                            if (fldAttr.ReadOnly)
                            {
                                fldTemp.ReadOnly = true;
                            }
                        }
                        fldTemp.SqlName = sqlName;
                    }
                    if (!ignore)
                    {
                        tempList.Add(fldTemp);
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
                    }
                }
            }
            // TempList has info on all fields in the class. We don't know until looking through the whole thing if they've
            // used marker attributes or not. If not, add them all. Ignored ones won't be here.


            foreach (var item in tempList)
            {
                // Logic: using default (!excludeproperties) handling, anything with a public getter is included automatically.
                // In addition, only things identified in some way should be included.
                if ((!ExcludeProperties && item.HasPublicGetter) 
                    || item.FieldAttr || item.PK || item.InMap)
                {
                    string nameLower = item.Name.ToLower();
                    // Remove & Re-Add if its from the map to preserve case from property names
                    // TODo clean this up. Don't add to the index up front, create separate list
                    bool inMap = item.InMap;

                    
                    Field fld = inMap ? temporaryFieldInfo[nameLower] : item;

                    //fieldNames.Add(item.Name);
                    //sqlFieldNames.Add(item.SqlName);

                    // use the best SqlName - if not present in the default data source, then get from the property name
                    string sqlName = String.IsNullOrEmpty(fld.SqlName) ? item.SqlName : fld.SqlName;

                    IDBFieldInfo fldInfo = new DBFieldInfo(item.PropInfo, sqlName,
                        isPk:fld.PK,
                        ignoreNull: fld.ConvertNullToDefault,
                        isReadOnly: fld.ReadOnly);

                    
                    if (fld.PK)
                    {
                        if (PrimaryKey != null)
                        {
                            throw new Exception("A different primary key '" + PrimaryKey.Name +
                                "' has already been identified, cannot make '" + fld.Name + "' the pk.");
                        }
                        PrimaryKey = fldInfo;
                    }
    
                   
                    fieldInfo.Add(fldInfo);
                    indexMap[nameLower] = fieldInfo.Count - 1;

                }
            }

            if (fieldInfo.Count == 0)
            {
                throw new Exception("The class has no properties that were mapped to the database.");
            }


            if (PrimaryKey==null)
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
            public bool InMap;
            public bool ReadOnly;
            public bool FieldAttr;
            public bool PK;
            public bool HasPublicGetter;
            public bool ConvertNullToDefault;
            public PropertyInfo PropInfo;
        }
        #endregion
    }
}