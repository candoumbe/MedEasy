using Agenda.CQRS.Features.Participants.Queries;
using Agenda.DTO;
using Agenda.Objects;

using AutoMapper.QueryableExtensions;

using DataFilters;

using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;

using MediatR;

using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Agenda.CQRS.Features.Participants.Handlers
{
    public class HandleGetPageOfAttendeeInfoQuery : IRequestHandler<GetPageOfAttendeeInfoQuery, Page<AttendeeInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        public HandleGetPageOfAttendeeInfoQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
        }

        public async Task<Page<AttendeeInfo>> Handle(GetPageOfAttendeeInfoQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<Attendee, AttendeeInfo>> selector = _expressionBuilder.GetMapExpression<Attendee, AttendeeInfo>();
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                return await uow.Repository<Attendee>()
                    .ReadPageAsync(selector,
                        pageSize: request.Data.PageSize,
                        request.Data.Page,
                        orderBy: new Sort<AttendeeInfo>(nameof(AttendeeInfo.Name)),
                        ct: default)
                    .ConfigureAwait(false);
            }
        }
    }
}
