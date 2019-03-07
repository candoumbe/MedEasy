using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading.Tasks;

namespace MedEasy.Gateway
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IWebHost host = CreateWebHostBuilder(args).Build();


            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                ILogger<Program> logger = services.GetRequiredService<ILogger<Program>>();
                IHostingEnvironment hostingEnvironment = services.GetRequiredService<IHostingEnvironment>();
                logger?.LogInformation("Starting {ApplicationContext}", hostingEnvironment.ApplicationName);

                await host.RunAsync()
                    .ConfigureAwait(false);

                logger?.LogInformation("{ApplicationContext} started", hostingEnvironment.ApplicationName);
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseSerilog((hosting, loggerConfig) => loggerConfig
                    .MinimumLevel.Verbose()
                    .Enrich.WithProperty("ApplicationContext", hosting.HostingEnvironment.ApplicationName)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .ReadFrom.Configuration(hosting.Configuration)
                )
                .ConfigureAppConfiguration((context, configurationBuilder) =>
                    configurationBuilder
                        .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile("configuration.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appSettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"configuration.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args)
                )
                .UseStartup<Startup>();
    }
}
