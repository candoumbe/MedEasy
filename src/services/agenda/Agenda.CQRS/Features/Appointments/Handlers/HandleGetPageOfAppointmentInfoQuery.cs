
using Agenda.CQRS.Features.Appointments.Queries;
using Agenda.DTO;
using Agenda.Objects;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DataFilters;
using MedEasy.Abstractions;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MediatR;
using Optional;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Linq.Expressions.ExpressionExtensions;
namespace Agenda.CQRS.Features.Appointments.Handlers
{
    public class HandleGetPageOfAppointmentInfoQuery : IRequestHandler<GetPageOfAppointmentInfoQuery, Page<AppointmentInfo>>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IExpressionBuilder _expressionBuilder;
        private readonly IDateTimeService _dateTimeService;

        /// <summary>
        /// Builds a new <see cref="HandleGetOneAppointmentInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="unitOfWorkFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <param name="dateTimeService">Service to get <see cref="DateTime"/>s/<see cref="DateTimeOffset"/>s</param>
        public HandleGetPageOfAppointmentInfoQuery(IUnitOfWorkFactory unitOfWorkFactory, IExpressionBuilder expressionBuilder, IDateTimeService dateTimeService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _expressionBuilder = expressionBuilder;
            _dateTimeService = dateTimeService;
        }

        public async Task<Page<AppointmentInfo>> Handle(GetPageOfAppointmentInfoQuery request, CancellationToken ct)
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork())
            {
                Expression<Func<Appointment, AppointmentInfo>> selector = _expressionBuilder.GetMapExpression<Appointment, AppointmentInfo>();
                DateTimeOffset now = _dateTimeService.UtcNowOffset();
                return await uow.Repository<Appointment>()
                    .WhereAsync(
                        selector,
                        (AppointmentInfo x) =>  (x.StartDate <= now && now <= x.EndDate) || now <= x.EndDate,
                        new Sort<AppointmentInfo>(nameof(AppointmentInfo.StartDate)),
                        request.Data.PageSize,
                        request.Data.Page,
                        ct)
                    .ConfigureAwait(false);
            }
        }
    }
}
