using Agenda.CQRS.Features.Participants.Queries;
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

namespace Agenda.CQRS.Features.Participants.Handlers
{
    /// <summary>
    /// Handles <see cref="GetOneAttendeeInfoByIdQuery"/> queries.
    /// </summary>
    public class HandleGetOneParticipantInfoByIdQuery : IRequestHandler<GetOneAttendeeInfoByIdQuery, Option<AttendeeInfo>>
    {
        private IUnitOfWorkFactory _uowFactory;
        private IMapper _mapper;

        /// <summary>
        /// Builds a <see cref="HandleGetOneParticipantInfoByIdQuery"/> instance.
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="mapper"></param>
        public HandleGetOneParticipantInfoByIdQuery(IUnitOfWorkFactory uowFactory, IMapper mapper)
        {
            _uowFactory = uowFactory;
            _mapper = mapper;
        }

        public async Task<Option<AttendeeInfo>> Handle(GetOneAttendeeInfoByIdQuery request, CancellationToken cancellationToken)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                Expression<Func<Attendee, AttendeeInfo>> selector = _mapper.ConfigurationProvider.ExpressionBuilder.GetMapExpression<Attendee, AttendeeInfo>();

                return await uow.Repository<Attendee>()
                    .SingleOrDefaultAsync(selector, x => x.Id == request.Data, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
