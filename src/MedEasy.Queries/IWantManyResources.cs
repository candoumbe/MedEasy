using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Queries
{
    /// <summary>
    /// Interfaces for queries that request many resources
    /// <para>
    /// A query here stands for a request that is <strong>idempotent</strong>.
    /// </para>
    /// </summary>
    /// <typeparam name="Tkey">Type of the command identifier</typeparam>
    /// <typeparam name="TResource">Type of the resource of the query</typeparam>
    public interface IWantManyResources<TQueryId, TResource> : IQuery<TQueryId, PaginationConfiguration, IPagedResult<TResource>>
        where TQueryId : IEquatable<TQueryId>
    {
        
    }
}
