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
        /// Gets all entities paginates
        /// </summary>
        /// <param name="orderBy">The "order by" clauses (cannot be <code>NULL</code>)</param>
        /// <param name="limit">Number of elements in a result page.</param>
        /// <param name="offset">Index of the page to</param>
        /// <returns></returns>
        /// <see cref="PagedResult{T}"/>
        /// <seealso cref="OrderClause{T}"/>
        PagedResult<TEntry> ReadAll(IEnumerable<OrderClause<TEntry>> orderBy, int limit, int offset);


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
        /// <param name="limit">Size of the page.</param>
        /// <param name="offset">Index of the page.</param>
        /// <returns><see cref="PagedResult{T}"/> which holds the result</returns>
        PagedResult<TResult> ReadAll<TResult>(Expression<Func<TEntry, TResult>> selector, IEnumerable<OrderClause<TResult>> orderBy, int limit, int offset);

        //Task<PagedResult<TEntry>> ReadPageAsync(IEnumerable<OrderClause<TEntry>> orderBy, int limit, int offset);
        
        Task<PagedResult<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector, int limit, int offset, IEnumerable<OrderClause<TResult>> orderBy = null);
        
        IEnumerable<TEntry> ReadAll();
        
        IEnumerable<TResult> ReadAll<TResult>(Expression<Func<TEntry, TResult>> selector);
        
        Task<IEnumerable<TEntry>> ReadAllAsync();
        
        Task<IEnumerable<TResult>> ReadAllAsync<TResult>(Expression<Func<TEntry, TResult>> selector);

        //IEnumerable<GroupedResult<TKey, TEntry>>  GroupBy<TKey>(Expression<Func<TEntry, TKey>> keySelector);

        //Task<IEnumerable<GroupedResult<TKey, TEntry>>> GroupByAsync<TKey>(Expression<Func<TEntry, TKey>> keySelector);

        //IEnumerable<GroupedResult<TKey, TResult>> GroupBy<TKey, TResult>( Expression<Func<TEntry, TKey>> keySelector, Expression<Func<TEntry, TResult>> selector);

        //Task<IEnumerable<GroupedResult<TKey, TResult>>> GroupByAsync<TKey, TResult>( Expression<Func<TEntry, TKey>> keySelector, Expression<Func<TEntry, TResult>> selector);

        IEnumerable<TEntry> Where(Expression<Func<TEntry, bool>> predicate);

        IEnumerable<TResult> Where<TKey, TResult>(Expression<Func<TEntry, TResult>> selector,
            Expression<Func<TEntry, bool>> predicate, Expression<Func<TResult, TKey>> keySelector,
            Expression<Func<IGrouping<TKey, TResult>, TResult>> groupBySelector);
        
        IEnumerable<TResult> Where<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate);

        //IEnumerable<TGroupData> Where<TKey, TResult, TGroupData>(Expression<Func<TEntry, bool>> predicate, Expression<Func<TEntry, TKey>> keySelector, Expression<Func<IGrouping<TKey, TEntry>, TGroupData>> groupSelector);
        
        Task<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate);

        
        Task<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate);

        /// <summary>
        /// Asynchronously retrieves entries grouped using the <see cref="keySelector"/>
        /// </summary>
        /// <typeparam name="TKey">Type of the element that will serve to "group" entries together</typeparam>
        /// <typeparam name="TResult">Type of the group result</typeparam>
        /// <param name="keySelector">Selector which defines how results should be grouped</param>
        /// <param name="predicate">Predicate that will be used to filter groups</param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> WhereAsync<TKey, TResult>(Expression<Func<TEntry, bool>> predicate, Expression<Func<TEntry, TKey>> keySelector, Expression<Func<IGrouping<TKey, TEntry>, TResult>> groupSelector);
        
        

        Task<IEnumerable<TEntry>> WhereAsync(
            Expression<Func<TEntry, bool>> predicate, 
            IEnumerable<OrderClause<TEntry>> orderBy = null, 
            IEnumerable<IncludeClause<TEntry>> includedProperties = null);
        
        Task<IEnumerable<TResult>> WhereAsync<TResult>(
            Expression<Func<TEntry, TResult>> selector, 
            Expression<Func<TEntry, bool>> predicate, 
            IEnumerable<OrderClause<TResult>> orderBy = null, 
            IEnumerable<IncludeClause<TEntry>> includedProperties = null);
        
        //Task<IEnumerable<TResult>> WhereAsync<TResult, TKey>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TResult, bool>> predicate, Expression<Func<TResult, TKey>> keySelector, IEnumerable<OrderClause<TResult>> orderBy = null);


        Task<PagedResult<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate,  
            IEnumerable<OrderClause<TEntry>> orderBy, int limit, int offset);
        Task<PagedResult<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TResult>> orderBy, int limit, int offset);

        IEnumerable<TEntry> Where(Expression<Func<TEntry, bool>> predicate, 
            IEnumerable<OrderClause<TEntry>> orderBy = null, 
            IEnumerable<IncludeClause<TEntry>> includedProperties = null);
        IEnumerable<TResult> Where<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TResult>> orderBy = null, IEnumerable<IncludeClause<TEntry>> includedProperties = null);
        
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

        TResult Max<TResult>(Expression<Func<TEntry, TResult>> selector);

        /// <summary>
        /// Asynchronously gets the max value of the selected element
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="selector"></param>
        /// <returns></returns>
        Task<TResult> MaxAsync<TResult>(Expression<Func<TEntry, TResult>> selector);
        
        TProperty Min<TProperty>(Expression<Func<TEntry, TProperty>> selector);
        Task<TResult> MinAsync<TResult>(Expression<Func<TEntry, TResult>> selector);


        /// <summary>
        /// Checks if the current repository contains at least one entry
        /// </summary>
        /// <returns>
        ///     <code>true</code> if the repository contains at least one element or <code>false</code> otherwise
        /// </returns>
        bool Any();

        /// <summary>
        /// Asynchronously checks if the current repository contains at least one entry 
        /// </summary>
        /// <returns>
        ///     <code>true</code> if the repository contains at least one element or <code>false</code> otherwise
        /// </returns>
        Task<bool> AnyAsync();

        /// <summary>
        /// Asynchronously checks if the current repository contains at least one entry 
        /// </summary>
        /// <returns>
        ///     <code>true</code> if the repository contains at least one element or <code>false</code> otherwise
        /// </returns>
        bool Any(Expression<Func<TEntry, bool>> predicate);

        /// <summary>
        /// Asynchronously checks if the current repository contains one entry at least
        /// </summary>
        /// <returns>
        ///     <code>true</code> if the repository contains at least one element or <code>false</code> otherwise
        /// </returns>
        Task<bool> AnyAsync(Expression<Func<TEntry, bool>> predicate);

        /// <summary>
        /// Gets the number of entries in the repository.
        /// </summary>
        /// <returns>
        ///     the number of entries in the repository
        /// </returns>
        int Count();
        
        /// <summary>
        /// Asynchrounously gets the number of entries in the repository.
        /// </summary>
        /// <returns>
        ///     the number of entries in the repository
        /// </returns>
        Task<int> CountAsync();


        /// <summary>
        /// Gets the number of entries in the repository that honor the <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        int Count(Expression<Func<TEntry, bool>> predicate);

        /// <summary>
        /// Asynchrounously gets the number of entries in the repository that honor the <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>
        ///     the number of entries in the repository
        /// </returns>
        Task<int> CountAsync(Expression<Func<TEntry, bool>> predicate);

        /// <summary>
        /// Gets the single entry of the repository
        /// </summary>
        /// <returns>
        ///     the single entry of the repository
        /// </returns>
        
        TEntry Single();
        /// <summary>
        /// Asynchronously gets the single entry of the repository
        /// </summary>
        /// <returns>
        ///     the single entry of the repository
        /// </returns>
        /// <exception cref=""></exception>
        TEntry Single(Expression<Func<TEntry, bool>> predicate);
        
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

        TResult Single<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate);

        Task<TResult> SingleAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate);
        
        TEntry SingleOrDefault();
        Task<TEntry> SingleOrDefaultAsync();
        
        TEntry SingleOrDefault(Expression<Func<TEntry, bool>> predicate);
        Task<TEntry> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate);

        TResult SingleOrDefault<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate);
        Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate);
        
        TEntry First();
        Task<TEntry> FirstAsync();
        
        TEntry FirstOrDefault();
        Task<TEntry> FirstOrDefaultAsync();
        
        TEntry First(Expression<Func<TEntry, bool>> predicate);
        Task<TEntry> FirstAsync(Expression<Func<TEntry, bool>> predicate);
        
        TEntry FirstOrDefault(Expression<Func<TEntry, bool>> predicate);
        Task<TEntry> FirstOrDefaultAsync(Expression<Func<TEntry, bool>> predicate);

        void Delete(Expression<Func<TEntry, bool>> predicate);

        TProperty First<TProperty>(Expression<Func<TEntry, TProperty>> selector, Expression<Func<TEntry, bool>> predicate);
        Task<TProperty> FirstAsync<TProperty>(Expression<Func<TEntry, TProperty>> selector, Expression<Func<TEntry, bool>> predicate);

        TProperty FirstOrDefault<TProperty>(Expression<Func<TEntry, TProperty>> selector, Expression<Func<TEntry, bool>> predicate);
        Task<TProperty> FirstOrDefaultAsync<TProperty>(Expression<Func<TEntry, TProperty>> selector, Expression<Func<TEntry, bool>> predicate);

        

    }
}