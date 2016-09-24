using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Queries.Specialty
{
    /// <summary>
    /// Request many <see cref="SpecialtyInfo"/>s
    /// </summary>
    public interface IWantManySpecialtyInfoQuery : IWantManyResources<Guid, SpecialtyInfo>
    {
    }
}
