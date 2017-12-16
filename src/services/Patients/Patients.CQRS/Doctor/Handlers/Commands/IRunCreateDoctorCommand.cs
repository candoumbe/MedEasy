using Doctors.CQRS.Doctor.Commands;
using MedEasy.Handlers.Core.Commands;
using Patients.DTO;
using System;

namespace Patients.CQRS.Doctor.Handlers.Commands
{
    public interface IRunCreateDoctorCommand : IRunCommandAsync<Guid, CreateDoctorInfo, DoctorInfo, ICreateDoctorCommand>
    {

    }

}
