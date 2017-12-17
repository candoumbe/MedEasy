using MedEasy.CQRS.Core.Commands;
using Patients.CQRS.Doctor.Commands;
using System;

namespace Patients.CQRS.Doctor.Handlers.Commands
{
    public interface IRunDeleteDoctorInfoByIdCommand : IRunCommandAsync<Guid, Guid, IDeleteDoctorByIdCommand>
    {

    }

}
