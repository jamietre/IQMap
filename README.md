# IQMap - Instant Query Mapper #

6/4/12

IQMap is a Dapper-inspired ORM. It seeks to fill the void between quick & dirty. That is to say, it seeks to combine the elegance and simplicty of Dapper, with some advanced features for object data management. It was written from scratch and does not actually share any code with Dapper, though I have borrowed some tests.

This project has evolved into something more like Entity Framework but without the baggage. That is, my goal is to completely implement an `IQueryable`  model that generates good SQL, but works against POCO objects with no implicit knowledge of their provider. Practically speaking, something needs to know about table names, primary keys, and columns, unless you want to type them into every query. The approach here is minimalist. You can use a true POCO and write queries against it, or you can decorate an object with just a table & primary key name, and be able to use LINQ-like queries to construct an efficient query in code.

*The API is substantially changed from the last push in February. It also works a lot better.*

IQMap features:

* Works with POCO objects

* Uses attributes to identify special behavior, though you can also control that behavior by altering metadata for an object. This means you could take advantage of all of its features without any class decoration, but decorating DAOs will make your life easier.

* Should work with any ADO.NET provider, though it will require you to implement a provider-specific strategy. (See Implementation/EngineSpecific)

* Tries to support all kinds of natural syntaxes for passing queries. When using with marked-up objects (see attributes, below) you only need to pass criteria for most queries.

* When using the query builder and decorated classes, queries are conducted in the most efficient manner possible. 

For example, with a class Dog, decorated to tell IQMap the fundamentals it needs to know about the table:

    [IQClass(PrimaryKey="id",TableName="dogs")]
    public class Dog {
        public int Id { get; private set; }
        public string Breed { get;set; }
        public string Name { get;set; }
        ...
    } 

This code: 

    Dog dog = IQ.From<Dog>().Select("id").OrderBy("breed").First();

will create a query that looks like this:

    SELECT TOP 1 id FROM dogs ORDER BY breed;

That is: First knows it only needs one element. The query doesn't get constructed until the first method that actually requests data is executed (in fact, it will never be constructed at all, until it's actually enumerated). The `First` implementation alters the query construct to request only one record.

If you didn't provide the "Select", it would select all fields. If you didn't provide the "First", but instead just iterated over the output, e.g.:

    IEnumerable<Dog> dogs = IQ.From<Dog>().Select("id").OrderBy("breed");

it would not include the TOP clause.

The code that builds the query is highly modularized and it's easy to create an implementation for, say, MySql, that just outputs the right language for result bounding, and other provider-specific things like returning an identity ID after an INSERT. I've provided implementations for MS SQL Server and SQL Server CE.

Basic behavior:

* Maps any matching fields from an SQL query to a same-named property.

* Only properties with public getters are included by default (though you can override this behavior by marking a field). Fields and private-get/internal properties are not included by default.

* You can map a property to a different SQL name using the "SqlName" option of the IQField attribute.

* Metadata about each object you load with IQMap is available, including field-level dirty state.  (This info is used to generate optimized update queries for saving -- only changed data is included).



Examples: 

Pass just "where" criteria for most queries when using objects with metadata. When using
methods of the static IQ object, a default connection is used. SQL server is the default
provider implementation.

    IQ.Config.DefaultConnectionString="Data Source=localhost;Initial Catalog=tempdb;Integrated Security=True";
    
    var someObject = IQ.Load<SomeObject>("DataField=12");
    var someObject = IQ.Load<SomeObject>("DataField=@value",12);   

    IEnumerable<SomeObject> list = IQ.Query<SomeObject>("DataField > @value",12);
    IQ.Save(someObject);


It's a lot more fun to use decorated objects that know about the table name, though.

    var dogs = IQ.From<Dog>()
                 .Where("pk<@pk", 5)
                 .OrderBy("breed")
                 .ThenBy("pk")
                 .Reverse()
                 .First();

     == > SELECT TOP 1 * FROM Dogs where breed=@breed order by breed desc, pk desc

Every method creates a new instance of the querybuilder, so you can cache a partial query
and do different stuff to it.
    
    var dogs = IQ.From<Dog>();
    
    var onlyTerriers = dogs.Where("breed","Terrier");
    
    var firstTerrier = onlyTerriers.First();
    var secondTerrier = onlyTerriers.ElementAt(1);

Pretty much every LINQ method has been implement to optimize query creation when possible. If you aren't using the querybuilder, e.g. are starting from "Query", it will still defer execution until the end, though it may not be able to optimize.


You can also return a datareader if you want.. some extension methods are provided
for IDataReader to help you out. RunSql is also an extension method of IDbConnection.

    Cat cat1;
    Cat cat2;
    connection.RunSql("select * FROM animals")
        .MapNext(cat1)
        .MapNext(cat2)
        .Dispose();
    
    // Can be used with value types. The first column returned is always the source of your
    // value-typed data.

    IEnumerable<int> kittenIDs = IQ.Query<int>("select kittenID from kittens where catId=@catID",cat1.catID);

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


IQMap encapsulates its database interface methods into an object implementing an interface called `IDataStorageController` to expose its basic functionality. This has a number of basic methods to access an database. The abstract `SqlDataStorageController` class does most of the work and is universal for any ANSI SQL provider. Some provider-specific stuff must be implemented by overriding this basic class.

The included implementation `SqlDataController` should work with any roughly-ANSI SQL server. For non-MSSQL server databases, though, you may need to override the included `MSSQLDataStorageController` class. This has only a few methods, but they do important things like: 

* Returning the new ID for an inserted record
* Implementing FirstRow and LastRow for selecting ranges (note: at this moment in time -- those aren't implemented for MSSQL either,
  but that should be done very shortly)

These are not standardized, e.g. MySQL uses a LIMIT clause for row ranges. But they are extraordinarily useful, and so this core funcionality has been abstracted to make it easy to support other server types. It should be trivial to override the MSSQL implementation for most anything else.


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

`IQClass` - marks a class as being bound to a table. 

    // name of SQL table that this class maps to
    string TableName;   
    
    // primary key of the table.
    string PrimaryKey    

    // when true, uses * from select queries instead of producing a field list
    public bool SelectAll;

 If you mark something with `IQClass`, IQMap will look for a nonpublic, nonstatic method with this signature:

    protected void Constructor(IClassInfoConstructor data)
    {
        data.Query.TableName = "animals";
        data.Query.PrimaryKey = "PK";
        data.AddEventHandler(IQEventType.OnSave, OnSave);
    }

This is the "IQ constructor" and can be used to set up events and configure the base query definition, if it is more complex then a 1:1 class-to-table relationship. The "Query" property of the "data" parameter lets you define any or all parts of the initial query clause. Methods used to construct queries will build on this.



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
 
 
 