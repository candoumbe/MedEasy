using MedEasy.RestObjects;
using Newtonsoft.Json;
using System;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.Queries
{
    /// <summary>
    /// Base class for creating queries that request many resources
    /// </summary>
    /// <typeparam name="TResource">Type of the resource to get</typeparam>
    [JsonObject]
    public class GenericGetManyResourcesQuery<TResource> : IWantManyResources<Guid, TResource>
    {
        /// <summary>
        /// Query's identifier
        /// </summary>
        public Guid Id { get; }

        public PaginationConfiguration Data { get; }

        /// <summary>
        /// Builds a new <see cref="GenericGetManyResourcesQuery{TResource}"/> instance.
        /// </summary>
        /// <param name="queryConfig"></param>
        public GenericGetManyResourcesQuery(PaginationConfiguration queryConfig)
        {
            Id = Guid.NewGuid();
            Data = queryConfig;
        }

        public override string ToString() => SerializeObject(this);
    }
}
