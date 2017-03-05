using System;

namespace MedEasy.Queries.Appointment
{
    /// <summary>
    /// Immutable class to query <see cref="DTO.AppointmentInfo"/> by specifying its <see cref="DTO.AppointmentInfo.Id"/>
    /// </summary>
    public class WantOneAppointmentInfoByIdQuery : IWantOneAppointmentInfoByIdQuery
    {
        public Guid Id { get; }

        public Guid Data { get; }

        /// <summary>
        /// Builds a new <see cref="WantOneAppointmentInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="id">of the <see cref="AppointmentInfo"/> to retrieve</param>
        public WantOneAppointmentInfoByIdQuery(Guid id)
        {
            Id = Guid.NewGuid();
            Data = id;
        }

        
    }
}