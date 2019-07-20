﻿using Agenda.CQRS.Features.Appointments.Queries;
using Agenda.DTO;
using Agenda.DTO.Resources.Search;
using Agenda.Objects;
using DataFilters;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.CQRS.Core.Queries;
using MedEasy.DAL.Repositories;
using MedEasy.DTO.Search;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static DataFilters.FilterOperator;
using static DataFilters.FilterLogic;
using System;

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
            IList<IFilter> filters = new List<IFilter>();
            SearchAppointmentInfo searchCriteria = request.Data;
            if (searchCriteria.From.HasValue)
            {
                filters.Add(new CompositeFilter
                {
                    Logic = Or,
                    Filters = new[]
                    {
                        new Filter(field: nameof(Appointment.StartDate), @operator: GreaterThanOrEqual, value : searchCriteria.From.Value),
                        new Filter(field: nameof(Appointment.EndDate), @operator: GreaterThanOrEqual, value : searchCriteria.From.Value)
                    }
                });
            }

            if (searchCriteria.To.HasValue)
            {
                filters.Add(new CompositeFilter
                {
                    Logic = Or,
                    Filters = new[]
                    {
                        new Filter(field: nameof(Appointment.StartDate), @operator: LessThanOrEqualTo, value : searchCriteria.To.Value),
                        new Filter(field: nameof(Appointment.EndDate), @operator: LessThanOrEqualTo, value : searchCriteria.To.Value)
                    }
                });
            }

            SearchQueryInfo<AppointmentInfo> searchQueryInfo = new SearchQueryInfo<AppointmentInfo>
            {
                Page = searchCriteria.Page,
                PageSize = searchCriteria.PageSize,
                Sort = searchCriteria.Sort?.ToSort<AppointmentInfo>() ?? new Sort<AppointmentInfo>(nameof(AppointmentInfo.StartDate))
            };
            if (filters.Count > 0)
            {
                searchQueryInfo.Filter = filters.Once()
                    ? filters.Single()
                    : new CompositeFilter { Logic = And, Filters = filters };
            }

            return await _handleSearchQuery.Search<Appointment, AppointmentInfo>(new SearchQuery<AppointmentInfo>(searchQueryInfo), ct)
                .ConfigureAwait(false);
        }
    }
}