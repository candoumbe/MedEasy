using MedEasy.Commands.Specialty;
using MedEasy.Handlers.Core.Commands;
using System;

namespace MedEasy.Handlers.Core.Specialty.Commands
{
    /// <summary>
    /// Interface implemented by runners of <see cref="IDeleteSpecialtyByIdCommand"/>s
    /// </summary>s
    public interface IRunDeleteSpecialtyByIdCommand : IRunCommandAsync<Guid, Guid, IDeleteSpecialtyByIdCommand>
    {

    }

}
