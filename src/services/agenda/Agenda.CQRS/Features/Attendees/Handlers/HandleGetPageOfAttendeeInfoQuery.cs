namespace Agenda.CQRS.Features.Participants.Handlers
{
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

    /// <summary>
    /// Handles <see cref="GetPageOfAttendeeInfoQuery"/> queries.
    /// </summary>
    public class HandleGetPageOfAttendeeInfoQuery : IRequestHandler<GetPageOfAttendeeInfoQuery, Page<AttendeeInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleGetPageOfAttendeeInfoQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        public HandleGetPageOfAttendeeInfoQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
        }

        ///<inheritdoc/>
        public async Task<Page<AttendeeInfo>> Handle(GetPageOfAttendeeInfoQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<Attendee, AttendeeInfo>> selector = _expressionBuilder.GetMapExpression<Attendee, AttendeeInfo>();
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();

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
