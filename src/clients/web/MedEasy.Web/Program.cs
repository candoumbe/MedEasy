namespace MedEasy.Web
{
    using Blazorise;
    using Blazorise.Bootstrap;
    using Blazorise.Icons.FontAwesome;

    using MedEasy.Web.Apis.Identity.Interfaces;

    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    using Refit;

    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

            Uri baseAddress = new(builder.HostEnvironment.BaseAddress);

            builder.Services
                  .AddBlazorise(options =>
                  {
                      options.ChangeTextOnKeyPress = true;
                  })
                  .AddBootstrapProviders()
                  .AddFontAwesomeIcons();

            builder.Services.AddRefitClient<IIdentityApi>().ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(baseAddress, "/api/identity");
            });
            
            builder.Services.AddScoped(sp =>
            {
                return new HttpClient { BaseAddress = baseAddress };
            });

            builder.RootComponents.Add<App>("#app");
            await builder.Build().RunAsync();
        }
    }
}
