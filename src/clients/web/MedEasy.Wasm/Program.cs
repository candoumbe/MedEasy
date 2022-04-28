using Blazored.LocalStorage;
using Blazored.SessionStorage;

using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

using MedEasy.Wasm;
using MedEasy.Wasm.Services;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using NodaTime;

using Refit;

using System.IdentityModel.Tokens.Jwt;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

_ = new JwtHeader();

_ = new JwtPayload();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<AuthenticationStateProvider, MedEasyAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<MedEasyAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStore>();
builder.Services.AddScoped<AuthorizationHeaderHandler>();
builder.Services.AddScoped<IClock>(_ => SystemClock.Instance);

builder.Services.AddLogging();

builder.Services.AddRefitClient<MedEasy.Wasm.Apis.Identity.v1.IIdentityApi>()
                .ConfigureHttpClient(client =>
                {
                    string proxyUrl = builder.Configuration.GetValue<string>("ProxyUrl").TrimEnd('/');
                    client.BaseAddress =  new Uri($"{proxyUrl}/api/identity");
                });
builder.Services.AddRefitClient<MedEasy.Wasm.Apis.Identity.v2.IIdentityApi>()
                .ConfigureHttpClient(client =>
                {
                    string proxyUrl = builder.Configuration.GetValue<string>("ProxyUrl").TrimEnd('/');
                    client.BaseAddress = new Uri($"{proxyUrl}/api/identity");
                });

builder.Services.AddRefitClient<MedEasy.Wasm.Apis.Agenda.v1.IAgendaApi>()
                .ConfigureHttpClient(client =>
                {
                    string proxyUrl = builder.Configuration.GetValue<string>("ProxyUrl").TrimEnd('/');
                    client.BaseAddress = new Uri($"{proxyUrl}/api/agenda");
                })
                .AddHttpMessageHandler<AuthorizationHeaderHandler>();

builder.Services.AddBlazorise(options =>
                {
                    options.Immediate = true;
                })
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();

await builder.Build().RunAsync();
