using MedEasy.Commands.Doctor;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using System;

namespace MedEasy.Handlers.Core.Doctor.Commands
{
    public interface IRunCreateDoctorCommand : IRunCommandAsync<Guid, CreateDoctorInfo, DoctorInfo, ICreateDoctorCommand>
    {

    }

}
