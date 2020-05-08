using Agenda.DataStores;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Serilog;
using System;
using Npgsql;
using System.Threading.Tasks;
namespace Agenda.API
{
#pragma warning disable RCS1102 // Make class static.
    public class Program
#pragma warning restore RCS1102 // Make class static.
    {
        public static async Task Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();

            using IServiceScope scope = host.Services.CreateScope();
            IServiceProvider services = scope.ServiceProvider;
            ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
            IHostEnvironment env = services.GetRequiredService<IHostEnvironment>();
            AgendaContext context = services.GetRequiredService<AgendaContext>();
            logger?.LogInformation("Starting {ApplicationContext}", env.ApplicationName);

            try
            {
                if (!context.Database.IsInMemory())
                {
                    logger?.LogInformation("Upgrading {ApplicationContext}'s store", env.ApplicationName);
                    // Forces database migrations on startup
                    RetryPolicy policy = Policy
                        .Handle<NpgsqlException>(sql => sql.Message.Like("*failed*", ignoreCase: true))
                        .WaitAndRetryAsync(
                            retryCount: 5,
                            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                            onRetry: (exception, _, attempt, __) =>
                                logger?.LogError(exception, $"Error while upgrading database (Attempt {attempt})")
                            );
                    logger?.LogInformation("Starting {ApplictationContext} database migration", env.ApplicationName);

                    // Forces datastore migration on startup
                    await policy.ExecuteAsync(async () => await context.Database.MigrateAsync().ConfigureAwait(false))
                        .ConfigureAwait(false);

                    logger?.LogInformation("{ApplicationContext} updated", env.ApplicationName);
                }
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
