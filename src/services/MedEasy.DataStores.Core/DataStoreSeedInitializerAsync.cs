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
using Extensions.Hosting.AsyncInitialization;

/// <summary>
/// Helper class to seed <typeparamref name="TDataStore"/> asynchronously.
/// </summary>
/// <typeparam name="TDataStore">Type of the <see cref="DbContext"/>to seed</typeparam>
public class DataStoreSeedInitializerAsync<TDataStore> : IAsyncInitializer
    where TDataStore : DbContext
{
    private readonly Func<TDataStore> _storeFactory;
    private readonly Func<TDataStore, Task> _seeder;
#if NET6_0_OR_GREATER
    private readonly IHostEnvironment _hostingEnvironment; 
#else
    private readonly IHostingEnvironment _hostingEnvironment; 
#endif
    private readonly ILogger<DataStoreSeedInitializerAsync<TDataStore>> _logger;

    /// <summary>
    /// Builds a new <see cref="DataStoreSeedInitializerAsync{TDataStore}"/> instance.
    /// </summary>
    /// <param name="hostingEnvironment"></param>
    /// <param name="logger"></param>
    /// <param name="dataStoreFactory"></param>
#if NET6_0_OR_GREATER
    public DataStoreSeedInitializerAsync(IHostEnvironment hostingEnvironment, ILogger<DataStoreSeedInitializerAsync<TDataStore>> logger, Func<TDataStore> dataStoreFactory, Func<TDataStore, Task> seeder) 
#else
    public DataStoreSeedInitializerAsync(IHostingEnvironment hostingEnvironment, ILogger<DataStoreSeedInitializerAsync<TDataStore>> logger, Func<TDataStore> dataStoreFactory, Func<TDataStore, Task> seeder) 
#endif
    {
        _hostingEnvironment = hostingEnvironment;
        _logger = logger;
        _storeFactory = dataStoreFactory;
        _seeder = seeder;
    }

    ///<inheritdoc/>
    public async Task InitializeAsync()
    {
        try
        {
            TDataStore store = _storeFactory.Invoke();
            _logger?.LogInformation("Seeder {ApplicationContext}'s store", _hostingEnvironment.ApplicationName);
            _logger?.LogInformation("Connection string : {ConnectionString}", store.Database.GetConnectionString());
            // Forces database migrations on startup
            AsyncRetryPolicy policy = Policy
                .Handle<DbException>()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (exception, timeSpan, attempt, pollyContext) =>

                        _logger?.LogError(exception, "Error while seeding (Attempt {Attempt})", attempt)
);
            _logger?.LogInformation("Seeding {ApplicationContext} database", _hostingEnvironment.ApplicationName);

            await policy.ExecuteAsync(async () => await _seeder.Invoke(store).ConfigureAwait(false))
                        .ConfigureAwait(false);

            _logger?.LogInformation("{ApplicationContext} database seeded", _hostingEnvironment.ApplicationName);
        }
        catch (Exception)
        {
            _logger?.LogError("Error occured when seeding {ApplicationContext}'s store", _hostingEnvironment.ApplicationName);
            throw;
        }
    }
}
