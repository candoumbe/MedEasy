using MedEasy.Commands.Prescription;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Exceptions;
using Optional;
using System;

namespace MedEasy.Handlers.Core.Prescription.Commands
{
    public interface IRunCreatePrescriptionCommand : IRunCommandAsync<Guid, CreatePrescriptionInfo, PrescriptionInfo, ICreatePrescriptionCommand>
    {

    }

}
