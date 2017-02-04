using MedEasy.DTO;
using System;

namespace MedEasy.Commands.Appointment
{
    public interface ICreateAppointmentCommand : ICommand<Guid, CreateAppointmentInfo>
    {
    }
}
