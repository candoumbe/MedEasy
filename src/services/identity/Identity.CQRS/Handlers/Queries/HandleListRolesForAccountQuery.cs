using AutoMapper.QueryableExtensions;

using Identity.CQRS.Queries.Roles;
using Identity.DTO;
using Identity.Ids;
using Identity.Objects;

using MedEasy.DAL.Interfaces;

using MediatR;

using Optional;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers.Queries
{
    /// <summary>
    /// Handles queries to list roles for a specific account
    /// </summary>
    public class HandleListRolesForAccountQuery : IRequestHandler<ListRolesForAccountQuery, Option<IEnumerable<RoleInfo>>>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        public HandleListRolesForAccountQuery(IUnitOfWorkFactory unitOfWorkFactory, IExpressionBuilder expressionBuilder)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }

        public async Task<Option<IEnumerable<RoleInfo>>> Handle(ListRolesForAccountQuery request, CancellationToken cancellationToken)
        {
            AccountId accountId = request.Data;

            using IUnitOfWork uow = _unitOfWorkFactory.NewUnitOfWork();

            return await uow.Repository<Account>().SingleOrDefaultAsync(selector: (Account x) => x.Roles.Select(ar => new RoleInfo { Name = ar.Role.Code }),
                                                                        predicate: x => x.Id == accountId,
                                                                        cancellationToken)
                                                  .ConfigureAwait(false);
        }
    }
}
