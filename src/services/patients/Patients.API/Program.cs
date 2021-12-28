namespace Patients.API
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
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            IHost host = CreateHostBuilder(args)
                .Build();

            using IServiceScope scope = host.Services.CreateScope();
            IServiceProvider services = scope.ServiceProvider;
            ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
            IHostEnvironment environment = services.GetRequiredService<IHostEnvironment>();

            logger?.LogInformation("Starting {ApplicationContext}", environment.ApplicationName);

            try
            {
                logger?.LogInformation("Upgrading {ApplicationContext} store", environment.ApplicationName);

                await host.InitAsync().ConfigureAwait(false);

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
        /// Builds the host
        /// </summary>
        /// <param name="args">command line arguments</param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
                .ConfigureDefaults(args)
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
                });
    }
}