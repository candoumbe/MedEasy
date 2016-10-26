using MedEasy.DTO;
using MedEasy.Handlers.Queries;
using MedEasy.Objects;
using MedEasy.Queries;
using System;
using System.Collections.Generic;

namespace MedEasy.Handlers.Prescription.Queries
{
    /// <summary>
    /// Base interface for handlers of most recent <see cref="PrescriptionHeaderInfo"/>
    /// </summary>
    public interface IHandleGetMostRecentPrescriptionsInfo : IHandleQueryAsync<Guid, GetMostRecentPrescriptionsInfo, IEnumerable<PrescriptionHeaderInfo>, IQuery<Guid, GetMostRecentPrescriptionsInfo, IEnumerable<PrescriptionHeaderInfo>>>
    {
    }
}
