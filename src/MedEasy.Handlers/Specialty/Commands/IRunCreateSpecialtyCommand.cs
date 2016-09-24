using MedEasy.Commands.Specialty;
using MedEasy.DTO;
using MedEasy.Handlers.Commands;
using System;

namespace MedEasy.Handlers.Specialty.Commands
{
    /// <summary>
    /// Interface implemented by runners of <see cref="ICreateSpecialtyCommand"/>s
    /// </summary>
    public interface IRunCreateSpecialtyCommand : IRunCommandAsync<Guid, CreateSpecialtyInfo, SpecialtyInfo, ICreateSpecialtyCommand>
    {

    }

}
