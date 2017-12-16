using MedEasy.CQRS.Core.Commands;
using Patients.DTO;
using System;

namespace Doctors.CQRS.Doctor.Commands
{
    /// <summary>
    /// Basic shape of a command to create a new <see cref="DoctorInfo"/> resource.
    /// </summary>
    public interface ICreateDoctorCommand : ICommand<Guid, CreateDoctorInfo, DoctorInfo>
    {
        
    }
}
