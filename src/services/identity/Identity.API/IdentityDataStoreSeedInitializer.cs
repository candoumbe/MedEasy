namespace Identity.API
{
    using Identity.CQRS.Commands.Accounts;
    using Identity.CQRS.Queries.Accounts;
    using Identity.DataStores;
    using Identity.DTO;
    using Identity.Ids;
    using Identity.Objects;

    using MedEasy.CQRS.Core.Commands.Results;
    using MedEasy.DataStores.Core;

    using MediatR;

    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Optional;

    /// <summary>
    /// Seeds the datastore
    /// </summary>
    public class IdentityDataStoreSeedInitializer : DataStoreSeedInitializerAsync<IdentityContext>
    {
        /// <summary>
        /// Builds a new <see cref="IdentityDataStoreSeedInitializer"/> instance.
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        /// <param name="logger"></param>
        /// <param name="dataStore"></param>
        /// <param name="options"></param>
        /// <param name="mediator"></param>
        public IdentityDataStoreSeedInitializer(IHostEnvironment hostingEnvironment,
                                                ILogger<DataStoreSeedInitializerAsync<IdentityContext>> logger,
                                                IdentityContext dataStore,
                                                IOptionsSnapshot<AccountOptions> options,
                                                IMediator mediator)
            : base(hostingEnvironment,
                   logger,
                   dataStore,
                   async _ =>
                   {
                       SystemAccount[] accounts = options.Value.Accounts;
                       logger.LogInformation("{AccountsCount} account(s) to create/update", accounts.Length);
                       logger.LogInformation("Accounts : {@Accounts}", accounts.Select(account => new { account.Username, account.Email }).ToArray());

                       await accounts.ForEachAsync(async account =>
                       {
                           logger.LogInformation("Creating account for {Username}", account.Username);
                           NewAccountInfo accountInfo = new()
                           {
                               Id = AccountId.New(),
                               Username = account.Username,
                               Email = account.Email,
                               Password = account.Password,
                               ConfirmPassword = account.Password,
                               Name = account.Email
                           };
                           CreateAccountInfoCommand command = new(accountInfo);

                           Option<AccountInfo, CreateCommandFailure> result = await mediator.Send(command);

                           result.Match(accountCreated => logger.LogInformation("Created account <{AccountId}> for {Username} created successfully", accountCreated.Id.Value, accountCreated.Username),
                                        async failure =>
                                        {
                                            switch (failure)
                                            {
                                                case CreateCommandFailure.Conflict:
                                                    {
                                                        GetOneAccountInfoByUsernameQuery request = new(command.Data.Username);
                                                        Option<AccountInfo> accountMayExist = await mediator.Send(request)
                                                                                                            .ConfigureAwait(false);

                                                        accountMayExist.MatchSome(async existingAccount =>
                                                        {
                                                            DeleteAccountInfoByIdCommand deleteAccountCmd = new(existingAccount.Id);
                                                            await mediator.Send(request.Data).ConfigureAwait(false);
                                                            await mediator.Send(command).ConfigureAwait(false);
                                                        });
                                                    }
                                                    break;
                                                default:
                                                    logger.LogCritical("Could not create account for {Username} (Reason : {FailureReason})", account.Username, failure);
                                                    break;
                                            }
                                        });
                       });
                   })
        {
        }
    }
}
