namespace MedEasy.CQRS.Core.Queries
{
    using MedEasy.DAL.Repositories;
    using MedEasy.RestObjects;

    using System;

    /// <summary>
    /// Describes a query to get a page of <see cref="TResult"/> elements.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <typeparam name="TKey">Type of the query identifier.</typeparam>
    /// <typeparam name="TResult">Type of the element of the page</typeparam>
    /// <seealso cref="Page{T}"/>
    public interface IWantPageOf<TKey, TResult> : IQuery<TKey, PaginationConfiguration, Page<TResult>>
        where TKey : IEquatable<TKey>
    {
    }
}
