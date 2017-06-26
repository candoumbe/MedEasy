using MedEasy.Commands.Specialty;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Commands;
using MedEasy.Handlers.Core.Exceptions;
using Optional;
using System;

namespace MedEasy.Handlers.Core.Specialty.Commands
{
    /// <summary>
    /// Interface implemented by runners of <see cref="ICreateSpecialtyCommand"/>s
    /// </summary>
    public interface IRunCreateSpecialtyCommand : IRunCommandAsync<Guid, CreateSpecialtyInfo, Option<SpecialtyInfo, CommandException>, ICreateSpecialtyCommand>
    {

    }

}
