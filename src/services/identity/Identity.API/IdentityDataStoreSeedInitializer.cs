namespace Identity.API;

using Identity.CQRS.Commands.Accounts;
using Identity.CQRS.Queries.Accounts;
using Identity.DataStores;
using Identity.DTO;
using Identity.Ids;
using Identity.Objects;
using Identity.ValueObjects;

using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DataStores.Core;

using MediatR;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Optional;

using System.Threading.Tasks;

/// <summary>
/// Seeds the datastore
/// </summary>
public class IdentityDataStoreSeedInitializer : DataStoreSeedInitializerAsync<IdentityDataStore>
{
    

    /// <summary>
    /// Builds a new <see cref="IdentityDataStoreSeedInitializer"/> instance.
    /// </summary>
    /// <param name="hostingEnvironment"></param>
    /// <param name="logger"></param>
    /// <param name="dataStoreFactory"></param>
    /// <param name="options"></param>
    /// <param name="mediator"></param>
    public IdentityDataStoreSeedInitializer(IHostEnvironment hostingEnvironment,
                                            ILogger<DataStoreSeedInitializerAsync<IdentityDataStore>> logger,
                                            Func<IdentityDataStore> dataStoreFactory,
                                            IOptionsSnapshot<AccountOptions> options,
                                            IMediator mediator)
        : base(hostingEnvironment,
               logger,
               dataStoreFactory,
               async _ =>
               {
                   SystemAccount[] accounts = options.Value.Accounts;
                   logger.LogInformation("{AccountsCount} account(s) to create/update", accounts.Length);
                   logger.LogInformation("Accounts : {@Accounts}", accounts);

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
                           Name = account.Username.Value
                       };
                       CreateAccountInfoCommand command = new(accountInfo);

                       Option<AccountInfo, CreateCommandFailure> result = await mediator.Send(command);

                       result.Match(accountCreated => logger.LogInformation("Account <{AccountId}> for {Username} created successfully", accountCreated.Id.Value, accountCreated.Username),
                                    async failure =>
                                    {
                                        switch (failure)
                                        {
                                            case CreateCommandFailure.Conflict:
                                                {
                                                    logger.LogInformation("An account {Username} already exists. Performing update.", account.Username);
                                                    GetOneAccountInfoByUsernameQuery request = new(command.Data.Username);
                                                    Option<AccountInfo> accountMayExist = await mediator.Send(request)
                                                                                                        .ConfigureAwait(false);

                                                    await accountMayExist.Match(async existingAccount =>
                                                    {
                                                        logger.LogInformation("Deleting previous {Username} account", existingAccount.Username);
                                                        DeleteAccountInfoByIdCommand deleteAccountCmd = new(existingAccount.Id);
                                                        await mediator.Send(command).ConfigureAwait(false);
                                                        logger.LogTrace("Deleting previous {Username} account succeed", existingAccount.Username);
                                                        logger.LogTrace("Creating account");
                                                        await mediator.Send(command).ConfigureAwait(false);
                                                        logger.LogTrace("Creating account succeeded");
                                                        logger.LogInformation("Account {AccountId} for {Username} successfully updated", existingAccount.Id, accountInfo.Username);
                                                    },
                                                    () => Task.CompletedTask);
                                                }
                                                break;
                                            default:
                                                logger.LogCritical("Could not create account for {Username} (Reason : {FailureReason})", account.Username, failure);
                                                break;
                                        }
                                    });
                   });

                   logger.LogInformation("Accounts created/updated successfully");
               })
    {
        
    }
}
