using System;

namespace MedEasy.Commands.Prescription
{
    public interface IDeletePrescriptionByIdCommand : ICommand<Guid, Guid>
    {
    }
}
