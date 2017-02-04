using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Queries.Appointment
{
    /// <summary>
    /// Request many <see cref="AppointmentInfo"/>s
    /// </summary>
    public interface IWantManyAppointmentInfoQuery : IWantManyResources<Guid, AppointmentInfo>
    {
    }
}
