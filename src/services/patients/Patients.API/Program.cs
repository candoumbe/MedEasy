using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Patients.Context;

using Polly;
using Polly.Retry;

using Serilog;

using System;
using System.Threading.Tasks;
using Npgsql;

namespace Patients.API
{
#pragma warning disable CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
    public class Program
#pragma warning restore CS1591 // Commentaire XML manquant pour le type ou le membre visible publiquement
    {
        /// <summary>
        /// Host's entry point
        /// <param name="args">Command line arguments</param>
        /// </summary>
        public static async Task Main(string[] args)
        {
            IWebHost host =
                CreateWebHostBuilder(args)
                .Build();

            using IServiceScope scope = host.Services.CreateScope();
            IServiceProvider services = scope.ServiceProvider;
            ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
            IHostEnvironment environment = services.GetRequiredService<IHostEnvironment>();
            PatientsContext context = services.GetRequiredService<PatientsContext>();

            logger?.LogInformation("Starting {ApplicationContext}", environment.ApplicationName);

            try
            {
                if (!context.Database.IsInMemory())
                {
                    logger?.LogInformation("Upgrading {ApplicationContext} store", environment.ApplicationName);
                    // Forces database migrations on startup
                    RetryPolicy policy = Policy
                        .Handle<NpgsqlException>(sql => sql.Message.Like("*failed*", ignoreCase: true))
                        .WaitAndRetryAsync(
                            retryCount: 5,
                            sleepDurationProvider: (retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))),
                            onRetry: (exception, timeSpan, attempt, pollyContext) =>
                                logger?.LogError(exception, $"Error while upgrading database (Attempt {attempt}/{pollyContext.Count})")
                            );
                    logger?.LogInformation("Starting {ApplicationContext} migration", environment.ApplicationName);

                    // Forces datastore migration on startup
                    await policy.ExecuteAsync(async () => await context.Database.MigrateAsync().ConfigureAwait(false))
                        .ConfigureAwait(false);

                    logger?.LogInformation("{ApplicationContext} store updated", environment.ApplicationName);
                }
                await host.RunAsync()
                    .ConfigureAwait(false);

                logger?.LogInformation("{ApplicationContext} started", environment.ApplicationName);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred when starting Patients.API");
            }
        }

        /// <summary>
        /// Configures the host
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args)
               .UseStartup<Startup>()
               .UseKestrel((hosting, options) => options.AddServerHeader = hosting.HostingEnvironment.IsDevelopment())
                .UseSerilog((hosting, loggerConfig) => loggerConfig
                    .MinimumLevel.Verbose()
                    .Enrich.WithProperty("ApplicationContext", hosting.HostingEnvironment.ApplicationName)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .ReadFrom.Configuration(hosting.Configuration)
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
