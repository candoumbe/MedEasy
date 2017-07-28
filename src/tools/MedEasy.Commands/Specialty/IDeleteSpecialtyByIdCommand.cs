using System;
using MedEasy.Commands;

namespace MedEasy.Commands.Specialty
{
    public interface IDeleteSpecialtyByIdCommand : ICommand<Guid, Guid>
    {
    }
}
