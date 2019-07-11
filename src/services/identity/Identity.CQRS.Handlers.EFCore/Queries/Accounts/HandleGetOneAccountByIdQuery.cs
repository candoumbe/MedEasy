using AutoMapper.QueryableExtensions;
using Identity.CQRS.Queries.Accounts;
using Identity.DTO;
using Identity.Objects;
using MedEasy.DAL.Interfaces;
using MediatR;
using Optional;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers.Queries.Accounts
{
    /// <summary>
    /// Handler for <see cref="GetOneAccountByIdQuery"/> queries.
    /// </summary>
    public class HandleGetOneAccountByIdQuery : IRequestHandler<GetOneAccountByIdQuery, Option<AccountInfo>>
    {
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly IExpressionBuilder _expressionBuilder;

        /// <summary>
        /// Builds a new <see cref="HandleGetOneAccountByIdQuery"/> instance
        /// </summary>
        /// <param name="uowFactory"></param>
        /// <param name="expressionBuilder"></param>
        /// <exception cref="ArgumentNullException">if either <paramref name="uowFactory"/> or 
        /// <paramref name="expressionBuilder"/> is <c>null</c>.</exception>
        public HandleGetOneAccountByIdQuery(IUnitOfWorkFactory uowFactory, IExpressionBuilder expressionBuilder)
        {
            _uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
            _expressionBuilder = expressionBuilder ?? throw new ArgumentNullException(nameof(expressionBuilder));
        }

        public async Task<Option<AccountInfo>> Handle(GetOneAccountByIdQuery query, CancellationToken ct)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                // TODO use a selector
                Option<Account> optionalAccount = await uow.Repository<Account>()
                    .SingleOrDefaultAsync(x => x.Id== query.Data,
                    ct)
                    .ConfigureAwait(false);

                return optionalAccount.Match(
                    some: account => Option.Some(new AccountInfo
                    {
                        Id = account.Id,
                        Email = account.Email,
                        Name = account.Name,
                        Username = account.Username
                    }),
                    none: () => Option.None<AccountInfo>()
                );
            }
        }
    }
}
