using System;

namespace MedEasy.Commands.Doctor
{
    public interface IDeleteDoctorByIdCommand : ICommand<Guid, int>
    {
    }
}
