using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MedEasy.DAL.Repositories
{
    public interface IRepository<TEntry> where TEntry : class
    {
        /// <summary>
        /// <para>
        ///     Reads all entries from the repository.
        /// </para>
        /// <para>
        ///     
        /// </para>
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="selector">The selector.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="page">Index of the page.</param>
        /// <returns><see cref="IPagedResult{T}"/> which holds the result</returns>
        Task<IPagedResult<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector, int pageSize, int page, IEnumerable<OrderClause<TResult>> orderBy = null);
        
        /// <summary>
        /// Asynchronously gets all entries of the repository
        /// </summary>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        Task<IEnumerable<TEntry>> ReadAllAsync();
        
        /// <summary>
        /// Asynchronously gets all entries of the repository after applying <paramref name="selector"/>
        /// </summary>
        /// <typeparam name="TResult">Type of the result</typeparam>
        /// <param name="selector">projection to apply before retrieving the resul</param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> ReadAllAsync<TResult>(Expression<Func<TEntry, TResult>> selector);

        //IEnumerable<GroupedResult<TKey, TEntry>>  GroupBy<TKey>(Expression<Func<TEntry, TKey>> keySelector);

        //Task<IEnumerable<GroupedResult<TKey, TEntry>>> GroupByAsync<TKey>(Expression<Func<TEntry, TKey>> keySelector);

        //IEnumerable<GroupedResult<TKey, TResult>> GroupBy<TKey, TResult>( Expression<Func<TEntry, TKey>> keySelector, Expression<Func<TEntry, TResult>> selector);

        //Task<IEnumerable<GroupedResult<TKey, TResult>>> GroupByAsync<TKey, TResult>( Expression<Func<TEntry, TKey>> keySelector, Expression<Func<TEntry, TResult>> selector);
        
        /// <summary>
        /// Asynchronously gets entries of the repository that satisfied the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">Filter the entries to retrieve</param>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        Task<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate);

        /// <summary>
        /// Asynchronously gets entries of the repository that satisfied the specified <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <param name="predicate"></param>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        Task<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate);

        /// <summary>
        /// Asynchronously retrieves entries grouped using the <see cref="keySelector"/>
        /// </summary>
        /// <typeparam name="TKey">Type of the element that will serve to "group" entries together</typeparam>
        /// <typeparam name="TResult">Type of the group result</typeparam>
        /// <param name="keySelector">Selector which defines how results should be grouped</param>
        /// <param name="predicate">Predicate that will be used to filter groups</param>
        /// <returns><see cref="IEnumerable{T}"/></returns>
        Task<IEnumerable<TResult>> WhereAsync<TKey, TResult>(Expression<Func<TEntry, bool>> predicate, Expression<Func<TEntry, TKey>> keySelector, Expression<Func<IGrouping<TKey, TEntry>, TResult>> groupSelector);


        /// <summary>
        /// Asynchronously gets a <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="predicate">predicate to apply</param>
        /// <param name="orderBy">order to apply to the result</param>
        /// <param name="includedProperties">Properties to include in each object</param>
        /// <returns><see cref="IEnumerable{T}"/> which holds the resu;t</returns>
        Task<IEnumerable<TEntry>> WhereAsync(
            Expression<Func<TEntry, bool>> predicate, 
            IEnumerable<OrderClause<TEntry>> orderBy = null, 
            IEnumerable<IncludeClause<TEntry>> includedProperties = null);
        
        /// <summary>
        /// Asynchronously gets results that satisfied the <paramref name="predicate"/>
        /// </summary>
        /// <remarks>
        /// The <paramref name="orderBy"/> is applied <strong>AFTER</strong> the <paramref name="selector"/> and <paramref name="selector"/>.
        /// </remarks>
        /// <typeparam name="TResult">Type of result's items</typeparam>
        /// <param name="selector"></param>
        /// <param name="predicate"></param>
        /// <param name="orderBy"></param>
        /// <param name="includedProperties"></param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector, 
            Expression<Func<TEntry, bool>> predicate, 
            IEnumerable<OrderClause<TResult>> orderBy = null, 
            IEnumerable<IncludeClause<TEntry>> includedProperties = null);
        
        //Task<IEnumerable<TResult>> WhereAsync<TResult, TKey>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, Expression<Func<TResult, TKey>> keySelector, IEnumerable<OrderClause<TResult>> orderBy = null);

        /// <summary>
        /// Asynchronously gets a <see cref="IPagedResult{T}"/>.
        /// </summary>
        /// <param name="predicate">predicate to apply</param>
        /// <param name="orderBy">order to apply to the result</param>
        /// <param name="pageSize">number of items one page can contain at most</param>
        /// <param name="page">the page of result to get (1 for the page, 2 for the second, ...)</param>
        /// <returns><see cref="IPagedResult{T}"/> which holds the </returns>
        Task<IPagedResult<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate,  
            IEnumerable<OrderClause<TEntry>> orderBy, int pageSize, int page);

        /// <summary>
        /// Asynchronously gets a <see cref="IPagedResult{T}"/> of entries that satisfied the <paramref name="predicate"/>
        /// </summary>
        /// <remarks>
        /// The <paramref name="orderBy"/> is applied <strong>AFTER</strong> both <paramref name="selector"/> and <paramref name="predicate"/> where applied
        /// </remarks>
        /// <typeparam name="TResult">Type of items of the result</typeparam>
        /// <param name="selector">selector to apply</param>
        /// <param name="predicate">filter that entries must satisfied</param>
        /// <param name="orderBy">order to apply</param>
        /// <param name="pageSize">number of items a page can holds at most</param>
        /// <param name="page">the page of result to get.</param>
        /// <returns></returns>
        Task<IPagedResult<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TResult>> orderBy, int pageSize, int page);

        

        ///// <summary>
        ///// Gets an entry by its key(s).
        ///// </summary>
        ///// <param name="keys">Key(s) that uniquely identifies</param>
        ///// <returns>the corresponding entry or<code>NULL</code> if no entry found</returns>
        //TEntry Read(params object[] keys);

        ///// <summary>
        ///// Asynchronously gets an entry by its key(s).
        ///// </summary>
        ///// <param name="keys">Key(s) that uniquely identifies</param>
        ///// <returns>the corresponding entry or<code>NULL</code> if no entry found</returns>
        //Task<TEntry> ReadAsync(params object[] keys);

        /// <summary>
        /// Asynchronously gets the max value of the selected element
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        Task<TResult> MaxAsync<TResult>(Expression<Func<TEntry, TResult>> selector);
        
        /// <summary>
        /// Gets the mininum value after applying the <paramref name="selector"/>
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector">The projection to make before getting the minimum</param>
        /// <returns>The minimum value</returns>
        Task<TResult> MinAsync<TResult>(Expression<Func<TEntry, TResult>> selector);


        
        /// <summary>
        /// Asynchronously checks if the current repository contains at least one entry 
        /// </summary>
        /// <returns>
        ///     <code>true</code> if the repository contains at least one element or <code>false</code> otherwise
        /// </returns>
        Task<bool> AnyAsync();

        
        /// <summary>
        /// Asynchronously checks if the current repository contains one entry at least
        /// </summary>
        /// <returns>
        ///     <code>true</code> if the repository contains at least one element or <code>false</code> otherwise
        /// </returns>
        Task<bool> AnyAsync(Expression<Func<TEntry, bool>> predicate);

        
        /// <summary>
        /// Asynchrounously gets the number of entries in the repository.
        /// </summary>
        /// <returns>
        ///     the number of entries in the repository
        /// </returns>
        Task<int> CountAsync();
        
        /// <summary>
        /// Asynchrounously gets the number of entries in the repository that honor the <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>
        ///     the number of entries in the repository
        /// </returns>
        Task<int> CountAsync(Expression<Func<TEntry, bool>> predicate);

        
        /// <summary>
        /// Asynchronously gets the entry entry of the repository
        /// </summary>
        /// <returns>
        /// the single entry of the repository
        /// </returns>
        Task<TEntry> SingleAsync();

        /// <summary>
        /// Asynchronously gets the sinlge entry that corresponds to the specified <paramref name="predicate"/ >
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<TEntry> SingleAsync(Expression<Func<TEntry, bool>> predicate);
        
        Task<TResult> SingleAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate);

        /// <summary>
        /// Asynchronously gets the single <see cref="TEntry"/> of the repository.
        /// Throws <see cref="ArgumentException"/> if there's more than one entry in the repository
        /// </summary>
        /// <returns><c>null</c> if there no entry in the repository</returns>
        Task<TEntry> SingleOrDefaultAsync();

        /// <summary>
        /// Asynchronously gets the single <see cref="TEntry"/> element of the repository that fullfill the 
        /// <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate">Predicate which should gets one result at most</param>
        /// <returns>the corresponding entry or <code>null</code> if no entry found</returns>
        Task<TEntry> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate);

        Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate);
        
        /// <summary>
        /// Gets the first entry of the repository
        /// </summary>
        /// <returns></returns>
        Task<TEntry> FirstAsync();

        /// <summary>
        /// Gets the first entry of the repository
        /// </summary>
        /// <returns></returns>
        Task<TEntry> FirstOrDefaultAsync();

        /// <summary>
        /// Gets the first entry of the repository that fullfill the specified <paramref name="predicate"/>
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        Task<TEntry> FirstAsync(Expression<Func<TEntry, bool>> predicate);


        Task<TEntry> FirstOrDefaultAsync(Expression<Func<TEntry, bool>> predicate);

        void Delete(Expression<Func<TEntry, bool>> predicate);

        /// <summary>
        /// Gets the first entry
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="selector"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">if the repository is empty or no entry statifies the <paramref name="predicate"/></exception>
        Task<TProperty> FirstAsync<TProperty>(Expression<Func<TEntry, TProperty>> selector, Expression<Func<TEntry, bool>> predicate);


        Task<TProperty> FirstOrDefaultAsync<TProperty>(Expression<Func<TEntry, TProperty>> selector, Expression<Func<TEntry, bool>> predicate);

        /// <summary>
        /// Creates the specified entry
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        TEntry Create(TEntry entry);

        /// <summary>
        /// Create the specified entries
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        IEnumerable<TEntry> Create(IEnumerable<TEntry> entries);


        /// <summary>
        /// Checks if all entries of the repository satisfy the specified <paramref name="predicate"/>
        /// </summary>
        /// <returns><c>true</c> if all entries statifies the <param name="predicate" /> and <c>false</c> otherwise</returns>
        Task<bool> AllAsync(Expression<Func<TEntry, bool>> predicate);


        /// <summary>
        /// Checks if all entries of the repository satisfy the specified <paramref name="predicate"/>
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="predicate">predicate to evaluate all the entries against</param>
        /// <param name="selector">projection before testing the <paramref name="predicate"/></param>
        /// <returns><c>true</c> if all entries statifies the <param name="predicate" /> and <c>false</c> otherwise</returns>
        Task<bool> AllAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate);
    }
}