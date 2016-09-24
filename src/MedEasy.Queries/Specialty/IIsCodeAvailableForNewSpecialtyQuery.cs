using System;

namespace MedEasy.Queries.Specialty
{
    /// <summary>
    /// Interface for queries which request to check if a code is available for a new <see cref="DTO.SpecialtyInfo"/>
    /// </summary>
    public interface IIsCodeAvailableForNewSpecialtyQuery : IQuery<Guid, string, bool>
    {
        
    }
}