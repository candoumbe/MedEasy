using System;

namespace MedEasy.Commands.Appointment
{
    public interface IDeleteAppointmentByIdCommand : ICommand<Guid, Guid>
    {
    }
}
