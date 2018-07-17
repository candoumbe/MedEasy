using AutoMapper.QueryableExtensions;
using Identity.CQRS.Queries.Accounts;
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

namespace Identity.CQRS.Handlers.Queries.Accounts
{
    /// <summary>
    /// Handler for <see cref="GetAccountInfoByIdQuery"/> queries.
    /// </summary>
    public class HandleGetOneAccountInfoByIdQuery : IRequestHandler<GetAccountInfoByIdQuery, Option<AccountInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleGetOneAccountInfoByIdQuery"/> instance
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <exception cref="ArgumentNullException">if either <paramref name="uowFactory"/> or 
        /// <paramref name="expressionBuilder"/> is <c>null</c>.</exception>
        public HandleGetOneAccountInfoByIdQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }

        public async Task<Option<AccountInfo>> Handle(GetAccountInfoByIdQuery query, CancellationToken ct)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                var optionalAccount = await uow.Repository<Account>()
                    .SingleOrDefaultAsync(x => new
                    {
                        x.UserName

                    },
                    x => x.UUID == query.Data,
                    ct)
                    .ConfigureAwait(false);

                return optionalAccount.Match(
                    some: account => Option.Some(new AccountInfo
                    {

                    }),
                    none: () => Option.None<AccountInfo>()
                );
            }
        }
    }
}
