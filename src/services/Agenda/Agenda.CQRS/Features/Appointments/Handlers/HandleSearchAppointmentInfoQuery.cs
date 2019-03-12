using Agenda.CQRS.Features.Appointments.Queries;
using Agenda.DTO;
using Agenda.DTO.Resources.Search;
using Agenda.Objects;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.Data;
using MedEasy.DTO.Search;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MedEasy.Data.DataFilterLogic;
using static MedEasy.Data.DataFilterOperator;

namespace Agenda.CQRS.Features.Appointments.Handlers
{
    public class HandleSearchAppointmentInfoQuery : IRequestHandler<SearchAppointmentInfoQuery, Page<AppointmentInfo>>
    {
        private readonly IHandleSearchQuery _handleSearchQuery;

        /// <summary>
        /// Builds a new <see cref="HandleGetOneAppointmentInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="handleSearchQuery">Handle search queries</param>
        public HandleSearchAppointmentInfoQuery(IHandleSearchQuery handleSearchQuery)
        {
            _handleSearchQuery = handleSearchQuery;
        }

        public async Task<Page<AppointmentInfo>> Handle(SearchAppointmentInfoQuery request, CancellationToken ct)
        {
            IList<IDataFilter> filters = new List<IDataFilter>();
            SearchAppointmentInfo criteria = request.Data;
            if (criteria.From.HasValue)
            {
                filters.Add(new DataCompositeFilter
                {
                    Logic = Or,
                    Filters = new[]
                    {
                        new DataFilter(field: nameof(Appointment.StartDate), @operator: GreaterThanOrEqual, value : criteria.From.Value),
                        new DataFilter(field: nameof(Appointment.EndDate), @operator: GreaterThanOrEqual, value : criteria.From.Value)
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(criteria.Participant))
            {
                filters.Add(new DataFilter(nameof(Appointment.Participants), @operator: Contains, criteria.Participant));
            }

            SearchQueryInfo<AppointmentInfo> searchQueryInfo = new SearchQueryInfo<AppointmentInfo>
            {
                Page = criteria.Page,
                PageSize = criteria.PageSize
            };
            if (filters.Count > 0)
            {
                searchQueryInfo.Filter = filters.Once()
                    ? filters.Single()
                    : new DataCompositeFilter { Logic = And, Filters = filters };
            }

            return await _handleSearchQuery.Search<Appointment, AppointmentInfo>(new SearchQuery<AppointmentInfo>(searchQueryInfo), ct)
                .ConfigureAwait(false);
        }
    }
}
