using Measures.Context;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Measures.API
{
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
            IWebHost host = CreateWebHostBuilder(args)
                .Build();

            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
                MeasuresContext context = services.GetRequiredService<MeasuresContext>();
                logger?.LogInformation($"Starting Measures.API");

                try
                {
                    if (!context.Database.IsInMemory())
                    {
                        logger?.LogInformation($"Upgrading Measures store");
                        // Forces database migrations on startup
                        RetryPolicy policy = Policy
                            .Handle<SqlException>(sql => sql.Message.Like("*Login failed*", ignoreCase: true))
                            .WaitAndRetryAsync(
                                retryCount: 5,
                                sleepDurationProvider: (retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))),
                                onRetry: (exception, timeSpan, attempt, pollyContext) =>
                                    logger?.LogError(exception, $"Error while upgrading database (Attempt {attempt}/{pollyContext.Count})")
                                );
                        logger?.LogInformation("Starting measures database migration");

                        // Forces datastore migration on startup
                        await policy.ExecuteAsync(async () => await context.Database.MigrateAsync().ConfigureAwait(false))
                            .ConfigureAwait(false);

                        logger?.LogInformation($"Measures database updated");
                    }
                    await host.RunAsync()
                        .ConfigureAwait(false);

                    logger?.LogInformation($"Measures.API started");
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "An error occurred on startup.");
                }
            }
        }

        /// <summary>
        /// Configures the host
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args)
               .UseStartup<Startup>()
               .ConfigureAppConfiguration((context, builder) =>

                   builder
                       .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
                       .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                       .AddEnvironmentVariables()
                       .AddCommandLine(args)
               );
    }
}
