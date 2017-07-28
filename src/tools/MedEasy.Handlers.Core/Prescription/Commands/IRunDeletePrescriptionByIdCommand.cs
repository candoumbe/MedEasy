using MedEasy.Commands.Prescription;
using MedEasy.Handlers.Core.Commands;
using System;

namespace MedEasy.Handlers.Core.Prescription.Commands
{
    public interface IRunDeletePrescriptionByIdCommand : IRunCommandAsync<Guid, Guid, IDeletePrescriptionByIdCommand>
    {

    }

}
