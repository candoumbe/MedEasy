using Agenda.CQRS.Features.Participants.Queries;
using Agenda.DTO;
using Agenda.Objects;
using AutoMapper.QueryableExtensions;
using DataFilters.Expressions;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MediatR;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Agenda.CQRS.Features.Participants.Handlers
{
    public class HandleGetPageOfParticipantInfoQuery : IRequestHandler<GetPageOfParticipantInfoQuery, Page<ParticipantInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        public HandleGetPageOfParticipantInfoQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory;
            _expressionBuilder = expressionBuilder;
        }

        public async Task<Page<ParticipantInfo>> Handle(GetPageOfParticipantInfoQuery request, CancellationToken cancellationToken)
        {
            Expression<Func<Participant, ParticipantInfo>> selector = _expressionBuilder.GetMapExpression<Participant, ParticipantInfo>();
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                return await uow.Repository<Participant>()
                    .ReadPageAsync(selector,
                        pageSize: request.Data.PageSize,
                        request.Data.Page,
                        orderBy: new[] {OrderClause<ParticipantInfo>.Create(x => x.Name)},
                        ct: default)
                    .ConfigureAwait(false);
            }
        }
    }
}
