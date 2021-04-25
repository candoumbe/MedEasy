using Measures.DTO;
using Measures.Objects;

using MedEasy.CQRS.Core.Handlers;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;

using MediatR;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Measures.CQRS.Handlers.Patients
{
    /// <summary>
    /// Handles <see cref="SearchQuery{PatientInfo}"/> instances.
    /// </summary>
    public class HandleSearchPatientInfosQuery : IRequestHandler<SearchQuery<PatientInfo>, Page<PatientInfo>>
    {
        private readonly IHandleSearchQuery _handleSearchQuery;

        /// <summary>
        /// Builds a new <see cref="HandleSearchPatientInfosQuery"/> instance.
        /// </summary>
        /// <param name="handleSearchQuery">handler for all search queries.</param>
        public HandleSearchPatientInfosQuery(IHandleSearchQuery handleSearchQuery)
        {
            _handleSearchQuery = handleSearchQuery ?? throw new ArgumentNullException(nameof(handleSearchQuery));
        }

        public Task<Page<PatientInfo>> Handle(SearchQuery<PatientInfo> request, CancellationToken cancellationToken) =>
            _handleSearchQuery.Search<Patient, PatientInfo>(request, cancellationToken);
    }
}
