
### IQMap ToDo

IMPORTANT TODO:

1) Need a convention to tell when to track changes to an object and when not to. Too wasteful to do this
for basic query results.
2) Garbage collection
3) CACHING: Optiosn to control cache lifetime. Will also make testing easier.

BUGS:

IgnoreNull not working

##### 1) Implement the rest of linq methods as appropriate.
##### 2) Should we create a distinction between the typed(bound) methods and untyped ones? Generally speaking I think so.

Typed methods, you already know about the table def. They should always just accept where clauses. This eliminates all the uncertainty.

Untyped methods should always be expected to be passed a full query.

So what if you want to run an arbitrary query and then map to a specific object? Well, this by definiton does not need the object def to construct the query. 
You should just use the map method.

   
   IEnumerable<dynamic>   Select
   IEnumerable<T>         Select<T>()


var target = iq.Select("select * from something left join somethingelse where whatever").Map();



##### 3) Merge in IqMap
##### 