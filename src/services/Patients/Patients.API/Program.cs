using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Patients.Context;
using System;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Patients.API
{
    public class Program
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

            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
                PatientsContext context = services.GetRequiredService<PatientsContext>();
                logger?.LogInformation($"Starting Patients.API");

                try
                {
                    if (!context.Database.IsInMemory())
                    {
                        logger?.LogInformation("Upgrading Patients store");
                        // Forces database migrations on startup
                        RetryPolicy policy = Policy
                            .Handle<SqlException>(sql => sql.Message.Like("*Login failed*", ignoreCase: true))
                            .WaitAndRetryAsync(
                                retryCount: 5,
                                sleepDurationProvider: (retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))),
                                onRetry: (exception, timeSpan, attempt, pollyContext) =>
                                    logger?.LogError(exception, $"Error while upgrading database (Attempt {attempt}/{pollyContext.Count})")
                                );
                        logger?.LogInformation("Starting patients database migration");

                        // Forces datastore migration on startup
                        await policy.ExecuteAsync(async () => await context.Database.MigrateAsync().ConfigureAwait(false))
                            .ConfigureAwait(false);

                        logger?.LogInformation($"Patients database updated");
                    }
                    await host.RunAsync()
                        .ConfigureAwait(false);

                    logger?.LogInformation($"Patients.API started");
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "An error occurred when starting Patients.API");
                }
            }
        }

#if NETCOREAPP2_1
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
#endif
    }
}
