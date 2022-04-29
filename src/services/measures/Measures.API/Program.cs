namespace Measures.API
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Serilog;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Host's entry point
    /// </summary>
    public class Program
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
            IHostEnvironment environment = services.GetRequiredService<IHostEnvironment>();

            logger?.LogInformation("Starting {ApplicationContext}", environment.ApplicationName);

            try
            {
                logger?.LogInformation("Upgrading {ApplicationContext}' store", environment.ApplicationName);

                await host.InitAsync().ConfigureAwait(false);

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
        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                    .UseSerilog((hosting, loggerConfig) => loggerConfig
                        .MinimumLevel.Verbose()
                        .Enrich.WithProperty("ApplicationContext", hosting.HostingEnvironment.ApplicationName)
                        .Enrich.FromLogContext()
                        .Enrich.WithCorrelationIdHeader()
                        .WriteTo.Console()
                        .ReadFrom.Configuration(hosting.Configuration)
                    )
                    .ConfigureWebHostDefaults(webHost => webHost.UseStartup<Startup>()
                                                                .UseKestrel((hosting, options) => options.AddServerHeader = hosting.HostingEnvironment.IsDevelopment())
                    )
                    .ConfigureLogging((options) =>
                    {
                        options.ClearProviders() // removes all default providers
                                .AddSerilog()
                                .AddConsole();
                                                                            });
    }
}
