using System;
using Newtonsoft.Json;

namespace MedEasy.Commands.Doctor
{
    /// <summary>
    /// Command to delete a <see cref=""/> by its id
    /// </summary>
    [JsonObject]
    public class DeleteDoctorByIdCommand : CommandBase<Guid, Guid>, IDeleteDoctorByIdCommand
    {

        /// <summary>
        /// Builds a new <see cref="DeleteDoctorByIdCommand"/> instance.
        /// </summary>
        /// <param name="id">id of the <see cref="DTO.DoctorInfo"/> resource to delete</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is <see cref="Guid.Empty"/></exception>
        public DeleteDoctorByIdCommand(Guid id) : base(Guid.NewGuid(), id)
        {
            if (id == default)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }
        }

        
    }


    
}
