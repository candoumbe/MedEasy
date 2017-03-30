using System;
using MedEasy.RestObjects;

namespace MedEasy.Queries.Doctor
{
    /// <summary>
    /// Immutable class to query many <see cref="DTO.DoctorInfo"/> by specifying its <see cref="DTO.DoctorInfo.Id"/>
    /// </summary>
    public class WantManyDoctorInfosQuery : IWantManyDoctorInfoQuery
    {
        public Guid Id { get; }

        public PaginationConfiguration Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantManyDoctorInfosQuery"/> instance
        /// </summary>
        /// <param name="queryConfig">configuration of the query</param>
        public WantManyDoctorInfosQuery(PaginationConfiguration queryConfig)
        {
            Id = Guid.NewGuid();
            Data = queryConfig;
        }

        
    }
}