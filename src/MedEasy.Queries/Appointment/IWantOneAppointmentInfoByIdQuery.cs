using MedEasy.DTO;
using System;

namespace MedEasy.Queries.Appointment
{
    /// <summary>
    /// Gets a <see cref="AppointmentInfo"/> by its <see cref="AppointmentInfo.Id"/>
    /// </summary>
    public interface IWantOneAppointmentInfoByIdQuery : IWantOneResource<Guid, Guid, AppointmentInfo>
    { 
    }


    
}
