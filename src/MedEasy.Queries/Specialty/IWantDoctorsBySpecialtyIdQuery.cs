using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Queries.Specialty
{
    /// <summary>
    /// Request many <see cref="DoctorInfo"/> that have the specialty with the specified id
    /// </summary>
    public interface IFindDoctorsBySpecialtyIdQuery : IQuery<Guid, FindDoctorsBySpecialtyIdQueryArgs, IPagedResult<DoctorInfo>>
    {
    }
}
