using MedEasy.Commands.Prescription;
using MedEasy.Handlers.Commands;
using System;

namespace MedEasy.Handlers.Prescription.Commands
{
    public interface IRunDeletePrescriptionByIdCommand : IRunCommandAsync<Guid, int, IDeletePrescriptionByIdCommand>
    {

    }

}
