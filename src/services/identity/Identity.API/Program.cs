namespace Identity.API;

using Destructurama;

using MedEasy.ValueObjects;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Optional;

using Serilog;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// Entry point of the application
/// </summary>
public class Program
{
    /// <summary>
    /// Main method of the program
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static async Task Main(string[] args)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;

        IHost host = CreateHostBuilder(args).Build();

        using IServiceScope scope = host.Services.CreateScope();
        IServiceProvider services = scope.ServiceProvider;
        ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
        IHostEnvironment hostingEnvironment = services.GetRequiredService<IHostEnvironment>();
        logger?.LogInformation("Starting {ApplicationContext}", hostingEnvironment.ApplicationName);

        try
        {
            logger?.LogInformation("Upgrading {ApplicationContext}'s store", hostingEnvironment.ApplicationName);

            await host.InitAsync().ConfigureAwait(false);

            logger?.LogInformation("Identity database updated");

            await host.RunAsync()
                .ConfigureAwait(false);

            logger?.LogInformation("Identity.API started");
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
    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
                .UseSerilog((hosting, loggerConfig) =>
                {
                    loggerConfig = loggerConfig.MinimumLevel.Verbose()
                                               //.Destructure.ByIgnoringProperties<CQRS.Commands.v1.LoginCommand>(c => c.Data.LoginInfos.Password)
                                               //.Destructure.ByIgnoringProperties<CQRS.Commands.v2.LoginCommand>(c => c.Data.LoginInfos.Password)
                                               //.Destructure.ByIgnoringProperties<RefreshAccessTokenByUsernameCommand>(c => c.Data.refreshToken)
                                               .Destructure.ByIgnoringProperties<SystemAccount>(c => c.Password)
                                               .Destructure.ByIgnoringProperties<Password>(c => c.Value)
                                               .Destructure.ByIgnoringProperties<DTO.v1.BearerTokenInfo>(c => c.RefreshToken)
                                               .Destructure.ByIgnoringProperties<DTO.v2.BearerTokenInfo>(c => c.RefreshToken)
                                               .Enrich.WithProperty("ApplicationContext", hosting.HostingEnvironment.ApplicationName)
                                               .Enrich.FromLogContext()
                                               .Enrich.WithCorrelationIdHeader()
                                               .ReadFrom.Configuration(hosting.Configuration);

                    hosting.Configuration.GetServiceUri("seq")
                                        .SomeNotNull()
                                        .MatchSome(seqUri => loggerConfig.WriteTo.Seq(seqUri.AbsoluteUri));
                })
                .ConfigureWebHostDefaults(webHost => webHost.UseStartup<Startup>()
                                                            .UseKestrel((hosting, options) => options.AddServerHeader = hosting.HostingEnvironment.IsDevelopment())
                )
                .ConfigureLogging((context, options) =>
                {
                    options.ClearProviders() // removes all default providers
                       .AddSerilog();

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        options.AddConsole();
                    }
                })
                .ConfigureAppConfiguration((_, config) => config.AddJsonFile("accounts.json", optional: false, reloadOnChange: true))
        ;
}
