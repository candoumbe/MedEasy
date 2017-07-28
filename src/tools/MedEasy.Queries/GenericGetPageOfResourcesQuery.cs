﻿using MedEasy.RestObjects;
using Newtonsoft.Json;
using System;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.Queries
{
    /// <summary>
    /// Base class for creating queries that request many resources
    /// </summary>
    /// <typeparam name="TResource">Type of resources to get</typeparam>
    [JsonObject]
    public class GenericGetPageOfResourcesQuery<TResource> : IWantPageOfResources<Guid, TResource>
    {
        /// <summary>
        /// Query's identifier
        /// </summary>
        public Guid Id { get; }

        public PaginationConfiguration Data { get; }

        /// <summary>
        /// Builds a new <see cref="GenericGetPageOfResourcesQuery{TResource}"/> instance.
        /// </summary>
        /// <param name="queryConfig"></param>
        public GenericGetPageOfResourcesQuery(PaginationConfiguration queryConfig)
        {
            Id = Guid.NewGuid();
            Data = queryConfig;
        }

        public override string ToString() => SerializeObject(this);
    }
}