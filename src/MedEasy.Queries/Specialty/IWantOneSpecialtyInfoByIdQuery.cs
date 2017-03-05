using MedEasy.DTO;
using System;

namespace MedEasy.Queries.Specialty
{
    /// <summary>
    /// Gets a <see cref="SpecialtyInfo"/> by its <see cref="SpecialtyInfo.Id"/>
    /// </summary>
    public interface IWantOneSpecialtyInfoByIdQuery : IWantOneResource<Guid, Guid, SpecialtyInfo>
    { 
    }


    
}
