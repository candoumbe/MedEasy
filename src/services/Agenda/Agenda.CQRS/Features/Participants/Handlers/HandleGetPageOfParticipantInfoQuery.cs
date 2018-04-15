using Agenda.CQRS.Features.Participants.Queries;
using Agenda.DTO;
using MedEasy.DAL.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Agenda.CQRS.Features.Participants.Handlers
{
    public class HandleGetPageOfParticipantInfoQuery : IRequestHandler<GetPageOfParticipantInfoQuery, Page<ParticipantInfo>>
    {
        public Task<Page<ParticipantInfo>> Handle(GetPageOfParticipantInfoQuery request, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
