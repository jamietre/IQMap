## IQMap - Instant Query Mapper

2/1/12

IQMap is a Dapper-inspired ORM. It seeks to fill the void between quick & dirty. That is to say, 
it seeks to combine the elegance and simplicty of Dapper, with some advanced features for object
data management. It was written from scratch and does not actually share any code with Dapper, 
though I have borrowed some tests.

Basi

IQMap features:

- Works with POCO objects
- Uses attributes to identify special behavior, though you can also control that behavior by altering metadata for an object. This means you could take advantage of all of its features without any class decoration.
- Works with any ADO.NET provider, though some features might require you to implement a strategy. (e.g. the code to return a new ID from an insert is 
not the same for
- Tries to support all kinds of natural syntaxes for passing queries. When using with marked-up objects (see attributes, below)
  you only need to pass criteria for most queries.

Basic behavior:

- Maps any matching fields from an SQL query to a same-named property.
- Only properties with public getters are included by default (though you can override this behavior by marking a field).
  Fields and private-get/internal properties are not included by default.
- You can map a property to a different SQL name using the "SqlName" option of the IQField attribute.
- Metadata about each object you load with IQMap is available, including field-level dirty state. 
  (This info is used to generate optimized update queries for saving -- only changed data is included).

    // Pass just "where" criteria for most queries when using objects with metadata
	
	var someObject = IQ.Load<SomeObject>("DataField=12");
	var someObject = IQ.Load<SomeObject>("DataField=@value",12);
	IEnumerable<SomeObject>= IQ.LoadMany<SomeObject>("DataField > @value",12);
	IQ.Save(someObject);

	// Can be used on any objects -- generic "Query" on a connection returns a datareader.

	Cat cat1;
	Cat cat2;
    connection.Query("select * FROM animals")
		.MapNext(cat1)
		.MapNext(cat2)
		.Dispose();
	
	// Can be used with value types

	IEnumerable<int> kittenIDs = IQ.Query("select kittenID from kittens where catId=@catID",cat1.catID)
	    .MapAll<int>();

	// DataReaders are closed automatically except for chained methods, when using DataReader extension methods.

	IEnumerable<Cat> cats = IQ.Query("select * FROM animals where type='cat'")
		.MapAll<Cat>();

	Cat firstCat = IQ.Query("select * FROM animals where type='cat'")
		.MapFirst<Cat>();


    // Pass parameters by index
    IQ.Delete<SomeObject>("Obsolete=@obsolete and DestroyDate<@today",isObsolete,DateTime.Now);

	// Pass parameters by name
    IQ.Delete<SomeObject>("Obsolete=@obsolete and DestroyDate<@today","@today",DateTime.Now,"@obsolete",isObsolete);

	// Dapper style - use objects
    IQ.Delete<SomeObject>("Obsolete=@obsolete and DestroyDate<@today",new {today = DateTime.Now,obsolete=isObsolete});
	IQ.Delete<SomeObject>("Obsolete=@obsolete and DestroyDate<@today",new {today = DateTime.Now,obsolete=isObsolete});


Unlike Dapper it is not a single file that you can add to your project, but the code's a lot easier to read.

IQMap exposes a static object IQ with the following properties/methods:

    // Default connection object
    IDbConnection Connection                      

	// Configuration data
	IQMap.Config Config                           
	
	// Save an object using a primary key
	bool Save(object obj)
	
	// Create a new instance of class T, loading data for primary key  
    T LoadPK<T>(IConvertible primaryKeyValue)      
	T Load<T>(string query, params object[] parameters) 
	
	bool TryLoad<T>(string query, out T obj, params object[] parameters)
	bool TryLoadPK<T>(IConvertible primaryKeyValue, out T obj)

	// Load all matching records into new objects or value types
	IEnumerable<T> LoadMultiple<T>(string query, params object[] parameters)
	
	// Delete by primary key, return records affected
	int Delete<T>(IConvertible primaryKeyValue)

	// Delete any records matching 
	int Delete<T>(string query, params object[] parameters)
        {
            return Config.DataController.Delete<T>(IQ.Config.DefaultConnection, query, parameters);
        }
        public static int QueryScalar(string query, params object[] parameters)
        {
            return Config.DataController.QueryScalar(IQ.Config.DefaultConnection, query, parameters);
        }

        public static IDataReader Query(string query, params object[] parameters)
        {
            return Config.DataController.Query(IQ.Config.DefaultConnection, query, parameters);
        }

        public static IDBClassInfo GetClassInfo<T>()
        {
            return GetClassInfo(typeof(T));
        }
        public static IDBClassInfo GetClassInfo(Type type)
        {
            return DBObjectData.GetClassInfo(type);
        }
        public static IDBObjectData DBData(object obj)
        {
            IDBObjectData dbData;
            if (ObjectMetadata.TryGetValue(obj, out dbData))
            {
                if (ReferenceEquals(dbData.Owner, obj))
                {
                    return dbData;
                }
            }
            // Not found - must check everything in the DB since GetHashCode isn't guaranteeed to be unique or to stay the same
            // There are probably ways to optimize this, in fact, it may not even be necessary, but it should be pretty
            // inexpensive unless dealing with huge numbers of objects

            foreach (KeyValuePair<object, IDBObjectData> kvps in ObjectMetadata)
            {
                if (ReferenceEquals(kvps.Value.Owner, obj))
                {
                    return kvps.Value;
                }
                else if (kvps.Value.Orphaned)
                {
                    RemoveFromDict(kvps.Key);
                }
            }

            // Definitely not in the dictionary - create it

            IDBObjectData newObjectData = new DBObjectData(obj);
            ObjectMetadata[obj] = newObjectData;
            return newObjectData;

        }

* ATTRIBUTES *

IQMetaData - applies to a class.

	// name of SQL table that this class maps to
	string TableName;   
	
	// a csv of field info for this class, which can be used instead of field-level attributes e.g.
	// "TableID(pk),JoinedField(readonly),FirstName,LastName" will identify the fields to include and attributes
	string FieldMap 
	
	// when true, all public gettable properties are excluded by default and only ones marked with an attribute 
	// or mapped with FieldMap)  are included.
	bool ExcludeProperties

	// when true, uses * from select queries instead of producing a field list
    public bool SelectAll;


IQField - identifies a property as being included in the map (if not already) and defines behavior

    // SQL column name that is associated with this property
    public string SqlName;

	// the field is the primary key
    public bool IsPrimaryKey;

	// do not try to update this field when saving
    public bool ReadOnly;
	
	// if null values are found for a non-nullable property, use the data type default instead when mapping
	public bool IgnoreNull;