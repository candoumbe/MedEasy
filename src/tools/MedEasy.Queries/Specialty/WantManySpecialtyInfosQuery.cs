using System;
using MedEasy.RestObjects;

namespace MedEasy.Queries.Specialty
{
    /// <summary>
    /// Immutable class to query many <see cref="DTO.SpecialtyInfo"/> by specifying its <see cref="DTO.SpecialtyInfo.Id"/>
    /// </summary>
    public class WantManySpecialtyInfosQuery : IWantPageOfSpecialtyInfoQuery
    {
        public Guid Id { get; }

        public PaginationConfiguration Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantManySpecialtyInfosQuery"/> instance
        /// </summary>
        /// <param name="queryConfig">configuration of the query</param>
        public WantManySpecialtyInfosQuery(PaginationConfiguration queryConfig)
        {
            Id = Guid.NewGuid();
            Data = queryConfig;
        }

        
    }
}