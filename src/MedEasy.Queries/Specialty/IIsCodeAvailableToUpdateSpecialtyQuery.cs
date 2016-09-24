using System;

namespace MedEasy.Queries.Specialty
{
    /// <summary>
    /// Query to check if a code is available for a new <see cref="SpecialtyInfo"/>
    /// </summary>
    public interface IIsCodeAvailableToUpdateSpecialtyQuery : IQuery<Guid, string, bool>
    {
        
    }
}