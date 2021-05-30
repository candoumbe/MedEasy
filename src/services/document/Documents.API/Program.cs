namespace Documents.API
{
    using Documents.DataStore;

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
    using System.Data.Common;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            IHost host = CreateHostBuilder(args).Build();

            using IServiceScope scope = host.Services.CreateScope();
            IServiceProvider services = scope.ServiceProvider;
            ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
            DocumentsStore context = services.GetRequiredService<DocumentsStore>();
            IHostEnvironment hostingEnvironment = services.GetRequiredService<IHostEnvironment>();
            logger?.LogInformation("Starting {ApplicationContext}", hostingEnvironment.ApplicationName);

            try
            {
                logger?.LogInformation("Upgrading {ApplicationContext}'s store", hostingEnvironment.ApplicationName);
                // Forces database migrations on startup
                RetryPolicy policy = Policy
                    .Handle<DbException>()
                    .WaitAndRetryAsync(
                        retryCount: 5,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (exception, _, attempt, __) =>
                            logger?.LogError(exception, "Error while upgrading database (Attempt {Attempt})", attempt)
                        );
                logger?.LogInformation("Starting {ApplicationContext} database migration", hostingEnvironment.ApplicationName);

                // Forces datastore migration on startup
                await policy.ExecuteAsync(async () => await context.Database.MigrateAsync().ConfigureAwait(false))
                    .ConfigureAwait(false);

                logger?.LogInformation("{ApplicationContext} store updated", hostingEnvironment.ApplicationName);

                await host.RunAsync()
                    .ConfigureAwait(false);

                logger?.LogInformation($"Identity.API started");
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
