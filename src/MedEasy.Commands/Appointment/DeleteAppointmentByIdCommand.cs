using System;
using Newtonsoft.Json;

namespace MedEasy.Commands.Appointment
{
    /// <summary>
    /// Command to delete a Appointment by its id
    /// </summary>
    [JsonObject]
    public class DeleteAppointmentByIdCommand : CommandBase<Guid, Guid>, IDeleteAppointmentByIdCommand
    {

        /// <summary>
        /// Builds a new <see cref="DeleteAppointmentByIdCommand"/> with a default validator
        /// </summary>
        /// <param name="id">id of the resource to delete</param>
        public DeleteAppointmentByIdCommand(Guid id) : base(Guid.NewGuid(), id)
        {}

        
    }


    
}
