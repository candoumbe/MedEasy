using MedEasy.Commands.Doctor;
using MedEasy.DTO;
using MedEasy.Handlers.Commands;
using System;

namespace MedEasy.Handlers.Doctor.Commands
{
    public interface IRunCreateDoctorCommand : IRunCommandAsync<Guid, CreateDoctorInfo, DoctorInfo, ICreateDoctorCommand>
    {

    }

}
