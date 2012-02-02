# IQMap - Instant Query Mapper #

2/1/12

IQMap is a Dapper-inspired ORM. It seeks to fill the void between quick & dirty. That is to say, 
it seeks to combine the elegance and simplicty of Dapper, with some advanced features for object
data management. It was written from scratch and does not actually share any code with Dapper, 
though I have borrowed some tests.


IQMap features:

* Works with POCO objects
* Uses attributes to identify special behavior, though you can also control that behavior by altering metadata for an object. This means you could take advantage of all of its features without any class decoration.
* Works with any ADO.NET provider, though some features might require you to implement a strategy. (e.g. the code to return a new ID from an insert is 
not the same for
* Tries to support all kinds of natural syntaxes for passing queries. When using with marked-up objects (see attributes, below) you only need to pass criteria for most queries.

Basic behavior:

* Maps any matching fields from an SQL query to a same-named property.
* Only properties with public getters are included by default (though you can override this behavior by marking a field). Fields and private-get/internal properties are not included by default.
* You can map a property to a different SQL name using the "SqlName" option of the IQField attribute.
* Metadata about each object you load with IQMap is available, including field-level dirty state.  (This info is used to generate optimized update queries for saving -- only changed data is included).


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

IQMap encapsulates its database interface methods into two objects implementing an interface called `IDataController` 
to expose its basic functionality. An `IDataController` also has a depenency of an `IDataStorageController` that
provides low-level access to a data service's query methods. 

The included implementation `SqlDataController` should work with any roughly-ANSI SQL server. For non-MSSQL server
databases, though, you may need to override the included `MSSQLDataStorageController` class. This has only a few methods,
but they do important things like: 

* Returning the new ID for an inserted record
* Implementing FirstRow and LastRow for selecting ranges (note: at this moment in time -- those aren't implemented for MSSQL either,
  but that should be done very shortly)

These are not standardized, e.g. MySQL uses a LIMIT clause for row ranges. But they are extraordinarily useful, 
and so this core funcionality has been abstracted to make it easy to support other server types. It should be 
trivial to override the MSSQL implementation for most anything else.

If you wanted to support a totally non-sql data mechanism, well, you could easily write an `IDataController` that worked
with any query language.

These methods are offered by the `IDataController` class. Then, various forms are available as extensions of `IDataReader`, 
`IDataRecord`, and `IDbConnection`, as well as methods of the static `IQ` object.

    // Returns a new implementation-specific connection
    IDbConnection GetConnection(string connectionString);

    // Save an object by primary key value, return true if anything was done (e.g. non-dirty save returns false).
    bool Save(IDbConnection connection, object obj);

    // Select by primary key, and return new instance of T
    T LoadPK<T>(IDbConnection connection, IConvertible primaryKeyValue) where T : new();

    // Run an arbitrary query and map to object or value type T
    T Load<T>(IDbConnection connection, string query, params object[] parameters);
        
    // "try" versions of above
    bool TryLoad<T>(IDbConnection connection, string query, out T obj, params object[] parameters);
    bool TryLoadPK<T>(IDbConnection connection, IConvertible primaryKeyValue, out T obj) where T : new();

    // Return one instance of type or value T for each query result. If buffered=true, entire result set will
    // be loaded into memory. Be cautious when using this method without buffering.
    IEnumerable<T> LoadMultiple<T>(IDbConnection connection, string query, bool buffered, params object[] parameters);

    int DeletePK<T>(IDbConnection connection, IConvertible primaryKeyValue) where T : new();
    int Delete<T>(IDbConnection connection, string query, params object[] parameters);

    IDataReader Query(IDbConnection connection, string query, params object[] parameters);
    int QueryScalar(IDbConnection connection, string query, params object[] parameters);


`IDataReader` extensions: These are designed to be chanied from a `Query` above but will, of course, work with
any IDataReader.

    // Map the first row of the reader to a new T and dispose the reader
    T MapFirst<T>(this IDataReader reader)
    
    // Map the first row of the reader to obj and dispose the reader. Returns false if no data.
    bool MapFirst(this IDataReader reader, object obj)

    // Get the next item from the datareader. (This is same as MapFirst when called with closeDataReader=true).
    T MapNext<T>(this IDataReader reader, bool closeDataReader=true) 
    
    // Map the next item from the datareader into obj and return the reader. Therefore, this can be chained to process
    // multiple rows.
    IDataReader MapNext(this IDataReader reader, object obj)

    // Map a record (the current row of a datareader, e.g.) to a new T.
    T Map<T>(this IDataRecord record)

    // Map a record to an existing object
    void Map(this IDataRecord reader,object obj)
    
    // Map each row of a datareader to new T objects/value types. If buffered=true, all data is loaded up front and
    // the reader is closed. Otherwise, the reader won't be closed until the client finishes enumeration (be careful).
    IEnumerable<T> MapAll<T>(this IDataReader reader, bool buffered=true) 


*ATTRIBUTES*

`IQMetaData` - applies to a class.

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


`IQField` - identifies a property as being included in the map (if not already) and defines behavior

    // SQL column name that is associated with this property
    public string SqlName;

    // the field is the primary key
    public bool IsPrimaryKey;

    // do not try to update this field when saving
    public bool ReadOnly;
    
    // if null values are found for a non-nullable property, use the data type default instead when mapping
    public bool IgnoreNull;
    
`IQEventHandler` - marks a method to be called for IQ-managed events (save, load, update, delete). That is, whenever
    you use a strongly-typed method on an object, it will call this function on certain events. The signature should be
    
     public void IQEvent(IQEventType type, IDBObjectData data)
     
 The 1st parameter is a flags enum with info about the even type. The 2nd is the metadata for this object instance and
 can be used to check dirty state on fields, enumerate bound fields, and so on. (This can also be gotten with
 `IQ.DBData(object)`.
 
 `IQIgnore` -  Skip this field entirely
 
 
 *Some Internal Discussion*
 
 IQMap keeps a cache of metadata for each type that it knows about. It builds this cache the first time it's used against
 a particular object type to enumerate the properties and so on, and keeps it in a concurrent dictionary.
 
 Additionally, when you `Load` a strongly typed object, it keeps track of the initial values from the SQL database to 
 provide dirty state information. This is used to provide optimized update queries, and may also come in handy for lots
 of other reasons.
 
 The cache gets flushed periodically by checking for broken `WeakReference`s. 
 
 The reflection should be pretty fast because all the hard work is done only once per object type. 
 
 Storing metadata about POCOs is a little tricky. If you override `GetHashCode()` such that it can change based on value data,
 things could get slow, because if it doesn't get a reference match when looking up by hash code, there's nothing else to do
 but look through the whole dictionary.
 
 This shouldn't happen often in practice, unless you have thousands of bound database objects in memory simultaneously with
 value-based hash codes (e.g. they would all be the same for newly created objects). Since I could imagine such a scenario,
 though, this is an area for improvement.
 
 
 