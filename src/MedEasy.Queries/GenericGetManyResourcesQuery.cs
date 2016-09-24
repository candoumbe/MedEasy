using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Queries
{
    /// <summary>
    /// Base class for creating queries that request many resources
    /// </summary>
    /// <typeparam name="TResource"></typeparam>
    public class GenericGetManyResourcesQuery<TResource> : IWantManyResources<Guid, TResource>
    {
        /// <summary>
        /// Query's identifier
        /// </summary>
        public Guid Id { get; }

        public GenericGetQuery Data { get; }

        /// <summary>
        /// Builds a new <see cref="GenericGetManyResourcesQuery{TResource}"/> instance.
        /// </summary>
        /// <param name="queryConfig"></param>
        public GenericGetManyResourcesQuery(GenericGetQuery queryConfig)
        {
            Id = Guid.NewGuid();
            Data = queryConfig;
        }

            
    }
}
