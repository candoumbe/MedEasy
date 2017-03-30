using System;
using MedEasy.RestObjects;

namespace MedEasy.Queries.Appointment
{
    /// <summary>
    /// Immutable class to query many <see cref="DTO.AppointmentInfo"/> by specifying its <see cref="DTO.AppointmentInfo.Id"/>
    /// </summary>
    public class WantManyAppointmentInfosQuery : IWantManyAppointmentInfoQuery
    {
        public Guid Id { get; }

        public PaginationConfiguration Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantManyAppointmentInfosQuery"/> instance
        /// </summary>
        /// <param name="queryConfig">configuration of the query</param>
        public WantManyAppointmentInfosQuery(PaginationConfiguration queryConfig)
        {
            Id = Guid.NewGuid();
            Data = queryConfig;
        }

        
    }
}