using Agenda.CQRS.Features.Appointments.Queries;
using Agenda.DTO;
using Agenda.Objects;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MedEasy.DAL.Interfaces;
using MediatR;
using Optional;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Agenda.CQRS.Features.Appointments.Handlers
{
    public class HandleGetOneAppointmentInfoByIdQuery : IRequestHandler<GetOneAppointmentInfoByIdQuery, Option<AppointmentInfo>>
    {
        private IUnitOfWorkFactory _uowFactory;
        private IMapper _mapper;

        /// <summary>
        /// Builds a <see cref="HandleGetOneAppointmentInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="mapper"></param>
        public HandleGetOneAppointmentInfoByIdQuery(IUnitOfWorkFactory uowFactory, IMapper mapper)
        {
            _uowFactory = uowFactory;
            _mapper = mapper;
        }

        public async Task<Option<AppointmentInfo>> Handle(GetOneAppointmentInfoByIdQuery request, CancellationToken ct)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                Expression<Func<Appointment, AppointmentInfo>> selector = _mapper.ConfigurationProvider.ExpressionBuilder.GetMapExpression<Appointment, AppointmentInfo>();

                return (Option<AppointmentInfo>)await uow.Repository<Appointment>()
                    .SingleOrDefaultAsync(selector, (Appointment x) => x.Id == request.Data, ct)
                    .ConfigureAwait(false);
            }
        }
    }
}
