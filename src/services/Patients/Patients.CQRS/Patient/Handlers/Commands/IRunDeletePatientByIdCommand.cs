using MedEasy.Handlers.Core.Commands;
using Patients.CQRS.Patient.Commands;
using System;

namespace Patients.CQRS.Patient.Handlers.Commands
{
    public interface IRunDeletePatientByIdCommand : IRunCommandAsync<Guid, Guid, IDeletePatientByIdCommand>
    {

    }

}
