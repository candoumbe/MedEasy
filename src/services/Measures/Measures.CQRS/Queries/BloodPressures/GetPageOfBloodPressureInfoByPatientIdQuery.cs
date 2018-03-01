using Measures.DTO;
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
    public class GetPageOfBloodPressureInfoByPatientIdQuery : IQuery<Guid, (Guid patientId, PaginationConfiguration pagination), Option<Page<BloodPressureInfo>>>
    {
        public (Guid patientId, PaginationConfiguration pagination) Data { get; }

        public Guid Id { get; }



        /// <summary>
        /// Builds a new <see cref="GetPageOfBloodPressureInfoByPatientIdQuery"/> instance.
        /// </summary>
        /// <param name="data">Data needed to get the result</param>
        public GetPageOfBloodPressureInfoByPatientIdQuery((Guid patientId, PaginationConfiguration pagination) data)
        {
            Id = Guid.NewGuid();
            if (Equals(default, data))
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }
            if (data.patientId == default)
            {
                throw new ArgumentOutOfRangeException(nameof(data.patientId));
            }
            if (Equals(data.pagination, default))
            {
                throw new ArgumentOutOfRangeException(nameof(data.pagination));
            }
            Data = data;
        }

        /// <summary>
        /// Builds a new <see cref="GetPageOfBloodPressureInfoByPatientIdQuery"/> instance.
        /// </summary>
        /// <param name="patientId">id of the patient</param>
        /// <param name="pagination">paging configuration</param>
        public GetPageOfBloodPressureInfoByPatientIdQuery(Guid patientId, PaginationConfiguration pagination) : this((patientId, pagination))
        {
        }
    }
}