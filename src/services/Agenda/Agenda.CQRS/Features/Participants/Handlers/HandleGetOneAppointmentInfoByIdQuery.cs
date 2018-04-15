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
    /// Handles <see cref="GetOneParticipantInfoByIdQuery"/> queries.
    /// </summary>
    public class HandleGetOneParticipantInfoByIdQuery : IRequestHandler<GetOneParticipantInfoByIdQuery, Option<ParticipantInfo>>
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

        public async Task<Option<ParticipantInfo>> Handle(GetOneParticipantInfoByIdQuery request, CancellationToken ct)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                Expression<Func<Participant, ParticipantInfo>> selector = _mapper.ConfigurationProvider.ExpressionBuilder.GetMapExpression<Participant, ParticipantInfo>();

                Option<ParticipantInfo> optionalParticipant = await uow.Repository<Participant>()
                    .SingleOrDefaultAsync(selector, x => x.Id == request.Data, ct)
                    .ConfigureAwait(false);

                return optionalParticipant;
                
            }

            
        }
    }
}
