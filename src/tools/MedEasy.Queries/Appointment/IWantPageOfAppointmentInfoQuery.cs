using MedEasy.DTO;
using System;

namespace MedEasy.Queries.Appointment
{
    /// <summary>
    /// Request many <see cref="AppointmentInfo"/>s
    /// </summary>
    public interface IWantPageOfAppointmentInfosQuery : IWantPageOfResources<Guid, AppointmentInfo>
    {
    }
}
