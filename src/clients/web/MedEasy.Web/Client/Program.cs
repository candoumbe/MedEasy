using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MedEasy.Web.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services
                .AddBlazorise(options =>
                  {
                      options.ChangeTextOnKeyPress = true;
                  })
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();

            builder.Services.AddBlazoredLocalStorage();


            builder.Services.AddTransient(sp => new HttpClient(sp.GetRequiredService<AuthorizationMessageHandler>()
                .ConfigureHandler(
                    authorizedUrls: new[]
                    {
                        "https://identity.api",
                        "https://measures.api",
                        "https://agenda.api",
                    }
                )
                ) { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            WebAssemblyHost host = builder.Build();
            host.Services
              .UseBootstrapProviders()
              .UseFontAwesomeIcons();

            await host.RunAsync();
        }
    }
}
