using Identity.DataStores.SqlServer;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Serilog;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Identity.API
{
#pragma warning disable RCS1102 // Make class static.
    public class Program
#pragma warning restore RCS1102 // Make class static.
    {
        private static string _appName = typeof(Program).Namespace;

        public static async Task Main(string[] args)
        {
            IWebHost host = CreateWebHostBuilder(args).Build();
            
            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
                IdentityContext context = services.GetRequiredService<IdentityContext>();
                logger?.LogInformation("Starting {{ApplicationContext}}", _appName);

                try
                {
                    if (!context.Database.IsInMemory())
                    {
                        logger?.LogInformation("Upgrading {{ApplicationContext}}'s store", _appName);
                        // Forces database migrations on startup
                        RetryPolicy policy = Policy
                            .Handle<SqlException>(sql => sql.Message.Like("*Login failed*", ignoreCase: true))
                            .WaitAndRetryAsync(
                                retryCount: 5,
                                sleepDurationProvider: (retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))),
                                onRetry: (exception, timeSpan, attempt, pollyContext) =>
                                    logger?.LogError(exception, $"Error while upgrading database (Attempt {attempt})")
                                );
                        logger?.LogInformation("Starting {{ApplictationContext}} database migration", _appName);

                        // Forces datastore migration on startup
                        await policy.ExecuteAsync(async () => await context.Database.MigrateAsync().ConfigureAwait(false))
                            .ConfigureAwait(false);

                        logger?.LogInformation($"Identity database updated");
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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
            => WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog((hosting, loggerConfig) => loggerConfig
                    .MinimumLevel.Verbose()
                    .Enrich.WithProperty("ApplicationContext", _appName)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .ReadFrom.Configuration(hosting.Configuration)
                )
                .ConfigureAppConfiguration((context, builder) =>

                    builder
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args)
                );
    }
}
