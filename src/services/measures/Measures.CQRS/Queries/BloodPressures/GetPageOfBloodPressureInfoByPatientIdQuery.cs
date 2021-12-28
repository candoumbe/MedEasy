namespace Measures.CQRS.Queries.BloodPressures
{
    using Measures.DTO;
    using Measures.Ids;

    using MedEasy.CQRS.Core.Queries;
    using MedEasy.DAL.Repositories;
    using MedEasy.RestObjects;

    using Optional;

    using System;

    /// <summary>
    /// Query to get a <see cref="Page{T}"/> of <see cref="BloodPressureInfo"/>s given a subject id.
    /// </summary>
    public class GetPageOfBloodPressureInfoBySubjectIdQuery : IQuery<Guid, (SubjectId subjectId, PaginationConfiguration pagination), Option<Page<BloodPressureInfo>>>
    {
        public (SubjectId subjectId, PaginationConfiguration pagination) Data { get; }

        public Guid Id { get; }

        /// <summary>
        /// Builds a new <see cref="GetPageOfBloodPressureInfoBySubjectIdQuery"/> instance.
        /// </summary>
        /// <param name="data">Data needed to get the result</param>
        private GetPageOfBloodPressureInfoBySubjectIdQuery((SubjectId subjectId, PaginationConfiguration pagination) data)
        {
            Id = Guid.NewGuid();
            Data = data;
        }

        /// <summary>
        /// Builds a new <see cref="GetPageOfBloodPressureInfoBySubjectIdQuery"/> instance.
        /// </summary>
        /// <param name="id">id of the patient</param>
        /// <param name="pagination">paging configuration</param>
        /// <exception cref="ArgumentNullException">either <paramref name="id"/> or <paramref name="pagination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is <see cref="SubjectId.Empty"/>.</exception>
        public GetPageOfBloodPressureInfoBySubjectIdQuery(SubjectId id, PaginationConfiguration pagination) : this((id, pagination))
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (pagination is null)
            {
                throw new ArgumentNullException(nameof(pagination));
            }

            if (id == SubjectId.Empty)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }
        }
    }
}