namespace Agenda.API
{
    using Agenda.DataStores;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Linq;
    using System.Data.Common;

    /// <summary>
    /// Entry class
    /// </summary>
#pragma warning disable RCS1102 // Make class static.
    public class Program
#pragma warning restore RCS1102 // Make class static.
    {
        public static async Task Main(string[] args)
        {

            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            IHost host = CreateHostBuilder(args).Build();

            using IServiceScope scope = host.Services.CreateScope();
            IServiceProvider services = scope.ServiceProvider;
            ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
            IHostEnvironment env = services.GetRequiredService<IHostEnvironment>();
            AgendaContext context = services.GetRequiredService<AgendaContext>();

            logger?.LogInformation("Starting {ApplicationContext}", env.ApplicationName);

            try
            {
                logger?.LogInformation("Upgrading {ApplicationContext}'s store", env.ApplicationName);
                // Forces database migrations on startup
                PolicyBuilder policy = Policy.Handle<DbException>();

                logger?.LogInformation("Starting {ApplictationContext} database migration", env.ApplicationName);
                string[] migrations = (await context.Database.GetPendingMigrationsAsync().ConfigureAwait(false))
                                                                     .ToArray();
                logger?.LogDebug("{MigrationCount} of pending migrations for {ApplicationContext}", migrations.Length, env.ApplicationName);
                foreach (string migrationName in migrations)
                {
                    logger?.LogInformation("Migration : {MigrationName}", migrationName);
                }

                await policy.WaitAndRetryAsync(retryCount: 5,
                                               sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                                               onRetry: (exception, _, attempt, __) => logger?.LogError(exception, $"Error while upgrading database (Attempt {attempt})"))
                            .ExecuteAsync(async () =>
                            {
                                await context.Database.MigrateAsync().ConfigureAwait(false);
                            })
                            .ConfigureAwait(false);

                logger?.LogInformation("{ApplicationContext} updated", env.ApplicationName);

                await host.RunAsync()
                    .ConfigureAwait(false);

                logger?.LogInformation("{ApplicationContext} started", env.ApplicationName);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred on startup.");
            }
        }

        /// <summary>
        /// Builds the host
        /// </summary>
        /// <param name="args">command line arguments</param>
        /// <returns></returns>

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webHost => webHost.UseStartup<Startup>()
                                                            .UseKestrel((hosting, options) => options.AddServerHeader = hosting.HostingEnvironment.IsDevelopment())
                                                            .UseSerilog((hosting, loggerConfig) => loggerConfig
                                                                .MinimumLevel.Verbose()
                                                                .Enrich.WithProperty("ApplicationContext", hosting.HostingEnvironment.ApplicationName)
                                                                .Enrich.FromLogContext()
                                                                .WriteTo.Console()
                                                                .ReadFrom.Configuration(hosting.Configuration)
                                                            )
                )
                .ConfigureLogging((options) =>
                {
                    options.ClearProviders() // removes all default providers
                        .AddSerilog()
                        .AddConsole();
                })
                .ConfigureAppConfiguration((context, builder) =>

                    builder
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args)
                );
    }
}
