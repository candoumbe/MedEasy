namespace MedEasy.ReverseProxy
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Serilog;

    using System.Diagnostics;

    using Optional;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            IHost host = CreateHostBuilder(args).Build();

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((options) =>
                {
                    options.ClearProviders() // removes all default providers
                        .AddSerilog()
                        .AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                              .UseKestrel((hosting, options) =>
                              {
                                  options.AddServerHeader = hosting.HostingEnvironment.IsDevelopment();
                              })
                              .UseSerilog((hosting, loggerConfig) =>
                              {
                                  loggerConfig = loggerConfig
                                      .MinimumLevel.Verbose()
                                      .Enrich.WithProperty("ApplicationContext", hosting.HostingEnvironment.ApplicationName)
                                      .Enrich.FromLogContext()
                                      .WriteTo.Console()
                                      .ReadFrom.Configuration(hosting.Configuration);

                                  hosting.Configuration.GetServiceUri("seq" )
                                                      .SomeNotNull()
                                                      .MatchSome(seqUri => loggerConfig.WriteTo.Seq(seqUri.AbsoluteUri));
                              });
                });

    }
}
