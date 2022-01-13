namespace MedEasy.DataStores.Core;

using MedEasy.AspnetCore.AsyncInitializers;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.Retry;

using Polly;

using System.Data.Common;
using System.Threading.Tasks;
using System;

/// <summary>
/// Helper class to perform <typeparamref name="TDataStore"/> asynchronously
/// </summary>
/// <typeparam name="TDataStore">Type of the <see cref="DbContext"/>to migrate</typeparam>
public class DataStoreMigrateInitializerAsync<TDataStore> : AsyncBaseIntializer
    where TDataStore : DbContext
{
    private readonly TDataStore _store;
#if NET6_0_OR_GREATER
    private readonly IHostEnvironment _hostingEnvironment; 
#else
    private readonly IHostingEnvironment _hostingEnvironment; 
#endif
    private readonly ILogger<DataStoreMigrateInitializerAsync<TDataStore>> _logger;

    /// <summary>
    /// Builds a new <see cref="DataStoreMigrateInitializerAsync{TDataStore}"/> instance.
    /// </summary>
    /// <param name="hostingEnvironment"></param>
    /// <param name="logger"></param>
    /// <param name="dataStore"></param>
#if NET6_0_OR_GREATER
    public DataStoreMigrateInitializerAsync(IHostEnvironment hostingEnvironment, ILogger<DataStoreMigrateInitializerAsync<TDataStore>> logger, TDataStore dataStore) 
#else
    public DataStoreMigrateInitializerAsync(IHostingEnvironment hostingEnvironment, ILogger<DataStoreMigrateInitializerAsync<TDataStore>> logger, TDataStore dataStore) 
#endif
    {
        _hostingEnvironment = hostingEnvironment;
        _logger = logger;
        _store = dataStore;
    }

    ///<inheritdoc/>
    public override async Task InitializeAsync()
    {
        try
        {
            _logger?.LogInformation("Upgrading {ApplicationContext}'s store", _hostingEnvironment.ApplicationName);
            _logger?.LogInformation("Connection string : {ConnectionString}", _store.Database.GetConnectionString());
            // Forces database migrations on startup
            RetryPolicy policy = Policy
                .Handle<DbException>()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, attempt, pollyContext) =>

                        _logger?.LogError(exception, "Error while upgrading database schema (Attempt {Attempt})", attempt)
);
            _logger?.LogInformation("Starting {ApplicationContext} database migration", _hostingEnvironment.ApplicationName);

            // Forces datastore migration on startup
            await policy.ExecuteAsync(async () => await _store.Database.MigrateAsync().ConfigureAwait(false))
                        .ConfigureAwait(false);

            _logger?.LogInformation("Identity database updated");
        }
        catch (System.Exception)
        {
            _logger?.LogError("Error");
            throw;
        }
    }
}
