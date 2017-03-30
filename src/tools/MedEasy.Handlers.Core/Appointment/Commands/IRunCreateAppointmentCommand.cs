using MedEasy.Commands.Appointment;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using System;

namespace MedEasy.Handlers.Core.Appointment.Commands
{
    public interface IRunCreateAppointmentCommand : IRunCommandAsync<Guid, CreateAppointmentInfo, AppointmentInfo, ICreateAppointmentCommand>
    {
    }

}
