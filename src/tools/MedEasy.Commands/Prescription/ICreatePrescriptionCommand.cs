using System;
using MedEasy.DTO;

namespace MedEasy.Commands.Prescription
{
    public interface ICreatePrescriptionCommand : ICommand<Guid, CreatePrescriptionInfo>
    {    }
}