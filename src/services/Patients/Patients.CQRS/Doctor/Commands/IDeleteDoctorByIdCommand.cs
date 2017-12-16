using MedEasy.CQRS.Core.Commands;
using System;

namespace Patients.CQRS.Doctor.Commands
{

    /// <summary>
    /// Shape of a command to delete a <see cref="DTO.DoctorInfo"/> resource by its id
    /// </summary>
    public interface IDeleteDoctorByIdCommand : ICommand<Guid, Guid>
    {
        
    }
}
