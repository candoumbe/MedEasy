using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using System;

namespace MedEasy.CQRS.Core.Queries
{
    /// <summary>
    /// Interfaces for queries that request a "page" of resources.
    /// <para>
    /// A query here stands for a request that is <strong>idempotent</strong>.
    /// </para>
    /// </summary>
    /// <typeparam name="TQueryId">Type of the command identifier</typeparam>
    /// <typeparam name="TResource">Type of the resource of the query</typeparam>
    public interface IWantPage<TQueryId, TResource> : IWantPage<TQueryId, PaginationConfiguration, TResource>
        where TQueryId : IEquatable<TQueryId>
    {

    }

    /// <summary>
    /// Interfaces for queries that request a "page" of resources.
    /// <para>
    /// A query here stands for a request that is <strong>idempotent</strong>.
    /// </para>
    /// </summary>
    /// <typeparam name="TQueryId">Type of the command identifier</typeparam>
    /// <typeparam name="TResource">Type of the resource of the query</typeparam>
    /// <typeparam name="TData">Type of data the query will carry.</typeparam>
    public interface IWantPage<TQueryId, TData, TResource> : IWant<TQueryId, TData, Page<TResource>>
        where TQueryId : IEquatable<TQueryId>
    {

    }
}
