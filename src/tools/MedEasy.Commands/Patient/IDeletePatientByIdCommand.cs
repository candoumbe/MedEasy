using System;

namespace MedEasy.Commands.Patient
{
    public interface IDeletePatientByIdCommand : ICommand<Guid, Guid>
    {
    }
}
