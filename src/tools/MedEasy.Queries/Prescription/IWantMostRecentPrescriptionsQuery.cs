using MedEasy.DTO;
using Optional;
using System;
using System.Collections.Generic;

namespace MedEasy.Queries.Prescriptions
{
    /// <summary>
    /// Gets most recents <see cref="PrescriptionHeaderInfo"/>s by its <see cref="PatientInfo.Id"/>
    /// </summary>
    public interface IWantMostRecentPrescriptionsQuery : IWantOneResource<Guid, GetMostRecentPrescriptionsInfo, Option<IEnumerable<PrescriptionHeaderInfo>>>
    { 
    }


    
}
