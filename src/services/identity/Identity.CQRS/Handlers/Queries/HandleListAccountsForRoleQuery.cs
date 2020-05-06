using AutoMapper.QueryableExtensions;
using Identity.CQRS.Queries.Roles;
using Identity.DTO;
using Identity.Objects;
using MedEasy.DAL.Interfaces;
using MediatR;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers.Queries
{
    /// <summary>
    /// Handles queries to list accounts for a specific role
    /// </summary>
    public class HandleListAccountsForRoleQuery : IRequestHandler<ListAccountsForRoleQuery, Option<IEnumerable<AccountInfo>>>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        public HandleListAccountsForRoleQuery(IUnitOfWorkFactory unitOfWorkFactory, IExpressionBuilder expressionBuilder)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }

        public async Task<Option<IEnumerable<AccountInfo>>> Handle(ListAccountsForRoleQuery request, CancellationToken cancellationToken)
        {
            Guid roleId = request.Data;

            using IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork();

            return await uow.Repository<Role>().SingleOrDefaultAsync(selector: x => x.Accounts.Select(ar => new AccountInfo { Id = ar.AccountId, Name = ar.Account.Name }),
                                                                     predicate: (Role x) => x.Id == roleId,
                                                                     cancellationToken)
                                                      .ConfigureAwait(false);
        }
    }
}
