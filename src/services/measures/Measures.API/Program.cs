namespace Measures.API
{
    using Measures.DataStores;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Npgsql;

    using Polly;
    using Polly.Retry;

    using Serilog;

    using System;
    using System.Threading.Tasks;

#pragma warning disable RCS1102 // Make class static.
    /// <summary>
    /// Host's entry point
    /// </summary>
    public class Program
#pragma warning restore RCS1102 // Make class static.
    {
        /// <summary>
        /// Host's entry point
        /// <param name="args">Command line arguments</param>
        /// </summary>
        public static async Task Main(string[] args)
        {
            IHost host = CreateHostBuilder(args)
                .Build();

            using IServiceScope scope = host.Services.CreateScope();

            IServiceProvider services = scope.ServiceProvider;
            ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
            MeasuresStore context = services.GetRequiredService<MeasuresStore>();
            IHostEnvironment environment = services.GetRequiredService<IHostEnvironment>();

            logger?.LogInformation("Starting {ApplicationContext}", environment.ApplicationName);

            try
            {

                logger?.LogInformation("Upgrading {ApplicationContext}' store", environment.ApplicationName);
                // Forces database migrations on startup
                RetryPolicy policy = Policy
                    .Handle<NpgsqlException>(sql => sql.Message.Like("*failed*", ignoreCase: true))
                    .WaitAndRetryAsync(
                        retryCount: 5,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (exception, timeSpan, attempt, pollyContext) =>
                            logger?.LogError(exception, $"Error while upgrading database (Attempt {attempt}/{pollyContext.Count})")
                        );
                logger?.LogInformation("Starting {ApplicationContext}' store migration", environment.ApplicationName);

                // Forces datastore migration on startup
                await policy.ExecuteAsync(async () => await context.Database.MigrateAsync().ConfigureAwait(false))
                    .ConfigureAwait(false);

                logger?.LogInformation("{ApplicationContext} store upgraded", environment.ApplicationName);

                await host.RunAsync()
                    .ConfigureAwait(false);

                logger?.LogInformation("{ApplicationContext} started", environment.ApplicationName);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred on startup.");
            }
        }

        /// <summary>
        /// Configures the host
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
                                                                           .ConfigureWebHostDefaults(webHost => webHost.UseStartup<Startup>()
                                                                                                                       .UseKestrel((hosting, options) => options.AddServerHeader = hosting.HostingEnvironment.IsDevelopment())
                                                                                                                       .UseSerilog((hosting, loggerConfig) => loggerConfig
                                                                                                                            .MinimumLevel.Verbose()
                                                                                                                            .Enrich.WithProperty("ApplicationContext", hosting.HostingEnvironment.ApplicationName)
                                                                                                                            .Enrich.FromLogContext()
                                                                                                                            .WriteTo.Console()
                                                                                                                            .ReadFrom.Configuration(hosting.Configuration)
                                                                                                                        ))
                                                                            .ConfigureLogging((options) =>
                                                                            {
                                                                                options.ClearProviders() // removes all default providers
                                                                                       .AddSerilog()
                                                                                       .AddConsole();
                                                                            });
    }
}
