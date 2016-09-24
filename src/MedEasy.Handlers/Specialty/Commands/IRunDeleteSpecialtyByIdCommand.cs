using MedEasy.Commands.Specialty;
using MedEasy.DTO;
using MedEasy.Handlers.Commands;
using System;

namespace MedEasy.Handlers.Specialty.Commands
{
    /// <summary>
    /// Interface implemented by runners of <see cref="IDeleteSpecialtyByIdCommand"/>s
    /// </summary>s
    public interface IRunDeleteSpecialtyByIdCommand : IRunCommandAsync<Guid, int, IDeleteSpecialtyByIdCommand>
    {

    }

}
