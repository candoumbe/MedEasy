using MedEasy.Commands.Prescription;
using MedEasy.DTO;
using MedEasy.Handlers.Commands;
using System;

namespace MedEasy.Handlers.Prescription.Commands
{
    public interface IRunCreatePrescriptionCommand : IRunCommandAsync<Guid, CreatePrescriptionInfo, PrescriptionInfo, ICreatePrescriptionCommand>
    {

    }

}
