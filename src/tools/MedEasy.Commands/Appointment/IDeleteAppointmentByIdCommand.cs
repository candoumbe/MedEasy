using System;

namespace MedEasy.Commands.Appointment
{
    /// <summary>
    /// Command to delete an <see cref="Objects.Appointment"/> by its identifier
    /// </summary>
    public interface IDeleteAppointmentByIdCommand : ICommand<Guid, Guid>
    {
    }
}
