using MedEasy.DAL.Repositories;
using MedEasy.DTO;
using MedEasy.RestObjects;
using System;

namespace MedEasy.Queries.Patient
{
    /// <summary>
    /// Request many <see cref="PatientInfo"/>s
    /// </summary>
    public interface IWantManyPatientInfoQuery : IWantManyResources<Guid, PatientInfo>
    {
    }
}
