using Identity.CQRS.Commands;
using Identity.Objects;
using MedEasy.Abstractions;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.Interfaces;
using MediatR;
using Optional;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.CQRS.Handlers.Commands
{
    public class HandleInvalidateAccessTokenByUsernameCommand : IRequestHandler<InvalidateAccessTokenByUsernameCommand, InvalidateAccessCommandResult>
    {
        private readonly IDateTimeService _datetimeService;
        private readonly IUnitOfWorkFactory _uowFactory;

        /// <summary>
        /// Builds a new <see cref="HandleInvalidateAccessTokenByUsernameCommand"/> instance
        /// </summary>
        /// <param name="datetimeService"></param>
        /// <param name="uowFactory"></param>
        public HandleInvalidateAccessTokenByUsernameCommand(IDateTimeService datetimeService, IUnitOfWorkFactory uowFactory)
        {
            _datetimeService = datetimeService;
            _uowFactory = uowFactory;
        }

        public async Task<InvalidateAccessCommandResult> Handle(InvalidateAccessTokenByUsernameCommand cmd, CancellationToken ct)
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                Option<Account> optionalAccount = await uow.Repository<Account>().SingleOrDefaultAsync(x => x.UserName == cmd.Data, ct)
                    .ConfigureAwait(false);

                return await optionalAccount.Match(
                   some: async account =>
                   {
                        account.RefreshToken = null;
                        await uow.SaveChangesAsync(ct)
                            .ConfigureAwait(false);

                       return InvalidateAccessCommandResult.Done;
                   },
                   none : () => Task.FromResult(InvalidateAccessCommandResult.Failed_NotFound)
                ).ConfigureAwait(false);
            }
        }
    }
}
