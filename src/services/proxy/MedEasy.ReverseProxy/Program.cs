namespace MedEasy.ReverseProxy
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    using Optional;

    using Serilog;

    using System.Diagnostics;
    using System.Threading.Tasks;

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
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                              .UseKestrel((hosting, options) =>
                              {
                                  options.AddServerHeader = hosting.HostingEnvironment.IsDevelopment();
                              });
                });
    }
}
