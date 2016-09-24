using MedEasy.Handlers.Queries;
using MedEasy.Queries.Specialty;
using System;

namespace MedEasy.Handlers.Specialty.Queries
{
    /// <summary>
    /// Interface for handlers that can check if a <c>code</c> can safely be used when creating a new <see cref="DTO.SpecialtyInfo"/>
    /// </summary>
    public interface IHandleIsCodeAvailableForNewSpecialtyQuery : IHandleQueryAsync<Guid,string, bool, IIsCodeAvailableForNewSpecialtyQuery>
    {
    }
}
