using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Queries
{
    /// <summary>
    /// Interfaces for queries that request a "page" of resources.
    /// <para>
    /// A query here stands for a request that is <strong>idempotent</strong>.
    /// </para>
    /// </summary>
    /// <typeparam name="Tkey">Type of the command identifier</typeparam>
    /// <typeparam name="TResource">Type of the resource of the query</typeparam>
    public interface IWantPageOfResources<TQueryId, TResource> : IWantPageOfResources<TQueryId, PaginationConfiguration, TResource>
        where TQueryId : IEquatable<TQueryId>
    {
        
    }


    /// <summary>
    /// Interfaces for queries that request a "page" of resources.
    /// <para>
    /// A query here stands for a request that is <strong>idempotent</strong>.
    /// </para>
    /// </summary>
    /// <typeparam name="Tkey">Type of the command identifier</typeparam>
    /// <typeparam name="TResource">Type of the resource of the query</typeparam>
    /// <typeparam name="TData">Type of data the query will carry.</typeparam>
    public interface IWantPageOfResources<TQueryId, TData, TResource> : IWantResource<TQueryId, TData, IPagedResult<TResource>>
        where TQueryId : IEquatable<TQueryId>
    {

    }
}
