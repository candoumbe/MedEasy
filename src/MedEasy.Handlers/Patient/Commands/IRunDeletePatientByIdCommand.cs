using MedEasy.Commands.Patient;
using MedEasy.Handlers.Commands;
using System;

namespace MedEasy.Handlers.Patient.Commands
{
    public interface IRunDeletePatientByIdCommand : IRunCommandAsync<Guid, int, IDeletePatientByIdCommand>
    {

    }

}
