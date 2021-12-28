namespace Agenda.API
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using System;
    using System.Threading.Tasks;
    using System.Diagnostics;

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

            logger?.LogInformation("Starting {ApplicationContext}", env.ApplicationName);

            try
            {
                logger?.LogInformation("Upgrading {ApplicationContext}'s store", env.ApplicationName);
                await host.InitAsync().ConfigureAwait(false);
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
                .ConfigureAppConfiguration((_, builder) =>

                    builder
                        .AddEnvironmentVariables()
                        .AddCommandLine(args)
                );
    }
}
