using Measures.DTO;
using Measures.Ids;

using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.RestObjects;

using Optional;

using System;

namespace Measures.CQRS.Queries.BloodPressures
{
    /// <summary>
    /// Query to get a <see cref="MedEasy.DAL.Repositories.Page{T}"/> of <see cref="BloodPressureInfo"/>s given a patient id.
    /// </summary>
    public class GetPageOfBloodPressureInfoByPatientIdQuery : IQuery<Guid, (PatientId patientId, PaginationConfiguration pagination), Option<Page<BloodPressureInfo>>>
    {
        public (PatientId patientId, PaginationConfiguration pagination) Data { get; }

        public Guid Id { get; }

        /// <summary>
        /// Builds a new <see cref="GetPageOfBloodPressureInfoByPatientIdQuery"/> instance.
        /// </summary>
        /// <param name="data">Data needed to get the result</param>
        private GetPageOfBloodPressureInfoByPatientIdQuery((PatientId patientId, PaginationConfiguration pagination) data)
        {
            Id = Guid.NewGuid();
            Data = data;
        }

        /// <summary>
        /// Builds a new <see cref="GetPageOfBloodPressureInfoByPatientIdQuery"/> instance.
        /// </summary>
        /// <param name="id">id of the patient</param>
        /// <param name="pagination">paging configuration</param>
        /// <exception cref="ArgumentNullException">either <paramref name="id"/> or <paramref name="pagination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is <see cref="PatientId.Empty"/>.</exception>
        public GetPageOfBloodPressureInfoByPatientIdQuery(PatientId id, PaginationConfiguration pagination) : this((id, pagination))
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (pagination is null)
            {
                throw new ArgumentNullException(nameof(pagination));
            }

            if (id == PatientId.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }
        }
    }
}