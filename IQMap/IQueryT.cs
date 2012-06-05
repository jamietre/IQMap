using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using IQMap.SqlQueryBuilder;
using IQMap.SqlQueryBuilder.Impl;

namespace IQMap
{

    public interface IQuery<T> : IQuery, IEnumerable<T>, IDisposable
    {

        /// <summary>
        /// Return a single matching element from the SELECT query, or throw an exception if there is not exactly one result
        /// </summary>
        /// <returns>An object of type T</returns>
        new T Single();
        /// <summary>
        /// Return a single matching element from the SELECT query, default(T) if there are no matching results,
        /// or an exception if there are more than one result
        /// </summary>
        /// <returns>An object of type T</returns>
        new T SingleOrDefault();
        /// <summary>
        /// Return a single matching element from the SELECT query, or a new instance of the type T if there are no matching
        /// results, or an exception if there are more than one result
        /// </summary>
        /// <returns>An object of type T</returns>
        new T SingleOrNew();

        /// <summary>
        /// Return the first element in a SELECT query.
        /// </summary>
        /// <returns>An object of type T</returns>
        new T First();
        /// <summary>
        /// Return the first element in a SELECT query, or default(T) if there are no results
        /// </summary>
        /// <returns>An object of type T</returns>
        new T FirstOrDefault();
        /// <summary>
        /// Return the first element in a SELECT query, or a new instance of T if there are no results
        /// </summary>
        /// <returns>An object of type T</returns>
        new T FirstOrNew();

        /// <summary>
        /// Return the last element in a SELECT query.
        /// </summary>
        /// <returns>An object of type T</returns>
        new T Last();
        /// <summary>
        /// Return the last element in a SELECT query, or default(T) if there are no results
        /// </summary>
        /// <returns>An object of type T</returns>
        new T LastOrDefault();
        /// <summary>
        /// Return the last element in a SELECT query, or a new instance of T if there are no results
        /// </summary>
        /// <returns>An object of type T</returns>
        new T LastOrNew();


        /// <summary>
        /// Return the Nth element return from a SELECT query. If the query has fewer than N results, throw an exception.
        /// </summary>
        /// <returns>An object of type T</returns>
        new T ElementAt(int position);
        /// <summary>
        /// Return the Nth element return from a SELECT query, or default(T) if there are fewer than N results.
        /// </summary>
        /// <returns>An object of type T</returns>
        new T ElementAtOrDefault(int position);

        // Grouping/Summing Functions

        

        IQuery<T> To(T target);
        //IEnumerable<U> Map<U>(Func<T, U> filter);

        bool TrySingle(out T item);
        bool TryFirst(out T item);
        bool TryLast(out T item);

        new IEnumerable<T> Enumerate();

        /// <summary>
        /// The target of single-element map operations; typically set using To
        /// </summary>
        T Target { get; set; }
         

    }
}
