using MedEasy.Commands.Patient;
using MedEasy.Handlers.Core.Commands;
using System;

namespace MedEasy.Handlers.Core.Patient.Commands
{
    public interface IRunDeletePatientByIdCommand : IRunCommandAsync<Guid, Guid, IDeletePatientByIdCommand>
    {

    }

}
