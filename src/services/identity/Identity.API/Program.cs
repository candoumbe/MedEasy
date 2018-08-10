using Identity.DataStores.SqlServer;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Identity.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IWebHost host = BuildWebHost(args);
            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
                IdentityContext context = services.GetRequiredService<IdentityContext>();
                logger?.LogInformation($"Starting Identity.API");

                try
                {
                    if (!context.Database.IsInMemory())
                    {
                        logger?.LogInformation($"Upgrading Identity store");
                        // Forces database migrations on startup
                        Policy
                            .Handle<SqlException>(sql => sql.Message.Like("*Login failed*", ignoreCase : true))
                            .WaitAndRetry(
                                retryCount: 5,
                                sleepDurationProvider: (retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))),
                                onRetry: (exception, timeSpan, attempt, pollyContext) =>
                                {
                                    logger?.LogInformation($"Upgrading database (Attempt {attempt}/{pollyContext.Count})");
                                    if (exception != default)
                                    {
                                        logger?.LogError(exception, "Error while upgrading database");
                                    }

                                    // Forces database migrations on startup
                                    context.Database.Migrate();
                                });
                        logger?.LogInformation($"Database updated");

                    }
                    await host.RunAsync()
                        .ConfigureAwait(false);

                    logger?.LogInformation($"Identity.API started");
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "An error occurred on startup.");
                }
            }
        }

        /// <summary>
        /// Builds the host
        /// </summary>
        /// <param name="args">command line arguments</param>
        /// <returns></returns>
        public static IWebHost BuildWebHost(string[] args)
        {
            IWebHost host = WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((context, builder) =>

                    builder
                        .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args)
                )
                .Build();


            return host;
        }
    }
}
