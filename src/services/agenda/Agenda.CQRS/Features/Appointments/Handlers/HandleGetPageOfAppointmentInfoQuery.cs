namespace Agenda.CQRS.Features.Appointments.Handlers
{

    using Agenda.CQRS.Features.Appointments.Queries;
    using Agenda.DTO;
    using Agenda.Objects;

    using AutoMapper.QueryableExtensions;

    using DataFilters;

    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;

    using MediatR;

    using NodaTime;

    using System;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    public class HandleGetPageOfAppointmentInfoQuery : IRequestHandler<GetPageOfAppointmentInfoQuery, Page<AppointmentInfo>>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly IClock _dateTimeService;

        /// <summary>
        /// Builds a new <see cref="HandleGetOneAppointmentInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="unitOfWorkFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <param name="dateTimeService">Service to get <see cref="DateTime"/>s/<see cref="DateTimeOffset"/>s</param>
        public HandleGetPageOfAppointmentInfoQuery(IUnitOfWorkFactory unitOfWorkFactory, IExpressionBuilder expressionBuilder, IClock dateTimeService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _expressionBuilder = expressionBuilder;
            _dateTimeService = dateTimeService;
        }

        /// <inheritdoc/>
        public async Task<Page<AppointmentInfo>> Handle(GetPageOfAppointmentInfoQuery request, CancellationToken cancellationToken)
        {
            using IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork();

            Expression<Func<Appointment, AppointmentInfo>> selector = _expressionBuilder.GetMapExpression<Appointment, AppointmentInfo>();
            Instant utcNow = _dateTimeService.GetCurrentInstant();

            return await uow.Repository<Appointment>()
                            .WhereAsync(selector,
                                        (AppointmentInfo x) => utcNow <= x.EndDate,
                                        new Sort<AppointmentInfo>(nameof(Appointment.StartDate)),
                                        request.Data.PageSize,
                                        request.Data.Page,
                                        cancellationToken)
                            .ConfigureAwait(false);
        }
    }
}
