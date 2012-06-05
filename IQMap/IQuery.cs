using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace IQMap
{   /// <summary>
    /// Most common interface for a Query or QueryBuilder. This should possibly include nongeneric versions of the final methods.
    /// </summary>
    public interface IQuery : IQueryable
    {
        IDbContext Context { get; set; }
        ISqlQuery QueryDef { get; }

        /// <summary>
        /// Return a single matching element from the SELECT query, or throw an exception if there is not exactly one result
        /// </summary>
        /// <returns>An object of type T</returns>
        object Single();
        /// <summary>
        /// Return a single matching element from the SELECT query, default(T) if there are no matching results,
        /// or an exception if there are more than one result
        /// </summary>
        /// <returns>An object of type T</returns>
        object SingleOrDefault();
        /// <summary>
        /// Return a single matching element from the SELECT query, or a new instance of the type T if there are no matching
        /// results, or an exception if there are more than one result
        /// </summary>
        /// <returns>An object of type T</returns>
        object SingleOrNew();

        /// <summary>
        /// Return the first element in a SELECT query.
        /// </summary>
        /// <returns>An object of type T</returns>
        object First();
        /// <summary>
        /// Return the first element in a SELECT query, or default(T) if there are no results
        /// </summary>
        /// <returns>An object of type T</returns>
        object FirstOrDefault();
        /// <summary>
        /// Return the first element in a SELECT query, or a new instance of T if there are no results
        /// </summary>
        /// <returns>An object of type T</returns>
        object FirstOrNew();

        /// <summary>
        /// Return the last element in a SELECT query.
        /// </summary>
        /// <returns>An object of type T</returns>
        object Last();
        /// <summary>
        /// Return the last element in a SELECT query, or default(T) if there are no results
        /// </summary>
        /// <returns>An object of type T</returns>
        object LastOrDefault();
        /// <summary>
        /// Return the last element in a SELECT query, or a new instance of T if there are no results
        /// </summary>
        /// <returns>An object of type T</returns>
        object LastOrNew();


        /// <summary>
        /// Return the Nth element return from a SELECT query. If the query has fewer than N results, throw an exception.
        /// </summary>
        /// <returns>An object of type T</returns>
        object ElementAt(int position);
        /// <summary>
        /// Return the Nth element return from a SELECT query, or default(T) if there are fewer than N results.
        /// </summary>
        /// <returns>An object of type T</returns>
        object ElementAtOrDefault(int position);

        bool Any();
        bool One();
        int Count();

        /// <summary>
        /// Return the output as a sequence of IDictionary-String,Object-
        /// </summary>
        /// <returns></returns>
        IQuery<IDictionary<string, object>> AsDictionaries();

        /// <summary>
        /// Map the results of the SELECT query to type U
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        //IEnumerable<U> Map<U>();
        /// <summary>
        /// Cast the output to type U
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <returns></returns>
        IQuery<U> As<U>();



        IEnumerable Enumerate();
    }


}
